// Flip Seven (Flip 7) — GameCore
// Modello dati delle carte del gioco — tutto in italiano.
// Tutte le classi in Core/ sono C# puro (nessun using UnityEngine).

/// <summary>
/// Categorie principali di carte nel mazzo Flip 7.
/// Numero: carte numerate 0-12. Causano sballo se duplicate.
/// Modificatore: modificano il punteggio (+X o x2). Non causano sballo.
/// Azione: carte effetto speciale (Congela, PescaTreCarte, SecondaChance).
/// </summary>
public enum TipoCarta
{
    Numero,
    Modificatore,
    Azione,
}

/// <summary>
/// Valori possibili per le carte Numero (0-12).
/// La distribuzione nel mazzo è: 1 carta "0", 1 carta "1",
/// 2 carte "2", 3 carte "3", ..., 12 carte "12".
/// </summary>
public enum ValoreNumero
{
    Zero = 0,
    Uno = 1,
    Due = 2,
    Tre = 3,
    Quattro = 4,
    Cinque = 5,
    Sei = 6,
    Sette = 7,
    Otto = 8,
    Nove = 9,
    Dieci = 10,
    Undici = 11,
    Dodici = 12
}

/// <summary>
/// Tipi di carte modificatore di punteggio.
/// Aggiungi: aggiunge un valore fisso (+2, +4, +6, +8, +10).
/// Moltiplica: moltiplica la somma delle carte Numero (x2).
/// </summary>
public enum TipoModificatore
{
    Aggiungi,
    Moltiplica
}

/// <summary>
/// Tipi di carte azione speciale.
/// Congela: blocca il giocatore (esce dal round con i punti correnti).
/// PescaTreCarte: forza il giocatore a pescare 3 carte.
/// SecondaChance: salva da uno sballo una volta.
/// </summary>
public enum TipoAzione
{
    Congela,
    PescaTreCarte,
    SecondaChance
}

/// <summary>
/// La classe Carta rappresenta una singola carta del gioco Flip 7.
/// Ogni carta ha un Tipo (Numero/Modificatore/Azione) e,
/// a seconda del Tipo, un valore specifico opzionale.
/// 
/// Esempi:
///   - Numero 7: new Carta(TipoCarta.Numero, ValoreNumero.Sette)
///   - +4:       new Carta(TipoCarta.Modificatore, modificatore: TipoModificatore.Aggiungi, valoreModificatore: 4)
///   - Congela!: new Carta(TipoCarta.Azione, azione: TipoAzione.Congela)
/// </summary>
public class Carta
{
    /// <summary>Tipologia principale della carta.</summary>
    public TipoCarta Tipo { get; }

    /// <summary>Valore numerico (solo se Tipo == Numero, altrimenti null).</summary>
    public ValoreNumero? Numero { get; }

    /// <summary>Tipo di modificatore (solo se Tipo == Modificatore, altrimenti null).</summary>
    public TipoModificatore? Modificatore { get; }

    /// <summary>Valore del modificatore Aggiungi (es. 2 per +2, 4 per +4). Solo se Modificatore == Aggiungi.</summary>
    public int? ValoreModificatore { get; }

    /// <summary>Tipo di carta azione (solo se Tipo == Azione, altrimenti null).</summary>
    public TipoAzione? Azione { get; }

    public Carta(TipoCarta tipo,
                 ValoreNumero? numero = null,
                 TipoModificatore? modificatore = null,
                 int? valoreModificatore = null,
                 TipoAzione? azione = null)
    {
        Tipo = tipo;
        Numero = numero;
        Modificatore = modificatore;
        ValoreModificatore = valoreModificatore;
        Azione = azione;
    }

    /// <summary>
    /// Restituisce una rappresentazione testuale della carta.
    /// Utile per debug, log di gioco e test.
    /// </summary>
    public override string ToString()
    {
        return Tipo switch
        {
            TipoCarta.Numero => $"Numero {Numero}",
            TipoCarta.Modificatore => Modificatore switch
            {
                TipoModificatore.Aggiungi => $"+{ValoreModificatore}",
                TipoModificatore.Moltiplica => "x2",
                _ => "?"
            },
            TipoCarta.Azione => Azione.ToString(),
            _ => "?"
        };
    }
}
