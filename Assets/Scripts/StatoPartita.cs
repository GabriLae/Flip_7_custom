using System.Linq;

/// <summary>
/// Stato della partita: InCorso (si continua a giocare) o Finita
/// (quando un giocatore raggiunge o supera il punteggio obiettivo, default 200).
/// </summary>
public enum StatoPartitaEnum
{
    /// <summary>La partita è ancora in corso — nessun vincitore.</summary>
    InCorso,

    /// <summary>La partita è finita — un giocatore ha raggiunto 200+ punti.</summary>
    Finita
}

/// <summary>
/// STATO DELLA PARTITA — Tiene traccia dei dati persistenti su più round:
/// - Punteggi totali accumulati da ogni giocatore
/// - Indice del Mazziere (ruota dopo ogni round)
/// - Condizione di fine partita (200+ punti)
/// 
/// Non contiene logica di turno o pescata — quella è in StatoRound.
/// </summary>
public class StatoPartita
{
    // ═══════════════════════════════════════════════════════════════════════
    //  PROPRIETÀ
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>Numero di giocatori in questa partita (tipicamente 3-8).</summary>
    public int NumeroGiocatori { get; }

    /// <summary>Punteggi totali accumulati da ogni giocatore su tutti i round.</summary>
    public int[] PunteggiTotali { get; private set; }

    /// <summary>Punteggio da raggiungere per vincere (default 200).</summary>
    public int PunteggioObiettivo { get; }

    /// <summary>Numero del round corrente (parte da 0, incrementa dopo ogni round).</summary>
    public int RoundCorrente { get; private set; }

    /// <summary>Indice del giocatore che fa da Mazziere nel round corrente.</summary>
    public int IndiceMazziere { get; private set; }

    /// <summary>Stato della partita: InCorso o Finita.</summary>
    public StatoPartitaEnum Stato { get; private set; }

    /// <summary>Indice del vincitore (solo se Stato == Finita), altrimenti null.</summary>
    public int? VincitoreIndice { get; private set; }

    // ═══════════════════════════════════════════════════════════════════════
    //  COSTRUTTORE
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Crea una nuova partita.
    /// </summary>
    /// <param name="numeroGiocatori">Quanti giocatori partecipano.</param>
    /// <param name="punteggioObiettivo">Punteggio necessario per vincere (default 200).</param>
    public StatoPartita(int numeroGiocatori, int punteggioObiettivo = 200)
    {
        NumeroGiocatori = numeroGiocatori;
        PunteggioObiettivo = punteggioObiettivo;
        PunteggiTotali = new int[numeroGiocatori];   // Tutti partono da 0
        RoundCorrente = 0;
        IndiceMazziere = 0;                            // Il giocatore 0 è il primo Mazziere
        Stato = StatoPartitaEnum.InCorso;
        VincitoreIndice = null;
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  METODI
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Aggiorna i punteggi totali con i punteggi del round appena concluso.
    /// Ogni giocatore riceve i punti guadagnati nel round.
    /// Poi passa il ruolo di Mazziere al giocatore successivo (in senso orario).
    /// </summary>
    /// <param name="punteggiRound">Array dei punteggi del round per ogni giocatore.</param>
    public void AggiornaPunteggi(int[] punteggiRound)
    {
        // Aggiunge i punti del round ai totali di ogni giocatore
        for (int i = 0; i < NumeroGiocatori; i++)
        {
            PunteggiTotali[i] += punteggiRound[i];
        }

        RoundCorrente++;
        IndiceMazziere = (IndiceMazziere + 1) % NumeroGiocatori;
    }

    /// <summary>
    /// Controlla se un giocatore ha raggiunto o superato il punteggio obiettivo.
    /// 
    ///    Se più giocatori superano l'obiettivo nello stesso round,
    ///    vince chi ha il punteggio totale più alto.
    /// </summary>
    /// <returns>True se la partita è finita, False se si continua.</returns>
    public bool ControllaVincitore()
    {
        for (int i = 0; i < NumeroGiocatori; i++)
        {
            if (PunteggiTotali[i] >= PunteggioObiettivo)
            {
                // Potrebbero esserci più giocatori sopra 200 — vince il più alto
                int indiceVincitore = TrovaIndicePunteggioMassimo();
                Stato = StatoPartitaEnum.Finita;
                VincitoreIndice = indiceVincitore;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Restituisce l'indice del giocatore con il punteggio totale più alto.
    /// Usato in caso di pareggio (più giocatori sopra 200).
    /// </summary>
    private int TrovaIndicePunteggioMassimo()
    {
        int maxPunti = PunteggiTotali.Max();
        return System.Array.IndexOf(PunteggiTotali, maxPunti);
    }
}
