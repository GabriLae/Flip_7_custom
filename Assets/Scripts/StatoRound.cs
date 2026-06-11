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
    /// Sì, se è Attivo oppure se è in modalità PescaTreCarte.
    /// </summary>
    public bool PuòPescare()
    {
        return _giocatori[GiocatoreCorrente].PuòPescare
            || _statoPescaTreCarte == StatoPescaTreCarte.InCorso;
    }

    /// <summary>
    /// Il giocatore corrente può fermarsi volontariamente?
    /// Sì, solo se:
    /// - È Attivo
    /// - NON è in modalità PescaTreCarte
    /// - Ha almeno una carta pescata, un modificatore, o un moltiplicatore
    /// </summary>
    public bool PuòFermarsi()
    {
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
    /// </summary>
    public void Pesca()
    {
        if (RoundFinito) return;

        // Pesca una carta dal mazzo
        Carta carta = _mazzo.Pesca();
        GestisciCartaPescata(GiocatoreCorrente, carta);

        // Se la pescata ha fatto finire il round (Flip7), fermati
        if (RoundFinito) return;

        if (_statoPescaTreCarte == StatoPescaTreCarte.InCorso)
        {
            // Il giocatore è in modalità PescaTreCarte: continua a pescare
            _carteContatorePescaTreCarte++;

            if (_carteContatorePescaTreCarte >= 3)
            {
                // Ha finito le 3 pescate forzate
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
    /// - Se è PescaTreCarte, attiva la modalità
    /// - Se è Congela durante PescaTreCarte, la differisce
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

        // Se è una carta PescaTreCarte, attiva la modalità speciale
        if (carta.Tipo == TipoCarta.Azione && carta.Azione == TipoAzione.PescaTreCarte)
        {
            _statoPescaTreCarte = StatoPescaTreCarte.InCorso;
            _giocatorePescaTreCarte = indice;
            _carteContatorePescaTreCarte = 0;
        }

        // Se è una carta Congela durante PescaTreCarte, la mette in coda
        if (carta.Tipo == TipoCarta.Azione
            && carta.Azione == TipoAzione.Congela
            && _statoPescaTreCarte == StatoPescaTreCarte.InCorso)
        {
            _azioniDifferite.Add(carta);
        }
    }

    /// <summary>
    /// Controlla se il giocatore ha pescato 7 numeri unici (Flip7!).
    /// Se sì, il round finisce immediatamente.
    /// </summary>
    private void ControllaFlip7(int indice)
    {
        if (_giocatori[indice].Numeri.Count >= 7)
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
