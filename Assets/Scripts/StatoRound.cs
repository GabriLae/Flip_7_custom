using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Stato della modalità speciale "PescaTreCarte" (Flip Three).
/// - Nessuno: nessun giocatore è in modalità PescaTreCarte.
/// - InCorso: il giocatore corrente è costretto a pescare 3 carte di fila.
/// </summary>
public enum StatoPescaTreCarte
{
    /// <summary>Nessun giocatore sta pescando 3 carte forzate.</summary>
    Nessuno,

    /// <summary>Il giocatore corrente è in modalità PescaTreCarte.</summary>
    InCorso
}

/// <summary>
/// Tipo di azione in attesa di selezione del bersaglio.
/// Quando un giocatore pesca Congela o Pesca 3, deve scegliere il bersaglio.
/// </summary>
public enum TipoAzioneInAttesa
{
    /// <summary>Nessuna azione in attesa — gioco normale.</summary>
    Nessuna,

    /// <summary>Il giocatore deve scegliere chi congelare.</summary>
    ScegliBersaglioCongela,

    /// <summary>Il giocatore deve scegliere su chi applicare Pesca 3 (PescaTreCarte).</summary>
    ScegliBersaglioPescaTreCarte,
}

/// <summary>
/// STATO DEL ROUND — Gestisce l'intero turno di gioco:
/// - Turni dei giocatori (chi è di turno, passaggio al prossimo)
/// - Pescate e azioni speciali (Congela, PescaTreCarte, SecondaChance)
/// - Condizioni di fine round (Flip7 attivato, nessun giocatore attivo)
/// - Azioni differite durante PescaTreCarte
/// 
/// Coordina il Mazzo (da cui pescare) e gli array di StatoRoundGiocatore.
/// </summary>
public class StatoRound
{
    // ═══════════════════════════════════════════════════════════════════════
    //  DIPENDENZE
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>Mazzo da cui pescare le carte.</summary>
    private Mazzo _mazzo;

    /// <summary>Stato individuale di ogni giocatore per questo round.</summary>
    private StatoRoundGiocatore[] _giocatori;

    // ═══════════════════════════════════════════════════════════════════════
    //  STATO DEL TURNO
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>Indice del giocatore attualmente di turno (0-based).</summary>
    public int GiocatoreCorrente { get; private set; }

    /// <summary>Numero totale di giocatori in questo round.</summary>
    public int NumeroGiocatori { get; }

    /// <summary>True se qualcuno ha attivato Flip7 (7 numeri unici).</summary>
    public bool Flip7Attivato { get; private set; }

    /// <summary>
    /// Il round finisce quando:
    /// 1. Qualcuno ha fatto Flip 7 (7 numeri unici pescati), oppure
    /// 2. Nessun giocatore è più nello stato "Attivo"
    /// </summary>
    public bool RoundFinito =>
        Flip7Attivato ||
        !_giocatori.Any(g => g.Stato == StatoGiocatore.Attivo);

    // ═══════════════════════════════════════════════════════════════════════
    //  STATO PESCA TRE CARTE
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>Indica se un giocatore è in modalità PescaTreCarte.</summary>
    private StatoPescaTreCarte _statoPescaTreCarte;

    /// <summary>Indice del giocatore in PescaTreCarte.</summary>
    private int _giocatorePescaTreCarte;

    /// <summary>Quante carte ha già pescato durante la modalità (max 3).</summary>
    private int _carteContatorePescaTreCarte;

    /// <summary>
    /// Azioni Congela accumulate durante la PescaTreCarte.
    /// Vengono risolte SOLO dopo aver finito le 3 pescate forzate.
    /// </summary>
    private List<Carta> _azioniDifferite;

    // ═══════════════════════════════════════════════════════════════════════
    //  STATO SELEZIONE BERSAGLIO
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Tipo di azione in attesa di selezione bersaglio.
    /// Se diverso da Nessuna, il gioco è in pausa in attesa di una scelta.
    /// </summary>
    public TipoAzioneInAttesa AzioneInAttesa { get; private set; }

    /// <summary>
    /// Indici dei giocatori validi come bersaglio per l'azione in attesa.
    /// Null se nessuna azione in attesa.
    /// </summary>
    public int[] BersagliDisponibili { get; private set; }

    // ═══════════════════════════════════════════════════════════════════════
    //  COSTRUTTORE
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Crea un nuovo round: tutti i giocatori partono come "Attivi",
    /// il primo giocatore di turno è il numero 0.
    /// </summary>
    /// <param name="mazzo">Il mazzo di carte già mescolato.</param>
    /// <param name="numeroGiocatori">Quanti giocatori partecipano.</param>
    public StatoRound(Mazzo mazzo, int numeroGiocatori)
    {
        _mazzo = mazzo;
        NumeroGiocatori = numeroGiocatori;
        GiocatoreCorrente = 0;
        Flip7Attivato = false;
        AzioneInAttesa = TipoAzioneInAttesa.Nessuna;
        BersagliDisponibili = null;

        _giocatori = new StatoRoundGiocatore[numeroGiocatori];
        for (int i = 0; i < numeroGiocatori; i++)
        {
            _giocatori[i] = new StatoRoundGiocatore();
        }

        _statoPescaTreCarte = StatoPescaTreCarte.Nessuno;
        _azioniDifferite = new List<Carta>();
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  ACCESSO AGLI STATI
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Restituisce lo stato del giocatore all'indice specificato.
    /// </summary>
    public StatoRoundGiocatore OttieniGiocatore(int indice)
    {
        return _giocatori[indice];
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  CONTROLLI
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Il giocatore corrente può pescare?
    /// - In modalità PescaTreCarte: sì, MA solo se il giocatore di turno è ancora Attivo
    ///   (se è Sballato, le pescate forzate vengono saltate automaticamente in Pesca()).
    /// - In gioco normale: solo se Attivo.
    /// </summary>
    public bool PuòPescare()
    {
        if (_statoPescaTreCarte == StatoPescaTreCarte.InCorso)
        {
            // Durante PescaTreCarte, controlla che il giocatore in modalità sia ancora Attivo
            return _giocatori[_giocatorePescaTreCarte].PuòPescare;
        }
        return _giocatori[GiocatoreCorrente].PuòPescare;
    }

    /// <summary>
    /// Il giocatore corrente può fermarsi volontariamente?
    /// Sì, solo se:
    /// - È Attivo
    /// - NON è in modalità PescaTreCarte
    /// - NON c'è un'azione in attesa di selezione bersaglio
    /// - Ha almeno una carta pescata, un modificatore, o un moltiplicatore
    /// </summary>
    public bool PuòFermarsi()
    {
        if (AzioneInAttesa != TipoAzioneInAttesa.Nessuna) return false;
        var g = _giocatori[GiocatoreCorrente];
        return g.PuòPescare
            && _statoPescaTreCarte == StatoPescaTreCarte.Nessuno
            && (g.Numeri.Count > 0
                || g.ModificatoreSomma > 0
                || g.HaMoltiplicatore);
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  AZIONI
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Il giocatore corrente pesca una carta dal mazzo.
    /// 
    ///  Se il giocatore è in modalità PescaTreCarte, continua a pescare
    ///    finché non ha pescato 3 carte. Le carte Congela vengono differite
    ///    e risolte solo dopo le 3 pescate.
    /// 
    ///  Se non è in PescaTreCarte, dopo la pescata passa al prossimo giocatore attivo.
    ///  Eccezione: se la carta pescata richiede selezione bersaglio (Congela, Pesca 3),
    ///    il turno NON viene passato — l'UI deve mostrare la scelta bersaglio.
    /// </summary>
    public void Pesca()
    {
        if (RoundFinito) return;

        // Pesca una carta dal mazzo
        Carta carta = _mazzo.Pesca();
        GestisciCartaPescata(GiocatoreCorrente, carta);

        // Se la pescata ha fatto finire il round (Flip7), fermati
        if (RoundFinito) return;

        // Se la carta richiede selezione bersaglio, non passare il turno
        // L'UI mostrerà i bersagli e chiamerà SelezionaBersaglio()
        if (AzioneInAttesa != TipoAzioneInAttesa.Nessuna) return;

        if (_statoPescaTreCarte == StatoPescaTreCarte.InCorso)
        {
            // Il giocatore è in modalità PescaTreCarte: continua a pescare
            _carteContatorePescaTreCarte++;

            // Se il giocatore è Sballato dopo questa pescata, salta le rimanenti pescate forzate
            if (_giocatori[_giocatorePescaTreCarte].ÈSballato)
            {
                _carteContatorePescaTreCarte = 3;   // Completa forzatamente PescaTreCarte
            }

            if (_carteContatorePescaTreCarte >= 3)
            {
                // Ha finito le 3 pescate forzate (o è sballato e ha saltato)
                _statoPescaTreCarte = StatoPescaTreCarte.Nessuno;
                _carteContatorePescaTreCarte = 0;

                // Risolvi le azioni Congela accumulate (solo se non è sballato)
                if (!_giocatori[_giocatorePescaTreCarte].ÈSballato)
                {
                    RisolviAzioniDifferite(_giocatorePescaTreCarte);
                }

                // Se dopo le azioni differite il round non è finito, passa al prossimo
                if (!RoundFinito)
                    PassaAlProssimoGiocatore();
            }
            // Se non ha ancora finito le 3, resta lo stesso giocatore
        }
        else
        {
            // Pesca normale: passa al prossimo giocatore attivo
            PassaAlProssimoGiocatore();
        }
    }

    /// <summary>
    /// Il giocatore corrente si ferma volontariamente (pulsante FERMA).
    /// Funziona solo se può fermarsi (controllo in PuòFermarsi()).
    /// </summary>
    public void Ferma()
    {
        if (!PuòFermarsi()) return;

        _giocatori[GiocatoreCorrente].Ferma();
        PassaAlProssimoGiocatore();
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  SELEZIONE BERSAGLIO
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Seleziona il bersaglio per l'azione in attesa (Congela o Pesca 3).
    /// 
    ///  Per Congela: il bersaglio viene congelato, poi si passa il turno.
    ///  Per Pesca 3: il bersaglio entra in modalità PescaTreCarte.
    ///    - Se il bersaglio è un altro giocatore, il turno passa a lui.
    ///    - Se il bersaglio è sé stesso, resta lo stesso giocatore.
    /// 
    /// </summary>
    /// <param name="indiceBersaglio">Indice del giocatore bersaglio (0-based).</param>
    public void SelezionaBersaglio(int indiceBersaglio)
    {
        if (AzioneInAttesa == TipoAzioneInAttesa.Nessuna) return;
        if (BersagliDisponibili == null
            || !System.Array.Exists(BersagliDisponibili, i => i == indiceBersaglio))
            return;

        switch (AzioneInAttesa)
        {
            case TipoAzioneInAttesa.ScegliBersaglioCongela:
                _giocatori[indiceBersaglio].Congela();
                AzioneInAttesa = TipoAzioneInAttesa.Nessuna;
                BersagliDisponibili = null;
                if (!RoundFinito)
                    PassaAlProssimoGiocatore();
                break;

            case TipoAzioneInAttesa.ScegliBersaglioPescaTreCarte:
                AzioneInAttesa = TipoAzioneInAttesa.Nessuna;
                BersagliDisponibili = null;

                if (indiceBersaglio != GiocatoreCorrente)
                {
                    // Pesca 3 su un altro giocatore: passa il turno al bersaglio
                    GiocatoreCorrente = indiceBersaglio;
                }
                // Attiva PescaTreCarte sul bersaglio
                _statoPescaTreCarte = StatoPescaTreCarte.InCorso;
                _giocatorePescaTreCarte = indiceBersaglio;
                _carteContatorePescaTreCarte = 0;
                _azioniDifferite.Clear();
                break;
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  FINE ROUND
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Calcola i punteggi finali di tutti i giocatori a fine round.
    /// Ogni giocatore viene valutato con CalcolatorePunteggio.
    /// </summary>
    /// <returns>Array di punteggi, uno per giocatore.</returns>
    public int[] CalcolaPunteggi()
    {
        return _giocatori.Select(g => CalcolatorePunteggio.CalcolaPunteggio(g)).ToArray();
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  METODI PRIVATI
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gestisce l'effetto di una carta appena pescata da un giocatore:
    /// - Aggiunge la carta allo stato del giocatore
    /// - Se sballato e ha SecondaChance, la usa
    /// - Controlla se ha attivato Flip7
    /// - Se è Congela: imposta selezione bersaglio (non durante PescaTreCarte)
    /// - Se è PescaTreCarte (Pesca 3): imposta selezione bersaglio
    /// - Se è Congela DURANTE PescaTreCarte: la differisce (risolta dopo le 3 pescate)
    /// </summary>
    private void GestisciCartaPescata(int indice, Carta carta)
    {
        var giocatore = _giocatori[indice];

        // Aggiungi la carta allo stato del giocatore
        giocatore.AggiungiCarta(carta);

        // Se il giocatore è sballato ma ha la SecondaChance → la consuma e torna Attivo
        if (giocatore.ÈSballato && giocatore.HaSecondaChance)
        {
            giocatore.UsaSecondaChance();
        }

        // Controlla se ha raggiunto 7 numeri unici (Flip7!)
        ControllaFlip7(indice);

        // ── Gestione carte Azione con selezione bersaglio ──
        if (carta.Tipo != TipoCarta.Azione) return;

        if (carta.Azione == TipoAzione.PescaTreCarte)
        {
            // Flip Three: il giocatore sceglie il bersaglio (sé stesso o un altro giocatore attivo)
            var bersagli = _giocatori
                .Select((g, i) => new { g, i })
                .Where(x => x.g.Stato == StatoGiocatore.Attivo)
                .Select(x => x.i)
                .ToArray();
            BersagliDisponibili = bersagli;
            AzioneInAttesa = TipoAzioneInAttesa.ScegliBersaglioPescaTreCarte;
            _azioniDifferite.Clear();
        }
        else if (carta.Azione == TipoAzione.Congela)
        {
            if (_statoPescaTreCarte == StatoPescaTreCarte.InCorso)
            {
                // Durante PescaTreCarte: la Congela viene differita
                _azioniDifferite.Add(carta);
            }
            else
            {
                // Congela normale: il giocatore sceglie un altro giocatore attivo da congelare
                var bersagli = _giocatori
                    .Select((g, i) => new { g, i })
                    .Where(x => x.g.Stato == StatoGiocatore.Attivo && x.i != indice)
                    .Select(x => x.i)
                    .ToArray();
                if (bersagli.Length > 0)
                {
                    BersagliDisponibili = bersagli;
                    AzioneInAttesa = TipoAzioneInAttesa.ScegliBersaglioCongela;
                }
                // Se nessun bersaglio valido, la Congela viene sprecata (nessun effetto)
            }
        }
        // SecondaChance: già gestita sopra (UsaSecondaChance)
    }

    /// <summary>
    /// Controlla se il giocatore ha pescato 7 numeri unici (Flip7!).
    /// Se sì, il round finisce immediatamente.
    /// </summary>
    private void ControllaFlip7(int indice)
    {
        var giocatore = _giocatori[indice];
        if (giocatore.Numeri.Count >= 7)
        {
            Flip7Attivato = true;
        }
    }

    /// <summary>
    /// Passa il turno al prossimo giocatore che è ancora "Attivo".
    /// Cerca in ordine circolare partendo dal giocatore successivo.
    /// Se nessuno è attivo, il round finirà (RoundFinito = true).
    /// </summary>
    private void PassaAlProssimoGiocatore()
    {
        for (int i = 0; i < NumeroGiocatori; i++)
        {
            int indice = (GiocatoreCorrente + 1 + i) % NumeroGiocatori;
            if (_giocatori[indice].Stato == StatoGiocatore.Attivo)
            {
                GiocatoreCorrente = indice;
                return;
            }
        }
        // Nessun giocatore attivo trovato → RoundFinito diventerà true
    }

    /// <summary>
    /// Risolve le azioni differite accumulate durante PescaTreCarte.
    /// Al momento gestisce solo le carte Congela.
    /// </summary>
    private void RisolviAzioniDifferite(int indice)
    {
        foreach (var carta in _azioniDifferite)
        {
            if (carta.Tipo == TipoCarta.Azione && carta.Azione == TipoAzione.Congela)
            {
                _giocatori[indice].Congela();
            }
        }
        _azioniDifferite.Clear();
    }
}
