using System.Collections.Generic;
using System;

/// <summary>
/// MAZZO — Gestisce le 94 carte del gioco Flip 7.
/// 
/// Composizione:
/// - 79 carte Numero (0×1, 1×1, 2×2, 3×3, 4×4, 5×5, 6×6, 7×7, 8×8, 9×9, 10×10, 11×11, 12×12)
/// - 6 carte Modificatore (5 × Aggiungi: +2/+4/+6/+8/+10, 1 × Moltiplica: x2)
/// - 3 carte Azione (Congela, PescaTreCarte, SecondaChance)
/// Totale: 79 + 6 + 3 = 88... 
/// 
/// In realtà la distribuzione corretta è:
/// - 1 carta "0", 1 carta "1", 2 carte "2", 3 carte "3", ..., 12 carte "12" = 1+1+2+3+4+5+6+7+8+9+10+11+12 = 79
/// - 5 modificatori +X + 1 x2 = 6
/// - 3 azioni
/// Totale = 79 + 6 + 3 = 88 carte
/// 
/// Funzionalità:
/// - Costruzione automatica delle carte
/// - Mescola (Fisher-Yates shuffle)
/// - Pesca con rimescolo automatico dagli scarti
/// </summary>
public class Mazzo
{
    /// <summary>Mazzo principale (Stack: pesca dalla cima).</summary>
    private Stack<Carta> _mazzo;

    /// <summary>Carte scartate (per rimescolo quando il mazzo finisce).</summary>
    private List<Carta> _scarti;

    /// <summary>Generatore di numeri casuali per la mescola.</summary>
    private Random _rng;

    // ═══════════════════════════════════════════════════════════════════════
    //  COSTRUTTORE
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Costruttore: costruisce tutte le 88 carte e le mescola.
    /// Il mazzo è subito pronto per l'uso.
    /// </summary>
    public Mazzo()
    {
        _mazzo = new Stack<Carta>();
        _scarti = new List<Carta>();
        _rng = new Random();
        CostruisciCarte();
        Mescola();
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  PROPRIETÀ E METODI PUBBLICI
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>Quante carte rimangono nel mazzo (da pescare).</summary>
    public int Rimanenti => _mazzo.Count;

    /// <summary>
    /// Pesca una carta dalla cima del mazzo.
    /// 
    ///  Se il mazzo è vuoto, rimescola automaticamente dagli scarti
    ///    (lancia eccezione solo se anche gli scarti sono vuoti).
    /// </summary>
    /// <returns>La carta pescata.</returns>
    public Carta Pesca()
    {
        if (_mazzo.Count == 0)
            RimescolaDaScarti();

        return _mazzo.Pop();
    }

    /// <summary>
    /// Scarta una carta (aggiunge alla pila degli scarti).
    /// Le carte scartate verranno rimescolate nel mazzo quando finisce.
    /// </summary>
    /// <param name="carta">La carta da scartare.</param>
    public void Scarta(Carta carta)
    {
        _scarti.Add(carta);
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  METODI PRIVATI
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Costruisce tutte le 88 carte del mazzo Flip 7.
    /// 
    /// Carte Numero (79 totali):
    /// - 1 carta "0", 1 carta "1", poi per i valori 2-12: N carte del valore N
    /// 
    /// Carte Modificatore (6 totali):
    /// - +2, +4, +6, +8, +10 (Aggiungi)
    /// - x2 (Moltiplica)
    /// 
    /// Carte Azione (3 totali):
    /// - Congela, PescaTreCarte, SecondaChance
    /// </summary>
    private void CostruisciCarte()
    {
        // ── Carte Numero (0-12) ──
        // Distribuzione: 1 carta per "0", 1 per "1", 2 per "2", ..., 12 per "12"
        for (int i = 0; i <= 12; i++)
        {
            int count = i == 0 ? 1 : i;  // "0" ha 1 carta, "1" ha 1 carta, "2" ha 2, ...
            for (int j = 0; j < count; j++)
            {
                _mazzo.Push(new Carta(TipoCarta.Numero, (ValoreNumero)i));
            }
        }

        // ── Carte Modificatore ──
        int[] valoriAggiunta = { 2, 4, 6, 8, 10 };
        foreach (var val in valoriAggiunta)
        {
            _mazzo.Push(new Carta(
                TipoCarta.Modificatore,
                modificatore: TipoModificatore.Aggiungi,
                valoreModificatore: val));
        }

        _mazzo.Push(new Carta(
            TipoCarta.Modificatore,
            modificatore: TipoModificatore.Moltiplica));

        // ── Carte Azione ──
        _mazzo.Push(new Carta(TipoCarta.Azione, azione: TipoAzione.Congela));
        _mazzo.Push(new Carta(TipoCarta.Azione, azione: TipoAzione.PescaTreCarte));
        _mazzo.Push(new Carta(TipoCarta.Azione, azione: TipoAzione.SecondaChance));
    }

    /// <summary>
    /// Mescola il mazzo usando l'algoritmo Fisher-Yates (Knuth shuffle).
    /// Complessità: O(n) tempo, O(1) spazio extra.
    /// </summary>
    private void Mescola()
    {
        var array = _mazzo.ToArray();
        int n = array.Length;

        // Fisher-Yates: per ogni posizione, scambia con una casuale precedente
        for (int i = n - 1; i > 0; i--)
        {
            int j = _rng.Next(0, i + 1);
            // Scambia array[i] con array[j] (sintassi C# tuple)
            (array[i], array[j]) = (array[j], array[i]);
        }

        _mazzo.Clear();
        foreach (var carta in array)
        {
            _mazzo.Push(carta);
        }
    }

    /// <summary>
    /// Rimescola tutte le carte dagli scarti e le rimette nel mazzo.
    /// Lancia un'eccezione solo se anche gli scarti sono vuoti.
    /// </summary>
    private void RimescolaDaScarti()
    {
        if (_scarti.Count == 0)
            throw new InvalidOperationException("Non ci sono carte da rimescolare.");

        var array = _scarti.ToArray();
        int n = array.Length;

        // Fisher-Yates shuffle
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
