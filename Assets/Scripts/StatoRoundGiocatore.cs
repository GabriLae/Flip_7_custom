using System.Collections.Generic;

/// <summary>
/// Stato di un giocatore durante un round.
/// - Attivo: può ancora pescare o fermarsi.
/// - Congelato: bloccato da carta Congela, esce dal round con i punti correnti.
/// - Sballato: ha pescato un numero duplicato, esce con 0 punti.
/// - Fermo: si è fermato volontariamente, conserva i punti.
/// </summary>
public enum StatoGiocatore
{
    /// <summary>Il giocatore può ancora pescare o fermarsi.</summary>
    Attivo,

    /// <summary>Bloccato da carta Congela — esce dal round con i punti attuali.</summary>
    Congelato,

    /// <summary>Ha superato il limite (numero duplicato) — esce con 0 punti.</summary>
    Sballato,

    /// <summary>Si è fermato volontariamente — conserva i punti correnti.</summary>
    Fermo
}

/// <summary>
/// STATO INDIVIDUALE DI UN GIOCATORE per il round corrente.
/// 
/// Ogni giocatore ha:
/// - Stato corrente (Attivo/Congelato/Sballato/Fermo)
/// - SecondaChance: se true, può salvarsi da uno sballo (una volta sola)
/// - Carte pescate: numeri unici, modificatori, azioni
/// 
/// Regole:
/// - Se pesca un numero già pescato → Sballato (0 punti a fine round)
/// - Se ha SecondaChance e sballa → consuma la SecondaChance, torna Attivo
/// - I modificatori +X e x2 si accumulano senza causare sballo
/// - Le azioni applicano effetti speciali (Congela, ecc.)
/// </summary>
public class StatoRoundGiocatore
{
    // ═══════════════════════════════════════════════════════════════════════
    //  PROPRIETÀ
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>Stato corrente del giocatore in questo round.</summary>
    public StatoGiocatore Stato { get; private set; }

    /// <summary>
    /// Il giocatore ha ancora la SecondaChance disponibile?
    /// Parte a true per tutti, diventa false dopo l'uso.
    /// </summary>
    public bool HaSecondaChance { get; private set; }

    /// <summary>Carte Numero pescate (valori unici — nessun duplicato).</summary>
    private List<ValoreNumero> _numeri;

    /// <summary>Tutte le carte pescate in ordine (per visualizzazione UI).</summary>
    private List<Carta> _cartePescate;

    /// <summary>Somma totale dei modificatori +X pescati.</summary>
    public int ModificatoreSomma { get; private set; }

    /// <summary>True se ha pescato un moltiplicatore x2.</summary>
    public bool HaMoltiplicatore { get; private set; }

    /// <summary>Quante carte Azione ha pescato (per bonus Flip7).</summary>
    public int ContoAzioni { get; private set; }

    // ═══════════════════════════════════════════════════════════════════════
    //  COSTRUTTORE
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Crea un nuovo stato giocatore: Attivo, con SecondaChance disponibile,
    /// nessuna carta pescata, nessun modificatore.
    /// </summary>
    public StatoRoundGiocatore()
    {
        Stato = StatoGiocatore.Attivo;
        HaSecondaChance = true;
        _numeri = new List<ValoreNumero>();
        _cartePescate = new List<Carta>();
        ModificatoreSomma = 0;
        HaMoltiplicatore = false;
        ContoAzioni = 0;
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  PROPRIETÀ CALCOLATE
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>True se il giocatore è Sballato.</summary>
    public bool ÈSballato => Stato == StatoGiocatore.Sballato;

    /// <summary>True se il giocatore può ancora pescare (solo se Attivo).</summary>
    public bool PuòPescare => Stato == StatoGiocatore.Attivo;

    // ═══════════════════════════════════════════════════════════════════════
    //  METODI
    // ═══════════════════════════════════════════════════════════════════════


    /// <summary>Restituisce la lista di tutte le carte pescate in ordine (per UI).</summary>
    public IReadOnlyList<Carta> CartePescate => _cartePescate.AsReadOnly();

    /// <summary>
    /// Restituisce una stringa compatta delle carte pescate per la UI.
    /// Esempio: "7 3 9 | +4 x2 | Congela"
    /// </summary>
    public string DescrizioneCarte()
    {
        var nums = new List<string>();
        var mods = new List<string>();
        var azioni = new List<string>();

        foreach (var c in _cartePescate)
        {
            switch (c.Tipo)
            {
                case TipoCarta.Numero:
                    nums.Add(((int)c.Numero.Value).ToString());
                    break;
                case TipoCarta.Modificatore:
                    mods.Add(c.ToString());
                    break;
                case TipoCarta.Azione:
                    azioni.Add(c.ToString());
                    break;
            }
        }

        var parti = new List<string>();
        if (nums.Count > 0) parti.Add(string.Join(" ", nums));
        if (mods.Count > 0) parti.Add(string.Join(" ", mods));
        if (azioni.Count > 0) parti.Add(string.Join(" ", azioni));
        return string.Join(" | ", parti);
    }

    public void AggiungiCarta(Carta carta)
    {
        // Registra la carta nella cronologia (per visualizzazione UI)
        _cartePescate.Add(carta);

        switch (carta.Tipo)
        {
            case TipoCarta.Numero:
                // Se il numero è già stato pescato → Sballato!
                // Usa cast esplicito a int per evitare problemi di comparazione enum in alcuni runtime
                if (ContieneNumero(carta.Numero.Value))
                {
                    Stato = StatoGiocatore.Sballato;
                }
                else
                {
                    _numeri.Add(carta.Numero.Value);
                }
                break;

            case TipoCarta.Modificatore:
                if (carta.Modificatore == TipoModificatore.Aggiungi)
                {
                    ModificatoreSomma += carta.ValoreModificatore.Value;
                }
                else if (carta.Modificatore == TipoModificatore.Moltiplica)
                {
                    HaMoltiplicatore = true;
                }
                break;

            case TipoCarta.Azione:
                ContoAzioni++;
                // Congela e PescaTreCarte (Flip Three): 
                // solo conteggiate qui come carte Azione per il bonus Flip7.
                // La logica di targeting e applicazione è gestita da StatoRound.
                // SecondaChance: già gestita in GestisciCartaPescata di StatoRound.
                break;
        }
    }

    /// <summary>
    /// Consuma la SecondaChance per prevenire uno sballo.
    /// Funziona solo se il giocatore è Sballato e ha ancora la SecondaChance.
    /// Dopo l'uso, torna Attivo.
    /// </summary>
    public void UsaSecondaChance()
    {
        if (HaSecondaChance && Stato == StatoGiocatore.Sballato)
        {
            HaSecondaChance = false;   // Consumata
            Stato = StatoGiocatore.Attivo;  // Torna in gioco!
        }
    }

    /// <summary>
    /// Congela il giocatore (forzato fuori dal round).
    /// Non può più pescare, ma conserva i punti.
    /// </summary>
    public void Congela()
    {
        Stato = StatoGiocatore.Congelato;
    }

    /// <summary>
    /// Ferma il giocatore volontariamente.
    /// </summary>
    public void Ferma()
    {
        Stato = StatoGiocatore.Fermo;
    }

    /// <summary>
    /// Restituisce la lista readonly dei numeri pescati (soli valori unici).
    /// </summary>
    public IReadOnlyList<ValoreNumero> Numeri => _numeri.AsReadOnly();

    // ═══════════════════════════════════════════════════════════════════════
    //  HELPER PRIVATI
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifica se un valore numerico è già stato pescato.
    /// Usa cast esplicito a int per evitare problemi di comparazione
    /// enum in alcuni runtime .NET (Unity IL2CPP / AOT).
    /// </summary>
    private bool ContieneNumero(ValoreNumero valore)
    {
        int target = (int)valore;
        for (int i = 0; i < _numeri.Count; i++)
        {
            if ((int)_numeri[i] == target) return true;
        }
        return false;
    }
}
