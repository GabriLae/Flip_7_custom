using System.Collections.Generic;

/// <summary>
/// Stato di un giocatore durante un round.
/// </summary>
public enum StatoGiocatore
{
    Attivo,      // può ancora pescare
    Congelato,   // bloccato da carta Congela
    Sballato,    // ha superato il limite (numero duplicato)
    Fermo        // si è fermato volontariamente
}

/// <summary>
/// Stato individuale di un giocatore per il round corrente.
/// Tiene traccia di carte pescate, modificatori, azioni e stato.
/// </summary>
public class StatoRoundGiocatore
{
    // ── Proprietà ──
    public StatoGiocatore Stato { get; private set; }
    public bool HaSecondaChance { get; private set; }   // parte a true

    // Carte pescate, divise per tipo
    private List<ValoreNumero> _numeri;                       // numeri pescati (campo privato)
    public int ModificatoreSomma { get; private set; }             // somma dei +X
    public bool HaMoltiplicatore { get; private set; }          // ha x2?
    public int ContoAzioni { get; private set; }             // quante carte azione pescate

    // ── Costruttore ──
    public StatoRoundGiocatore()
    {
        Stato = StatoGiocatore.Attivo;
        HaSecondaChance = true;
        _numeri = new List<ValoreNumero>();
        ModificatoreSomma = 0;
        HaMoltiplicatore = false;
        ContoAzioni = 0;
    }

    // ── Proprietà calcolate ──
    public bool ÈSballato => Stato == StatoGiocatore.Sballato;
    public bool PuòPescare => Stato == StatoGiocatore.Attivo;

    // ── Metodi ──

    /// <summary>
    /// Aggiunge una carta allo stato del giocatore.
    /// Se è un Numero duplicato → Sballato.
    /// Se è un Modificatore → aggiorna somma o moltiplicatore.
    /// Se è un'Azione Congela → imposta Congelato.
    /// FlipThree e SecondaChance sono gestiti da StatoRound.
    /// </summary>
    public void AggiungiCarta(Carta carta)
    {
        switch (carta.Tipo)
        {
            case TipoCarta.Numero:
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
                // PescaTreCarte e SecondaChance gestiti da StatoRound
                break;
        }
    }

    /// <summary>
    /// Consuma la SecondaChance per prevenire uno sballo.
    /// Funziona solo se il giocatore è Sballato e ha ancora la SecondaChance.
    /// </summary>
    public void UsaSecondaChance()
    {
        if (HaSecondaChance && Stato == StatoGiocatore.Sballato)
        {
            HaSecondaChance = false;
            Stato = StatoGiocatore.Attivo;
        }
    }

    /// <summary>
    /// Congela il giocatore (forzato fuori dal round).
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
    /// Restituisce la lista readonly dei numeri pescati.
    /// </summary>
    public IReadOnlyList<ValoreNumero> Numeri => _numeri.AsReadOnly();
}
