// Flip Seven (Flip 7) — GameCore
// Modello dati delle carte del gioco.
// Tutte le classi in Core/ sono C# puro (nessuna dipendenza da UnityEngine).

// ═══════════════════════════════════════════════════════════════════════════
//  ENUM — Tipi e valori delle carte
// ═══════════════════════════════════════════════════════════════════════════

/// <summary>
/// Categorie principali di carte nel mazzo Flip 7.
/// 
/// - Numero: carte numerate da 0 a 12. Causano sballo se pescate in duplicato.
/// - Modificatore: modificano il punteggio (+X o x2). Non causano sballo.
/// - Azione: carte con effetto speciale (Congela, PescaTreCarte, SecondaChance).
/// </summary>
public enum TipoCarta
{
    /// <summary>Carta numerata 0-12. Causa sballo se duplicata.</summary>
    Numero,

    /// <summary>Carta modificatore: +X (somma) o x2 (moltiplica). Non causa sballo.</summary>
    Modificatore,

    /// <summary>Carta azione speciale: Congela, PescaTreCarte o SecondaChance.</summary>
    Azione,
}

/// <summary>
/// Valori possibili per le carte Numero (da 0 a 12).
/// 
/// Distribuzione nel mazzo:
/// - "0": 1 carta
/// - "1": 1 carta
/// - "2": 2 carte
/// - "3": 3 carte
/// - ...
/// - "12": 12 carte
/// </summary>
public enum ValoreNumero
{
    Zero = 0, Uno = 1, Due = 2, Tre = 3, Quattro = 4,
    Cinque = 5, Sei = 6, Sette = 7, Otto = 8, Nove = 9,
    Dieci = 10, Undici = 11, Dodici = 12
}

/// <summary>
/// Tipi di carte modificatore di punteggio.
/// - Aggiungi: aggiunge un valore fisso al punteggio (+2, +4, +6, +8, +10).
/// - Moltiplica: raddoppia la somma delle carte Numero (x2).
/// </summary>
public enum TipoModificatore
{
    /// <summary>Aggiunge un valore fisso (+2, +4, +6, +8, +10).</summary>
    Aggiungi,

    /// <summary>Moltiplica la somma delle carte Numero (x2).</summary>
    Moltiplica
}

/// <summary>
/// Tipi di carte azione speciale.
/// - Congela: il giocatore viene bloccato ed esce dal round con i punti correnti.
/// - PescaTreCarte: costringe a pescare 3 carte di fila (le Congela vengono differite).
/// - SecondaChance: salva da uno sballo una volta sola.
/// </summary>
public enum TipoAzione
{
    /// <summary>Blocca il giocatore — esce dal round con i punti correnti.</summary>
    Congela,

    /// <summary>Forza il giocatore a pescare 3 carte consecutive.</summary>
    PescaTreCarte,

    /// <summary>Salva da uno sballo (si consuma dopo l'uso).</summary>
    SecondaChance
}

// ═══════════════════════════════════════════════════════════════════════════
//  CLASSE CARTA
// ═══════════════════════════════════════════════════════════════════════════

/// <summary>
/// CARTA — Modello dati di una singola carta del gioco Flip 7.
/// 
/// Ogni carta ha:
/// - Un Tipo obbligatorio (Numero, Modificatore o Azione)
/// - Valori specifici opzionali a seconda del Tipo
/// 
/// Esempi:
/// <code>
///   var cartaNumero = new Carta(TipoCarta.Numero, ValoreNumero.Sette);        // Numero 7
///   var cartaMod    = new Carta(TipoCarta.Modificatore, modificatore: TipoModificatore.Aggiungi, valoreModificatore: 4);  // +4
///   var cartaAzione = new Carta(TipoCarta.Azione, azione: TipoAzione.Congela); // Congela!
/// </code>
/// </summary>
public class Carta
{
    /// <summary>Tipologia principale della carta (Numero, Modificatore, Azione).</summary>
    public TipoCarta Tipo { get; }

    /// <summary>
    /// Valore numerico (solo se Tipo == Numero, altrimenti null).
    /// Esempio: ValoreNumero.Sette per la carta "7".
    /// </summary>
    public ValoreNumero? Numero { get; }

    /// <summary>
    /// Tipo di modificatore (solo se Tipo == Modificatore, altrimenti null).
    /// Esempio: TipoModificatore.Aggiungi per "+4".
    /// </summary>
    public TipoModificatore? Modificatore { get; }

    /// <summary>
    /// Valore del modificatore Aggiungi (solo se Modificatore == Aggiungi).
    /// Esempio: 4 per la carta "+4", 10 per "+10".
    /// </summary>
    public int? ValoreModificatore { get; }

    /// <summary>
    /// Tipo di carta azione (solo se Tipo == Azione, altrimenti null).
    /// Esempio: TipoAzione.Congela per la carta "Congela!".
    /// </summary>
    public TipoAzione? Azione { get; }

    /// <summary>
    /// Costruttore completo. Usa parametri opzionali con nomi per chiarezza.
    /// </summary>
    /// <param name="tipo">Tipo principale della carta (obbligatorio).</param>
    /// <param name="numero">Se Tipo == Numero, il valore numerico.</param>
    /// <param name="modificatore">Se Tipo == Modificatore, il tipo (Aggiungi/Moltiplica).</param>
    /// <param name="valoreModificatore">Se Modificatore == Aggiungi, il valore (+X).</param>
    /// <param name="azione">Se Tipo == Azione, il tipo di azione.</param>
    public Carta(
        TipoCarta tipo,
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
    /// Restituisce una rappresentazione testuale leggibile della carta.
    /// Utile per debug, log di gioco e test.
    /// 
    /// Esempi:
    ///   - "Numero 7"
    ///   - "+4"
    ///   - "x2"
    ///   - "Congela"
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
