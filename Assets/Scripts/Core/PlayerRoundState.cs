using System.Collections.Generic;

/// <summary>
/// Stato di un giocatore durante un round.
/// </summary>
public enum PlayerStatus
{
    Active,      // può ancora pescare
    Frozen,      // bloccato da carta Freeze
    Bust,        // ha sforato (numero duplicato)
    Stand        // si è fermato volontariamente
}

public class PlayerRoundState
{
    // ── Proprietà readonly ──
    public PlayerStatus Status { get; private set; }
    public bool HasSecondChance { get; private set; }   // parte a true

    // Carte pescate, divise per tipo
    private List<NumberValue> numeri;   // numeri pescati
    private int modificatoreSomma;             // somma dei +X
    private bool haMoltiplicatore;          // ha per 2?
    private int contoAzioni;             // quante carte azione pescate (minimo 7 per +15)

    // ── Costruttore ──
    public PlayerRoundState()
    {
        Status = PlayerStatus.Active;
        HasSecondChance = true;
        numeri = new List<NumberValue>();
        modificatoreSomma = 0;
        haMoltiplicatore = false;
        contoAzioni = 0;

    }
    public bool IsBust => Status == PlayerStatus.Bust;
    public bool CanDraw => Status == PlayerStatus.Active;

    // ── Metodi per aggiornare lo stato del giocatore ──
    public void AddCard(Card card)
    {
        /*AddCard(Card card)  — il più importante
    Devi distinguere i 3 casi con  switch  su  card.Kind :
    - CardKind.Number: controlla se  numeri.Contains(card.Number.Value)  → se sì,  Status = PlayerStatus.Bust . Altrimenti aggiungi alla lista.
    - CardKind.Modifier: controlla  card.Modifier :
    -  ModifierKind.Add  →  modificatoreSomma += card.ModifierValue.Value 
    -  ModifierKind.Multiply  →  haMoltiplicatore = true 
    - CardKind.Action:  contoAzioni++ , poi controlla  card.Action :
    -  ActionKind.Freeze  → chiama  Freeze()  (o imposta Status direttamente)
    -  ActionKind.FlipThree  → per ora non fare nulla (lo gestirà RoundState)
    -  ActionKind.SecondChance  → non fare nulla (si usa al Bust)*/

        switch (card.Kind)
        {
            case CardKind.Number:
                if (numeri.Contains(card.Number.Value))
                {
                    Status = PlayerStatus.Bust;
                }
                else
                {
                    numeri.Add(card.Number.Value);
                }
                break;

            case CardKind.Modifier:
                if (card.Modifier.Type == ModifierKind.Add)
                {
                    modificatoreSomma += card.Modifier.Value;
                }
                else if (card.Modifier.Type == ModifierKind.Multiply)
                {
                    haMoltiplicatore = true;
                }
                break;

            case CardKind.Action:
                contoAzioni++;
                if (card.Action.Type == ActionKind.Freeze)
                {
                    Freeze();
                }
                // FlipThree e SecondChance gestiti altrove
                break;
        }

    }
    public void UseSecondChance() // se ha SecondChance, lo consuma e torna Active (da Bust)
    {
        if (HasSecondChance && Status == PlayerStatus.Bust)
        {
            HasSecondChance = false;
            Status = PlayerStatus.Active;
        }
    }

    public void Freeze() // imposta lo status == Frozen
    {
        Status = PlayerStatus.Frozen;
    }

    }

    public void Stand() // imposta lo status == Stand
    {
        Status = PlayerStatus.Stand;
    }

    public IReadOnlyList<NumberValue> Numeri => numeri.AsReadOnly();
    // restituisce i numeri pescati (readonly)

}