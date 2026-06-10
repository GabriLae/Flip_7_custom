// Flip Seven (Flip 7) — GameCore
// Modello dati delle carte del gioco.
// Tutte le classi in Core/ sono C# puro (nessun using UnityEngine).

/// <summary>
/// Categorie principali di carte nel mazzo Flip 7.
/// Number: carte numerate 0-12. Causano Bust se duplicate.
/// Modifier: modificano il punteggio (+X o per 2). Non causano Bust.
/// Action: carte effetto speciale (Freeze, Flip Three, Second Chance).
/// </summary>
public enum CardKind
{
    Number,
    Modifier,
    Action,
}

/// <summary>
/// Valori possibili per le Number card (0-12).
/// La distribuzione nel mazzo è: 1 carta "0", 1 carta "1",
/// 2 carte "2", 3 carte "3", ..., 12 carte "12".
/// </summary>

public enum NumberValue
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
/// Add: aggiunge un valore fisso (+2, +4, +6, +8, +10).
/// Multiply: moltiplica la somma delle Number card (x2).
/// </summary>

public enum ModifierKind
{
    Add,
    Multiply
}

/// <summary>
/// Tipi di carte azione speciale.
/// Freeze: blocca il giocatore (esce dal round con i punti correnti).
/// FlipThree: forza il giocatore a pescare 3 carte.
/// SecondChance: salva da un Bust una volta.
/// </summary>

public enum ActionKind
{
    Freeze,
    FlipThree,
    SecondChance
}

/// <summary>
/// La classe Card rappresenta una singola carta del gioco Flip 7.
/// Ogni carta ha un Kind (Number/Modifier/Action) e,
/// a seconda del Kind, un valore specifico opzionale.
/// 
/// Esempi:
///   - Number 7: new Card(CardKind.Number, NumberValue.Sette)
///   - +4:      new Card(CardKind.Modifier, modifier: ModifierKind.Add, modifierValue: 4)
///   - Freeze!: new Card(CardKind.Action, action: ActionKind.Freeze)
/// </summary>

public class Card
{
    /// <summary>Tipologia principale della carta.</summary>
    public CardKind Kind { get; }
    /// <summary>Valore numerico (solo se Kind == Number, altrimenti null).</summary>
    public NumberValue? Number { get; }
    /// <summary>Tipo di modificatore (solo se Kind == Modifier, altrimenti null).</summary>
    public ModifierKind? Modifier { get; }
    /// <summary>Valore del modificatore Add (es. 2 per +2, 4 per +4). Solo se Modifier == Add.</summary>
    public int? ModifierValue { get; }
    /// <summary>Tipo di carta azione (solo se Kind == Action, altrimenti null).</summary>
    public ActionKind? Action { get; }
     public Card(CardKind kind,
                NumberValue? number = null,
                ModifierKind? modifier = null,
                int? modifierValue = null,
                ActionKind? action = null)
    {
        Kind = kind;
        Number = number;
        Modifier = modifier;
        ModifierValue = modifierValue;
        Action = action;
    }
    /// <summary>
    /// Restituisce una rappresentazione testuale della carta.
    /// Utile per debug, log di gioco e test.
    /// </summary>
    public override string ToString()
    {
        return Kind switch
        {
            CardKind.Number => $"Number {Number}",
            CardKind.Modifier => Modifier switch
            {
                ModifierKind.Add => $"+{ModifierValue}",
                ModifierKind.Multiply => "x2",
                _ => "?"
            },
            CardKind.Action => Action.ToString(),
            _ => "?"
        };
    }
 

}