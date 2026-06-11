using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Stato della modalità PescaTreCarte (Flip Three).
/// Nessuno: non in corso. InCorso: il giocatore sta pescando 3 carte forzate.
/// </summary>
public enum StatoPescaTreCarte
{
    Nessuno,
    InCorso
}

/// <summary>
/// Stato del round corrente: gestisce turni, pescate, azioni speciali e fine round.
/// Coordina il Mazzo e gli array di StatoRoundGiocatore.
/// </summary>
public class StatoRound
{
    // ── Dipendenze ──
    private Mazzo _mazzo;
    private StatoRoundGiocatore[] _giocatori;

    // ── Stato turno ──
    public int GiocatoreCorrente { get; private set; }
    public int NumeroGiocatori { get; }

    // ── Stato round ──
    public bool Flip7Attivato { get; private set; }

    /// <summary>
    /// Il round finisce quando:
    /// - Qualcuno ha fatto Flip 7 (7 numeri unici), oppure
    /// - Nessun giocatore è più Attivo
    /// </summary>
    public bool RoundFinito =>
        Flip7Attivato ||
        !_giocatori.Any(g => g.Stato == StatoGiocatore.Attivo);

    // ── PescaTreCarte tracking ──
    private StatoPescaTreCarte _statoPescaTreCarte;
    private int _giocatorePescaTreCarte;
    private int _carteContatorePescaTreCarte;
    private List<Carta> _azioniDifferite;

    // ── Costruttore ──
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

    // ── Accesso agli stati ──
    public StatoRoundGiocatore OttieniGiocatore(int indice)
    {
        return _giocatori[indice];
    }

    // ── Controlli ──
    public bool PuòPescare()
    {
        return _giocatori[GiocatoreCorrente].PuòPescare
            || _statoPescaTreCarte == StatoPescaTreCarte.InCorso;
    }

    public bool PuòFermarsi()
    {
        var g = _giocatori[GiocatoreCorrente];
        return g.PuòPescare
            && _statoPescaTreCarte == StatoPescaTreCarte.Nessuno
            && (g.Numeri.Count > 0
                || g.ModificatoreSomma > 0
                || g.HaMoltiplicatore);
    }

    // ── Azioni ──

    /// <summary>
    /// Il giocatore corrente pesca una carta.
    /// Se è in PescaTreCarte, resta lo stesso giocatore finché non pesca 3 carte.
    /// </summary>
    public void Pesca()
    {
        if (RoundFinito) return;

        Carta carta = _mazzo.Pesca();
        GestisciCartaPescata(GiocatoreCorrente, carta);

        // Se il round è finito (Flip7), non passare al prossimo
        if (RoundFinito) return;

        if (_statoPescaTreCarte == StatoPescaTreCarte.InCorso)
        {
            // Il giocatore è in PescaTreCarte — continua a pescare
            _carteContatorePescaTreCarte++;

            if (_carteContatorePescaTreCarte >= 3)
            {
                // Ha finito le 3 pescate forzate
                _statoPescaTreCarte = StatoPescaTreCarte.Nessuno;
                _carteContatorePescaTreCarte = 0;

                // Risolvi azioni differite (solo se il giocatore non è sballato)
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
            // Normale: passa al prossimo giocatore attivo
            PassaAlProssimoGiocatore();
        }
    }

    /// <summary>
    /// Il giocatore corrente si ferma volontariamente.
    /// </summary>
    public void Ferma()
    {
        if (!PuòFermarsi()) return;

        _giocatori[GiocatoreCorrente].Ferma();
        PassaAlProssimoGiocatore();
    }

    // ── Fine round ──

    /// <summary>
    /// Calcola i punteggi di tutti i giocatori a fine round.
    /// </summary>
    public int[] CalcolaPunteggi()
    {
        return _giocatori.Select(g => CalcolatorePunteggio.CalcolaPunteggio(g)).ToArray();
    }

    // ── Metodi privati ──

    private void GestisciCartaPescata(int indice, Carta carta)
    {
        var giocatore = _giocatori[indice];

        // Aggiungi la carta allo stato del giocatore
        giocatore.AggiungiCarta(carta);

        // Se il giocatore è sballato e ha SecondaChance → la usa
        if (giocatore.ÈSballato && giocatore.HaSecondaChance)
        {
            giocatore.UsaSecondaChance();
        }

        // Controlla Flip 7
        ControllaFlip7(indice);

        // Se è un'Azione PescaTreCarte, attiva la modalità
        if (carta.Tipo == TipoCarta.Azione && carta.Azione == TipoAzione.PescaTreCarte)
        {
            _statoPescaTreCarte = StatoPescaTreCarte.InCorso;
            _giocatorePescaTreCarte = indice;
            _carteContatorePescaTreCarte = 0;
        }

        // Se è un'Azione Congela durante PescaTreCarte, differisci
        if (carta.Tipo == TipoCarta.Azione
            && carta.Azione == TipoAzione.Congela
            && _statoPescaTreCarte == StatoPescaTreCarte.InCorso)
        {
            _azioniDifferite.Add(carta);
        }
    }

    private void ControllaFlip7(int indice)
    {
        if (_giocatori[indice].Numeri.Count >= 7)
        {
            Flip7Attivato = true;
        }
    }

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
        // Nessun giocatore attivo trovato — RoundFinito diventerà true
    }

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
