using System.Collections.Generic;  // serve per Stack e List
using System; // serve per Random e InvalidOperationException
public class Deck
{
    private Stack<Card> mazzo;
    private List<Card> scarti;
    private Random rng;

    /// <summary>
    /// Costruttore: costruisce le 94 carte, le mescola.
    /// </summary>
    public Deck()
    {
        mazzo = new Stack<Card>(); // Dichiarazione: crea un mazzo di carte vuoto, Stack struttura LIFO (Last In First Out)

        scarti = new List<Card>(); // Dichiarazione: crea una lista per gli scarti, List struttura dinamica che permette di aggiungere e rimuovere elementi

        rng = new Random(); // Crea un'istanza di Random per generare numeri casuali

        BuildCards(); // Costruisce le carte del mazzo
        Shuffle(); // Mescola le carte del mazzo

    }

    /// <summary>
    /// Quante carte rimangono nel mazzo.
    /// </summary>
    public int Remaining => mazzo.Count;

    /// <summary>
    /// Pesca una carta dal mazzo.
    /// Se il mazzo è vuoto, fa reshuffle dagli scarti.
    /// </summary>
    public Card Draw()
    {
        if (mazzo.Count == 0)
            ReshuffleFromDiscard();

        return mazzo.Pop();
    }

    /// <summary>
    /// Scarta una carta (aggiunge alla pila scarti).
    /// </summary>
    public void Discard(Card card)
    {
        scarti.Add(card);
    }

    //Metodi privati
    private void BuildCards()
    {
        // Costruisce le carte Number (0-12)
        for (int i = 0; i <= 12; i++)
        {
            int count = i == 0 ? 1 : i; // 1 carta "0", 2 carte "2", ..., 12 carte "12"
            for (int j = 0; j < count; j++)
            {
                mazzo.Push(new Card(CardKind.Number, (NumberValue)i)); // Aggiunge una carta Number al mazzo
            }
        }

        // Costruisce le carte Modifier
        int[] addValues = { 2, 4, 6, 8, 10 };
        foreach (var val in addValues)
        {
            mazzo.Push(new Card(CardKind.Modifier, modifier: ModifierKind.Add, modifierValue: val)); // Aggiunge una carta Modifier Add al mazzo
        }

        mazzo.Push(new Card(CardKind.Modifier, modifier: ModifierKind.Multiply)); // Aggiunge una carta Modifier Multiply al mazzo

        // Costruisce le carte Action
        mazzo.Push(new Card(CardKind.Action, action: ActionKind.Freeze)); // Aggiunge una carta Action Freeze al mazzo
        mazzo.Push(new Card(CardKind.Action, action: ActionKind.FlipThree)); // Aggiunge una carta Action FlipThree al mazzo
        mazzo.Push(new Card(CardKind.Action, action: ActionKind.SecondChance)); // Aggiunge una carta Action SecondChance al mazzo
    }
    private void Shuffle()
    {
        // Mescola le carte del mazzo usando l'algoritmo di Fisher-Yates
        /*L'algoritmo Fisher-Yates (conosciuto anche come Knuth shuffle) è il metodo più efficiente ed equo per mescolare in modo casuale gli elementi di un array o di una lista. Esso garantisce che 
        ogni possibile sequenza finale abbia la stessa probabilità di verificarsi (permutazione uniforme).Come funzionaLa versione moderna (ottimizzata da Richard Durstenfeld e Donald Knuth) lavora "in-place",
        ovvero modifica l'array originale senza richiederne uno nuovo in memoria. Partenza: Si posiziona all'ultimo elemento dell'array.Estrazione: Si genera un indice casuale compreso tra \(0\) e l'indice dell'elemento corrente.
        Scambio: L'elemento corrente viene scambiato con l'elemento che si trova all'indice casuale estratto. Iterazione: Si ripete il processo spostandosi all'elemento precedente, fino a raggiungere il primo elemento.
        ComplessitàTempo: \(\mathcal{O}(N)\), dove \(N\) è il numero di elementi. Esegue un solo ciclo attraverso la struttura.Spazio: \(\mathcal{O}(1)\), poiché effettua lo "scambio" direttamente negli slot di memoria esistenti.*/

        var cardsArray = mazzo.ToArray(); // Converte lo Stack in un array per poterlo mescolare
        int n = cardsArray.Length;

        for (int i = n - 1; i > 0; i--)
        {
            int j = rng.Next(0, i + 1); // Genera un indice casuale tra 0 e i
            // Scambia cardsArray[i] con cardsArray[j]
            var temp = cardsArray[i];
            cardsArray[i] = cardsArray[j];
            cardsArray[j] = temp;
        }

        // Ricostruisce lo Stack con le carte mescolate
        mazzo.Clear(); // Svuota lo Stack
        foreach (var card in cardsArray)
        {
            mazzo.Push(card); // Aggiunge le carte mescolate allo Stack
        }
    }
    private void ReshuffleFromDiscard()
    {
        if (scarti.Count == 0)
            throw new InvalidOperationException("Non ci sono carte da reshufflare.");

        // Mescola gli scarti e li sposta nel mazzo
        var scartiArray = scarti.ToArray(); // Converte la lista degli scarti in un array
        int n = scartiArray.Length;

        for (int i = n - 1; i > 0; i--)
        {
            int j = rng.Next(0, i + 1); // Genera un indice casuale tra 0 e i
            // Scambia scartiArray[i] con scartiArray[j]
            var temp = scartiArray[i];
            scartiArray[i] = scartiArray[j];
            scartiArray[j] = temp;
        }

        // Sposta le carte mescolate dallo scarto al mazzo
        foreach (var card in scartiArray)
        {
            mazzo.Push(card); // Aggiunge le carte mescolate allo Stack del mazzo
        }

        scarti.Clear(); // Svuota la lista degli scarti
    }
}