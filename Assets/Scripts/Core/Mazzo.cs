using System.Collections.Generic;
using System;

/// <summary>
/// Mazzo da 94 carte del gioco Flip 7.
/// Gestisce costruzione, mescola, pesca, scarti e rimescolo automatico.
/// </summary>
public class Mazzo
{
    private Stack<Carta> _mazzo;
    private List<Carta> _scarti;
    private Random _rng;

    /// <summary>
    /// Costruttore: costruisce le 94 carte, le mescola.
    /// </summary>
    public Mazzo()
    {
        _mazzo = new Stack<Carta>();
        _scarti = new List<Carta>();
        _rng = new Random();
        CostruisciCarte();
        Mescola();
    }

    /// <summary>
    /// Quante carte rimangono nel mazzo.
    /// </summary>
    public int Rimanenti => _mazzo.Count;

    /// <summary>
    /// Pesca una carta dal mazzo.
    /// Se il mazzo è vuoto, rimescola dagli scarti.
    /// </summary>
    public Carta Pesca()
    {
        if (_mazzo.Count == 0)
            RimescolaDaScarti();

        return _mazzo.Pop();
    }

    /// <summary>
    /// Scarta una carta (aggiunge alla pila scarti).
    /// </summary>
    public void Scarta(Carta carta)
    {
        _scarti.Add(carta);
    }

    // ── Metodi privati ──

    private void CostruisciCarte()
    {
        // Costruisce le carte Numero (0-12)
        for (int i = 0; i <= 12; i++)
        {
            int count = i == 0 ? 1 : i; // 1 carta "0", 1 carta "1", 2 carte "2", ..., 12 carte "12"
            for (int j = 0; j < count; j++)
            {
                _mazzo.Push(new Carta(TipoCarta.Numero, (ValoreNumero)i));
            }
        }

        // Costruisce le carte Modificatore
        int[] valoriAggiunta = { 2, 4, 6, 8, 10 };
        foreach (var val in valoriAggiunta)
        {
            _mazzo.Push(new Carta(TipoCarta.Modificatore, modificatore: TipoModificatore.Aggiungi, valoreModificatore: val));
        }

        _mazzo.Push(new Carta(TipoCarta.Modificatore, modificatore: TipoModificatore.Moltiplica));

        // Costruisce le carte Azione
        _mazzo.Push(new Carta(TipoCarta.Azione, azione: TipoAzione.Congela));
        _mazzo.Push(new Carta(TipoCarta.Azione, azione: TipoAzione.PescaTreCarte));
        _mazzo.Push(new Carta(TipoCarta.Azione, azione: TipoAzione.SecondaChance));
    }

    private void Mescola()
    {
        // Algoritmo Fisher-Yates (Knuth shuffle) — O(N), O(1) spazio
        var array = _mazzo.ToArray();
        int n = array.Length;

        for (int i = n - 1; i > 0; i--)
        {
            int j = _rng.Next(0, i + 1);
            // Scambia array[i] con array[j] (sintassi C# moderna)
            (array[i], array[j]) = (array[j], array[i]);
        }

        _mazzo.Clear();
        foreach (var carta in array)
        {
            _mazzo.Push(carta);
        }
    }

    private void RimescolaDaScarti()
    {
        if (_scarti.Count == 0)
            throw new InvalidOperationException("Non ci sono carte da rimescolare.");

        var array = _scarti.ToArray();
        int n = array.Length;

        for (int i = n - 1; i > 0; i--)
        {
            int j = _rng.Next(0, i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }

        foreach (var carta in array)
        {
            _mazzo.Push(carta);
        }

        _scarti.Clear();
    }
}
