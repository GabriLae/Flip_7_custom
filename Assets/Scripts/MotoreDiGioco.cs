// Flip Seven (Flip 7) — GameCore
// Motore di gioco principale — punto di ingresso unico per l'interfaccia Unity.
// Tutte le classi in Core/ sono C# puro (nessuna dipendenza da UnityEngine).

/// <summary>
/// DTO (Data Transfer Object) che rappresenta un'istantanea leggibile
/// dello stato di gioco corrente, pronta per essere visualizzata dall'UI.
/// 
/// L'interfaccia Unity (Bootstrap) chiama OttieniStatoPubblico() ogni frame
/// e usa questi dati per aggiornare testi, colori e log a schermo.
/// </summary>
public class StatoPubblico
{
    /// <summary>Indice del giocatore attualmente di turno (0-based).</summary>
    public int GiocatoreCorrente { get; set; }

    /// <summary>Numero del round corrente (0-based).</summary>
    public int RoundCorrente { get; set; }

    /// <summary>True se il round corrente è terminato (Flip7 o tutti fermi/sballati).</summary>
    public bool RoundFinito { get; set; }

    /// <summary>True se la partita è finita (qualcuno ha raggiunto 200+ punti).</summary>
    public bool PartitaFinita { get; set; }

    /// <summary>Indice del vincitore (null se partita in corso).</summary>
    public int? VincitoreIndice { get; set; }

    /// <summary>Punteggi totali accumulati da ogni giocatore su tutti i round.</summary>
    public int[] PunteggiTotali { get; set; }

    /// <summary>
    /// Stato di ogni giocatore nel round corrente.
    /// Valori possibili: "Attivo", "Congelato", "Sballato", "Fermo".
    /// </summary>
    public string[] StatiGiocatori { get; set; }

    /// <summary>Numero di carte Numero uniche pescate da ogni giocatore.</summary>
    public int[] ConteggioNumeri { get; set; }

    /// <summary>Punteggio potenziale attuale di ogni giocatore (prima del calcolo finale).</summary>
    public int[] PunteggiPotenziali { get; set; }
}

/// <summary>
/// MOTORE DI GIOCO PRINCIPALE — Facciata (Facade) che coordina:
/// - StatoPartita: punteggi totali, fine partita (200+ punti)
/// - StatoRound: turni correnti, pescate, azioni speciali
/// - Mazzo: costruzione, mescola, pesca
/// 
///  L'interfaccia Unity (Bootstrap) interagisce SOLO con questa classe.
/// Nessun altro file del core viene chiamato direttamente dall'UI.
/// </summary>
public class MotoreDiGioco
{
    // ═══════════════════════════════════════════════════════════════════════
    //  STATO INTERNO
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>Stato della partita (punteggi totali, fine partita).</summary>
    public StatoPartita Partita { get; private set; }

    /// <summary>Stato del round corrente (turni, pescate, azioni).</summary>
    public StatoRound Round { get; private set; }

    /// <summary>Mazzo di carte (94 carte, mescolato).</summary>
    public Mazzo Mazzo { get; private set; }

    // ═══════════════════════════════════════════════════════════════════════
    //  COMANDI DI PARTITA
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Avvia una nuova partita con il numero specificato di giocatori.
    /// Crea immediatamente anche il primo round.
    /// </summary>
    /// <param name="numeroGiocatori">Numero di giocatori (in genere 3-8).</param>
    public void AvviaPartita(int numeroGiocatori)
    {
        Partita = new StatoPartita(numeroGiocatori);
        AvviaRound();
    }

    /// <summary>
    /// Avvia un nuovo round: crea un nuovo Mazzo (con tutte le 94 carte mescolate)
    /// e un nuovo StatoRound (giocatori tutti attivi, turno al giocatore 0).
    /// </summary>
    public void AvviaRound()
    {
        Mazzo = new Mazzo();   // Mazzo fresco mescolato
        Round = new StatoRound(Mazzo, Partita.NumeroGiocatori);
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  COMANDI DI ROUND (delegati a StatoRound)
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>Il giocatore corrente può pescare una carta?</summary>
    public bool PuòPescare() => Round.PuòPescare();

    /// <summary>Il giocatore corrente può fermarsi volontariamente?</summary>
    public bool PuòFermarsi() => Round.PuòFermarsi();

    /// <summary>Il giocatore corrente pesca una carta dal mazzo.</summary>
    public void Pesca() => Round.Pesca();

    /// <summary>Il giocatore corrente si ferma volontariamente.</summary>
    public void Ferma() => Round.Ferma();

    /// <summary>True se il round corrente è terminato.</summary>
    public bool RoundFinito => Round.RoundFinito;

    // ═══════════════════════════════════════════════════════════════════════
    //  GESTIONE FINE ROUND
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Conclude il round corrente:
    /// 1. Calcola i punteggi finali di tutti i giocatori
    /// 2. Aggiorna i punteggi totali nella partita
    /// 3. Controlla se qualcuno ha raggiunto 200+ punti (vittoria)
    /// </summary>
    public void ConcludiRound()
    {
        int[] punteggi = Round.CalcolaPunteggi();
        Partita.AggiornaPunteggi(punteggi);
        Partita.ControllaVincitore();
    }

    /// <summary>True se la partita è finita (qualcuno ha raggiunto l'obiettivo).</summary>
    public bool PartitaFinita => Partita.Stato == StatoPartitaEnum.Finita;

    /// <summary>Indice del vincitore (0-based), o null se partita in corso.</summary>
    public int? VincitoreIndice => Partita.VincitoreIndice;

    // ═══════════════════════════════════════════════════════════════════════
    //  STATO PUBBLICO PER INTERFACCIA
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Restituisce un'istantanea (StatoPubblico) dello stato corrente,
    /// pronta per essere visualizzata dall'interfaccia Unity.
    /// 
    /// Viene chiamato ogni frame da Bootstrap.AggiornaDisplayGioco().
    /// </summary>
    public StatoPubblico OttieniStatoPubblico()
    {
        // Sicurezza: se non c'è partita, restituisci stato di fine
        if (Partita == null || Round == null)
            return new StatoPubblico { PartitaFinita = true };

        int n = Partita.NumeroGiocatori;

        var stato = new StatoPubblico
        {
            GiocatoreCorrente = Round.GiocatoreCorrente,
            RoundCorrente = Partita.RoundCorrente,
            RoundFinito = Round.RoundFinito,
            PartitaFinita = PartitaFinita,
            VincitoreIndice = VincitoreIndice,
            PunteggiTotali = (int[])Partita.PunteggiTotali.Clone(),
            StatiGiocatori = new string[n],
            ConteggioNumeri = new int[n],
            PunteggiPotenziali = new int[n],
        };

        // Compila i dati per ogni giocatore
        for (int i = 0; i < n; i++)
        {
            var g = Round.OttieniGiocatore(i);
            stato.StatiGiocatori[i] = g.Stato.ToString();     // "Attivo", "Sballato", ecc.
            stato.ConteggioNumeri[i] = g.Numeri.Count;        // Quanti numeri unici ha
            stato.PunteggiPotenziali[i] = CalcolatorePunteggio.CalcolaPunteggio(g);
        }

        return stato;
    }
}
