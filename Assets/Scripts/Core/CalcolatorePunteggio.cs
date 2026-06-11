using System.Linq;

/// <summary>
/// Calcola il punteggio di un giocatore alla fine del round.
/// Logica: somma numeri → (x2?) → aggiungi +X → (bonus 7+ azioni?)
/// </summary>
public static class CalcolatorePunteggio
{
    /// <summary>
    /// Calcola il punteggio finale per uno StatoRoundGiocatore.
    /// Ordine: somma Numeri → x2 → +X → +15 (Flip7)
    /// </summary>
    public static int CalcolaPunteggio(StatoRoundGiocatore stato)
    {
        // Regola 1: Se sballato → 0
        if (stato.ÈSballato)
        {
            return 0;
        }

        // Regola 2: Somma dei numeri pescati
        int punteggio = stato.Numeri.Sum(v => (int)v);

        // Regola 3: Se ha moltiplicatore, raddoppia la somma
        if (stato.HaMoltiplicatore)
        {
            punteggio *= 2;
        }

        // Regola 4: Aggiungi i modificatori +X
        punteggio += stato.ModificatoreSomma;

        // Regola 5: Se azioni >= 7, aggiungi 15 (Flip 7 bonus)
        if (stato.ContoAzioni >= 7)
        {
            punteggio += 15;
        }

        return punteggio;
    }
}
