using System.Linq;

/// <summary>
/// CALCOLATORE PUNTEGGIO — Classe statica che calcola il punteggio
/// di un giocatore alla fine del round.
/// 
/// Regole di calcolo (in ordine):
/// 1. Se il giocatore è Sballato → punteggio = 0
/// 2. Somma di tutti i valori delle carte Numero pescate
/// 3. Se ha x2 → raddoppia la somma
/// 4. Aggiunge i modificatori +X
/// 5. Se ha pescato 7+ carte Azione → bonus +15 (Flip7!)
/// </summary>
public static class CalcolatorePunteggio
{
    /// <summary>
    /// Calcola il punteggio finale per un giocatore a fine round.
    /// 
    ///  Formula:
    /// <code>
    ///   if (Sballato) → 0
    ///   else → (SommaNumeri × (x2 ? 2 : 1)) + Modificatori + (Azioni ≥ 7 ? 15 : 0)
    /// </code>
    /// 
    /// </summary>
    /// <param name="stato">Stato del giocatore nel round corrente.</param>
    /// <returns>Punteggio calcolato (intero, mai negativo).</returns>
    public static int CalcolaPunteggio(StatoRoundGiocatore stato)
    {
        //  REGOLA 1: Se sballato → 0 punti
        if (stato.ÈSballato)
        {
            return 0;
        }

        //  REGOLA 2: Somma dei valori delle carte Numero pescate
        int punteggio = stato.Numeri.Sum(v => (int)v);

        //  REGOLA 3: Se ha il moltiplicatore x2, raddoppia
        if (stato.HaMoltiplicatore)
        {
            punteggio *= 2;
        }

        //  REGOLA 4: Aggiungi i modificatori +X
        punteggio += stato.ModificatoreSomma;

        //  REGOLA 5: Bonus Flip7 — se ha pescato 7+ carte Azione, +15 punti
        if (stato.ContoAzioni >= 7)
        {
            punteggio += 15;
        }

        return punteggio;
    }
}
