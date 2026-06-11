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

    /// <summary>
    /// Aggiunge una carta allo stato del giocatore.
    /// 
    ///  Comportamento per tipo:
    ///   - Numero: se è duplicato → Sballato; altrimenti lo aggiunge ai numeri
    ///   - Modificatore (+X): somma il valore a ModificatoreSomma
    ///   - Modificatore (x2): imposta HaMoltiplicatore = true
    ///   - Azione Congela: imposta Stato = Congelato
    ///   - Azioni PescaTreCarte/SecondaChance: solo incrementa ContoAzioni
    ///     (la logica è gestita da StatoRound)
    /// </summary>
    public void AggiungiCarta(Carta carta)
    {
        switch (carta.Tipo)
        {
            case TipoCarta.Numero:
                // Se il numero è già stato pescato → Sballato!
                if (_numeri.Contains(carta.Numero.Value))
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
                if (carta.Azione == TipoAzione.Congela)
                {
                    Congela();
                }
                // PescaTreCarte e SecondaChance: solo conteggiate qui,
                // la logica è gestita da StatoRound
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
}
