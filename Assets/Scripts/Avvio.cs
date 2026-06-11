using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// AVVIO — Punto di ingresso del gioco Flip 7.
/// 
///  Si auto-crea all'avvio grazie a [RuntimeInitializeOnLoadMethod],
///    quindi NON serve creare GameObjects nella scena.
/// 
///  Gestisce 3 schermate:
///    - Menu: titolo, selezione giocatori (3-8), PLAY
///    - Gioco: info round, stati giocatori, log, PESCA/PASSA
///    - GameOver: vincitore, punteggi, torna al menu
/// 
///  Crea tutta la UI a runtime (Canvas, pannelli, testi, bottoni).
///  Nessuna scena o prefab richiesto.
/// </summary>
public class Avvio : MonoBehaviour
{
    // ═══════════════════════════════════════════════════════════════════════
    //  AVVIO AUTOMATICO
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Auto-creazione all'avvio del gioco.
    /// 
    /// [RuntimeInitializeOnLoadMethod] si esegue PRIMA del caricamento
    /// della prima scena. Crea un GameObject "Avvio" con questo script
    /// se non ne esiste già uno (evita duplicati al reload).
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoCreate()
    {
        if (FindFirstObjectByType<Avvio>() == null)
        {
            var go = new GameObject("Avvio");
            go.AddComponent<Avvio>();
            Object.DontDestroyOnLoad(go);
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  STATO INTERNO
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>Schermo attualmente visibile (Menu, Gioco o GameOver).</summary>
    private enum Schermo { Menu, Gioco, GameOver }
    private Schermo _schermoCorrente = Schermo.Menu;

    /// <summary>Motore di gioco (logica pura C#, nessuna dipendenza Unity).</summary>
    private MotoreDiGioco _gioco;

    /// <summary>Ultimo stato pubblico ricevuto dal motore (per display).</summary>
    private StatoPubblico _stato;

    /// <summary>Log degli eventi di gioco (pescate, passate, round, ecc.).</summary>
    private List<string> _logEventi = new List<string>();

    /// <summary>Numero di giocatori selezionato (default 4).</summary>
    private int _numeroGiocatori = 4;

    /// <summary>Indice del bottone giocatore selezionato (0 = 3 giocatori, 1 = 4, ...).</summary>
    private int _selezioneGiocatore = 1;

    // ── Riferimenti UI (creati a runtime) ──
    private GameObject _canvas;
    private GameObject _safeAreaContainer;  // Contiene i pannelli rispettando la safe area
    private GameObject _pannelloMenu;
    private GameObject _pannelloGioco;
    private GameObject _pannelloGameOver;

    private Button[] _btnGiocatori;     // 6 bottoni: 3, 4, 5, 6, 7, 8 giocatori
    private Button _btnPlay;            // Pulsante PLAY sul menu
    private Button _btnPesca;           // Pulsante PESCA durante il gioco
    private Button _btnPassa;           // Pulsante PASSA durante il gioco
    private Text _testoGiocatori;       // Stati e punteggi di tutti i giocatori
    private Text _testoLog;             // Log eventi (ultime 8 righe)
    private Text _testoInfo;            // Info sul round corrente
    private Text _testoGameOver;        // Messaggio finale (vincitore/punteggi)

    private Color _coloreNormale = new Color(0.2f, 0.3f, 0.4f);        // Grigio-blu per bottoni non selezionati
    private Color _coloreSelezionato = new Color(0.2f, 0.6f, 0.2f);    // Verde per bottone selezionato
    private Font _font;                 // Font caricato (con fallback)
    private bool _isPortrait;           // True se lo schermo è in verticale (mobile)

    /// <summary>Bottoni per la selezione bersaglio (Congela/Pesca 3).</summary>
    private List<Button> _bersaglioButtons;

    /// <summary>Pannello per la selezione bersaglio.</summary>
    private GameObject _pannelloBersaglio;

    // ═══════════════════════════════════════════════════════════════════════
    //  CICLO DI VITA UNITY
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Start() viene chiamato automaticamente da Unity dopo Awake().
    /// 
    /// 1. Carica il font (con fallback: LegacyRuntime.ttf → Arial.ttf)
    /// 2. Crea il Canvas
    /// 3. Mostra il Main Menu
    /// </summary>
    void Start()
    {
        Debug.Log("[Avvio] Avvio.Start() eseguito — creazione UI in corso...");

        // ═══ EVENT SYSTEM (ESSENZIALE per i click sui bottoni!) ═══
        // Senza EventSystem, i bottoni NON ricevono click.
        // Unity lo crea automaticamente solo nell'Editor, NON via codice.
        CreaEventSystem();

        // Carica font built-in di Unity, con fallback
        _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (_font == null)
        {
            _font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            Debug.Log("[Avvio] Font LegacyRuntime.ttf non trovato, uso Arial.ttf");
        }

        if (_font == null)
        {
            Debug.LogWarning("[Avvio] Nessun font trovato! I testi potrebbero non vedersi.");
        }

        CreaCanvas();
        Debug.Log("[Avvio] Canvas creato, creo menu...");
        CreaMenu();
        Debug.Log("[Avvio] Menu creato! Premi PLAY per iniziare.");
    }

    /// <summary>
    /// Update() viene chiamato ogni frame da Unity.
    /// Durante il gioco: aggiorna il display e controlla se il round è finito.
    /// Se il round è finito: conclude, passa al prossimo o mostra GameOver.
    /// </summary>
    void Update()
    {
        // Solo durante la schermata di gioco
        if (_schermoCorrente == Schermo.Gioco && _gioco != null)
        {
            // Aggiorna UI ogni frame (stati giocatori, log, info round)
            AggiornaDisplayGioco();

            // Se il round è finito (Flip7 attivato o nessun giocatore attivo)
            if (_gioco.RoundFinito)
            {
                _gioco.ConcludiRound();

                if (_gioco.PartitaFinita)
                {
                    // Qualcuno ha raggiunto 200+ punti → mostra GameOver
                    MostraGameOver();
                }
                else
                {
                    // Ancora nessun vincitore → avvia nuovo round
                    _logEventi.Add($"Round {_gioco.Partita.RoundCorrente} finito! Nuovo round...");
                    _gioco.AvviaRound();
                    AggiornaPulsanti();
                }
            }
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  EVENT SYSTEM — Necessario per intercettare i click sui bottoni
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Crea l'EventSystem se non esiste già.
    /// 
    /// L'EventSystem è il componente che processa gli input (click, tocchi,
    /// tastiera) e li instrada ai componenti UI (Button, Toggle, ecc.).
    /// 
    /// Senza EventSystem: i bottoni non rispondono a nessun click!
    /// In Unity, serve anche il StandaloneInputModule (input da mouse/tastiera).
    /// </summary>
    private void CreaEventSystem()
    {
        // Controlla se esiste già un EventSystem nella scena
        if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var go = new GameObject("EventSystem");
            go.AddComponent<UnityEngine.EventSystems.EventSystem>();
            // Il progetto usa il nuovo Input System Package → InputSystemUIInputModule
            go.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            Debug.Log("[Avvio] EventSystem + InputSystemUIInputModule creati (essenziale per i click!)");
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  CANVAS — Tela di fondo per tutta l'interfaccia UI
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Crea il Canvas principale (ScreenSpaceOverlay) con:
    /// - CanvasScaler: adatta la UI a qualsiasi risoluzione
    /// - GraphicRaycaster: necessario per intercettare i click sui bottoni
    /// - Sfondo scuro (Image) colore #14141F
    /// 
    /// Per mobile portrait usa reference 1080×1920 così il canvas riempie
    /// lo schermo in verticale (invece di essere compresso al centro).
    /// Per desktop/ultrawide usa reference 1920×1080.
    /// 
    /// Se esiste già un Canvas, lo distrugge prima (pulisce eventuali residui).
    /// </summary>
    private void CreaCanvas()
    {
        // Pulisce Canvas vecchi (utile se si rigenera)
        var oldCanvas = GameObject.Find("Canvas");
        if (oldCanvas != null) Destroy(oldCanvas);

        _canvas = new GameObject("Canvas");

        var c = _canvas.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;

        float screenRatio = Screen.width / (float)Screen.height;
        float refRatio = 1920f / 1080f; // 16:9

        _isPortrait = screenRatio < refRatio - 0.05f;

        // ── CanvasScaler con reference dinamica in base all'orientamento ──
        var scaler = _canvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

        if (_isPortrait)
        {
            // Mobile portrait: reference verticale 1080×1920, match=0 (larghezza)
            // Il canvas occupa tutta la larghezza e ~80% dell'altezza
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.0f;
        }
        else if (screenRatio > refRatio + 0.05f)
        {
            // Schermi ultrawide → scaling su altezza
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 1.0f;
        }
        else
        {
            // 16:9 classico → bilanciato
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
        }

        _canvas.AddComponent<GraphicRaycaster>();

        // Sfondo scuro minimal (raycastTarget = false così non blocca i click sui bottoni!)
        var sfondo = _canvas.AddComponent<Image>();
        sfondo.color = new Color(0.08f, 0.08f, 0.12f);
        sfondo.raycastTarget = false;

        // ── Contenitore Safe Area ──
        // Su mobile evita che UI venga coperta da notch, status bar, home indicator.
        // Su desktop non ha effetto (safe area = tutto schermo).
        _safeAreaContainer = new GameObject("SafeArea");
        _safeAreaContainer.transform.SetParent(_canvas.transform, false);
        var srt = _safeAreaContainer.AddComponent<RectTransform>();
        Rect sa = Screen.safeArea;
        srt.anchorMin = new Vector2(sa.xMin / Screen.width, sa.yMin / Screen.height);
        srt.anchorMax = new Vector2(sa.xMax / Screen.width, sa.yMax / Screen.height);
        srt.offsetMin = Vector2.zero;
        srt.offsetMax = Vector2.zero;
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  MAIN MENU — Schermata iniziale
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Crea la schermata del menu principale:
    /// - Titolo "FLIP 7" + sottotitolo "Riproduzione digitale"
    /// - Etichetta "Numero giocatori:"
    /// - 6 bottoni per selezionare 3-8 giocatori (quello selezionato è verde)
    /// - Pulsante PLAY per avviare la partita
    /// 
    /// Prima distrugge eventuali pannelli vecchi (menu, gioco, gameover).
    /// </summary>
    private void CreaMenu()
    {
        // Pulisce tutto prima di creare il menu
        DistruggiPannello(_pannelloMenu);
        DistruggiPannello(_pannelloGioco);
        DistruggiPannello(_pannelloGameOver);

        _pannelloMenu = CreaPannello("PanelMenu", true);
        var CT = _pannelloMenu.transform;

        // ── Layout adattivo: menu diverso per portrait (mobile) e landscape (desktop) ──
        if (_isPortrait)
        {
            // Portrait (1080×1920 reference) — menu distribuito verticalmente
            CreaTesto("Titolo", CT, "FLIP 7", 56, 0, 800, 500, 100);
            CreaTesto("Sottotitolo", CT, "Riproduzione digitale", 28, 0, 720, 500, 40);
            CreaTesto("Label", CT, "Numero giocatori:", 28, 0, 620, 400, 36);

            _btnGiocatori = new Button[6];
            int[] valori = { 3, 4, 5, 6, 7, 8 };
            for (int i = 0; i < 6; i++)
            {
                float x = (i - 2.5f) * 80f;
                int idx = i;
                _btnGiocatori[i] = CreaBottone(
                    $"BtnG{valori[i]}", CT, $"{valori[i]}",
                    x, 540, 70, 60,
                    i == 1 ? _coloreSelezionato : _coloreNormale
                );
                _btnGiocatori[i].onClick.AddListener(delegate { Seleziona(idx); });
            }

            _btnPlay = CreaBottone("BtnPlay", CT, "AVVIA PARTITA", 0, 380, 300, 65,
                new Color(0.15f, 0.5f, 0.15f));
            _btnPlay.onClick.AddListener(AvviaPartita);

            CreaBottone("BtnEsci", CT, "ESCI", 0, 240, 180, 50,
                new Color(0.5f, 0.15f, 0.15f)).onClick.AddListener(OnEsci);
        }
        else
        {
            // Landscape (1920×1080 reference) — menu standard
            CreaTesto("Titolo", CT, "FLIP 7", 56, 0, 120, 500, 100);
            CreaTesto("Sottotitolo", CT, "Riproduzione digitale", 28, 0, 60, 500, 40);
            CreaTesto("Label", CT, "Numero giocatori:", 28, 0, -10, 400, 36);

            _btnGiocatori = new Button[6];
            int[] valori = { 3, 4, 5, 6, 7, 8 };
            for (int i = 0; i < 6; i++)
            {
                float x = (i - 2.5f) * 80f;
                int idx = i;
                _btnGiocatori[i] = CreaBottone(
                    $"BtnG{valori[i]}", CT, $"{valori[i]}",
                    x, -90, 70, 60,
                    i == 1 ? _coloreSelezionato : _coloreNormale
                );
                _btnGiocatori[i].onClick.AddListener(delegate { Seleziona(idx); });
            }

            _btnPlay = CreaBottone("BtnPlay", CT, "AVVIA PARTITA", 0, -200, 300, 65,
                new Color(0.15f, 0.5f, 0.15f));
            _btnPlay.onClick.AddListener(AvviaPartita);

            CreaBottone("BtnEsci", CT, "ESCI", 0, -280, 180, 50,
                new Color(0.5f, 0.15f, 0.15f)).onClick.AddListener(OnEsci);
        }

        _schermoCorrente = Schermo.Menu;
    }

    /// <summary>
    /// Seleziona un numero di giocatori e aggiorna il colore dei bottoni.
    /// </summary>
    /// <param name="idx">Indice (0 = 3 giocatori, 1 = 4, ..., 5 = 8).</param>
    private void Seleziona(int idx)
    {
        _selezioneGiocatore = idx;
        _numeroGiocatori = idx + 3;   // Mappa indice → numero giocatori

        // Colora di verde il bottone selezionato, grigio gli altri
        for (int i = 0; i < _btnGiocatori.Length; i++)
        {
            if (_btnGiocatori[i] != null)
                _btnGiocatori[i].GetComponent<Image>().color =
                    i == idx ? _coloreSelezionato : _coloreNormale;
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  GIOCO — Schermata di gioco principale
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Avvia la partita: crea il MotoreDiGioco, distrugge il menu,
    /// crea l'interfaccia di gioco con tutti gli elementi UI.
    /// </summary>
    private void AvviaPartita()
    {
        // Pulisce log e inizializza il motore di gioco
        _logEventi.Clear();
        _bersaglioButtons = new List<Button>();
        _pannelloBersaglio = null;
        _gioco = new MotoreDiGioco();
        _gioco.AvviaPartita(_numeroGiocatori);
        _logEventi.Add($"Partita con {_numeroGiocatori} giocatori!");

        // Sostituisce il menu con la schermata di gioco
        DistruggiPannello(_pannelloMenu);
        _pannelloGioco = CreaPannello("PanelGioco", true);

        var PG = _pannelloGioco.transform;

        // ── Layout adattivo: posizioni diverse per portrait (mobile) e landscape (desktop) ──
        if (_isPortrait)
        {
            // ═══ Portrait (1080×1920 reference) — mobile ═══
            _testoInfo = CreaTesto("TestoInfo", PG,
                $"Round 1 | Giocatore 1 di turno", 36, 0, 780, 800, 55);
            _testoGiocatori = CreaTesto("TestoGiocatori", PG, "", 34, 0, 0, 700, 780);
            var rtG = _testoGiocatori.GetComponent<RectTransform>();
            rtG.anchorMin = new Vector2(0f, 0.5f);
            rtG.anchorMax = new Vector2(0f, 0.5f);
            rtG.pivot = new Vector2(0f, 0.5f);
            rtG.anchoredPosition = new Vector2(30f, 330f);
            _testoGiocatori.alignment = TextAnchor.MiddleLeft;
            _testoLog = CreaTesto("TestoLog", PG, "", 24, 0, -290, 700, 220);

            _btnPesca = CreaBottone("BtnPesca", PG, "PESCA", -90, -460, 130, 60,
                new Color(0.15f, 0.35f, 0.7f));  // Blu
            _btnPassa = CreaBottone("BtnPassa", PG, "PASSA", 90, -460, 130, 60,
                new Color(0.6f, 0.35f, 0.1f));    // Arancione

            _btnPesca.onClick.AddListener(OnPesca);
            _btnPassa.onClick.AddListener(OnPassa);

            // ESCI ben distanziato sotto i bottoni
            CreaBottone("BtnEsci", PG, "ESCI", 0, -580, 120, 45,
                new Color(0.5f, 0.15f, 0.15f)).onClick.AddListener(OnEsci);
        }
        else
        {
            // ═══ Landscape/ultrawide (1920×1080 reference) — desktop ═══
            _testoInfo = CreaTesto("TestoInfo", PG,
                $"Round 1 | Giocatore 1 di turno", 36, 0, 400, 900, 55);
            _testoGiocatori = CreaTesto("TestoGiocatori", PG, "", 34, 0, 0, 700, 580);
            var rtG = _testoGiocatori.GetComponent<RectTransform>();
            rtG.anchorMin = new Vector2(0f, 0.5f);
            rtG.anchorMax = new Vector2(0f, 0.5f);
            rtG.pivot = new Vector2(0f, 0.5f);
            rtG.anchoredPosition = new Vector2(30f, 30f);
            _testoGiocatori.alignment = TextAnchor.MiddleLeft;
            _testoLog = CreaTesto("TestoLog", PG, "", 24, 250, -200, 500, 300);

            _btnPesca = CreaBottone("BtnPesca", PG, "PESCA", -100, -390, 150, 65,
                new Color(0.15f, 0.35f, 0.7f));  // Blu
            _btnPassa = CreaBottone("BtnPassa", PG, "PASSA", 100, -390, 150, 65,
                new Color(0.6f, 0.35f, 0.1f));    // Arancione

            _btnPesca.onClick.AddListener(OnPesca);
            _btnPassa.onClick.AddListener(OnPassa);

            // ESCI ben distanziato sotto i bottoni
            CreaBottone("BtnEsci", PG, "ESCI", 0, -490, 140, 45,
                new Color(0.5f, 0.15f, 0.15f)).onClick.AddListener(OnEsci);
        }

        _schermoCorrente = Schermo.Gioco;
        AggiornaPulsanti();
    }

    /// <summary>
    /// Callback del pulsante PESCA.
    /// Il giocatore corrente pesca una carta dal mazzo.
    /// Se la carta richiede selezione bersaglio, viene mostrata l'UI di targeting.
    /// </summary>
    private void OnPesca()
    {
        if (_gioco == null || !_gioco.PuòPescare()) return;

        int g = _gioco.Round.GiocatoreCorrente + 1;
        _gioco.Pesca();

        if (_gioco.AzioneInAttesa != TipoAzioneInAttesa.Nessuna)
        {
            string nomeAzione = _gioco.AzioneInAttesa == TipoAzioneInAttesa.ScegliBersaglioCongela
                ? "CONGELA" : "PESCA 3";
            _logEventi.Add($"Giocatore {g} pesca [{nomeAzione}] — scegli il bersaglio!");
        }
        else
        {
            _logEventi.Add($"Giocatore {g} pesca.");
        }

        AggiornaPulsanti();
    }

    /// <summary>
    /// Callback del pulsante PASSA.
    /// Il giocatore corrente termina il suo turno volontariamente.
    /// </summary>
    private void OnPassa()
    {
        if (_gioco == null || !_gioco.PuòFermarsi()) return;

        int g = _gioco.Round.GiocatoreCorrente + 1;
        _gioco.Ferma();
        _logEventi.Add($"Giocatore {g} passa.");
        AggiornaPulsanti();
    }

    /// <summary>
    /// Aggiorna tutti gli elementi UI durante il gioco.
    /// Mostra:
    /// - Info round (numero round, giocatore di turno, mazziere)
    /// - Stati di tutti i giocatori con colori (Attivo=verde, Sballato=rosso, ecc.)
    /// - Log eventi (ultime 8 righe)
    /// </summary>
    private void AggiornaDisplayGioco()
    {
        _stato = _gioco.OttieniStatoPubblico();
        int corrente = _stato.GiocatoreCorrente + 1;
        int mazziere = _gioco.Partita.IndiceMazziere + 1;

        // ── Controlla se c'è un'azione in attesa di selezione bersaglio ──
        if (_gioco.AzioneInAttesa != TipoAzioneInAttesa.Nessuna)
        {
            string azioneNome = _gioco.AzioneInAttesa == TipoAzioneInAttesa.ScegliBersaglioCongela
                ? "CONGELA" : "PESCA 3";
            _testoInfo.text = $"<b>Scegli il bersaglio per {azioneNome}!</b>";
            MostraBersagli();
            return;
        }
        else if (_pannelloBersaglio != null)
        {
            // Pulisce la UI di targeting se non più necessaria
            PulisciBersagli();
        }

        // Riga informativa in alto
        _testoInfo.text = $"Round {_stato.RoundCorrente + 1} | " +
                          $"Giocatore {corrente} di turno | " +
                          $"Mazziere: Giocatore {mazziere}";

        // Stati giocatori (colonna sinistra)
        _testoGiocatori.text = "";
        for (int i = 0; i < _stato.PunteggiTotali.Length; i++)
        {
            // Colore in base allo stato
            string col = _stato.StatiGiocatori[i] switch
            {
                "Attivo"    => "#33cc33",    // Verde
                "Sballato"  => "#cc3333",    // Rosso
                "Congelato" => "#3399dd",    // Azzurro
                "Fermo"     => "#cccc33",    // Giallo
                _           => "#fff"
            };

            string pre = (i == _stato.GiocatoreCorrente) ? ">> " : "   ";

            // Mostra il riepilogo giocatore
            string l = $"{pre}G{i + 1}: <color={col}>{_stato.StatiGiocatori[i]}</color> " +
                       $"Carte:{_stato.ConteggioNumeri[i]} " +
                       $"Punti:{_stato.PunteggiPotenziali[i]} " +
                       $"Tot:{_stato.PunteggiTotali[i]}";

            if (i == _stato.GiocatoreCorrente)
                l = $"<b>{l}</b>";   // Grassetto per il giocatore corrente

            _testoGiocatori.text += l + "\n";

            // Mostra le carte pescate (se il giocatore ha almeno una carta)
            if (!string.IsNullOrEmpty(_stato.CarteDescrizioni[i]))
            {
                _testoGiocatori.text += $"      <size=26>{_stato.CarteDescrizioni[i]}</size>\n";
            }

            // Senza blank line extra per risparmiare spazio verticale
        }

        // Log eventi (colonna destra, ultime 8 righe)
        int start = Mathf.Max(0, _logEventi.Count - 8);
        _testoLog.text = string.Join("\n", _logEventi.Skip(start));
    }

    /// <summary>
    /// Abilita/disabilita i pulsanti PESCA e PASSA in base allo stato
    /// del giocatore corrente (es. disabilita "Pesca" se sballato).
    /// Disabilita entrambi se c'è un'azione in attesa di selezione bersaglio.
    /// </summary>
    private void AggiornaPulsanti()
    {
        bool targetingInCorso = _gioco != null && _gioco.AzioneInAttesa != TipoAzioneInAttesa.Nessuna;

        if (_btnPesca != null)
            _btnPesca.interactable = !targetingInCorso && _gioco.PuòPescare();

        if (_btnPassa != null)
            _btnPassa.interactable = !targetingInCorso && _gioco.PuòFermarsi();
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  ESC — Esce dall'applicazione
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Chiude l'applicazione. In Editor mostra un log (Application.Quit
    /// non funziona in Editor).
    /// </summary>
    private void OnEsci()
    {
        Debug.Log("[Avvio] Uscita richiesta dall'utente.");
        Application.Quit();
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  SELEZIONE BERSAGLIO — UI per Congela e Pesca 3
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Mostra i bottoni per selezionare il bersaglio dell'azione in attesa.
    /// Crea un pannello temporaneo con un bottone per ogni bersaglio disponibile.
    /// </summary>
    private void MostraBersagli()
    {
        if (_gioco == null || _pannelloGioco == null) return;

        int[] bersagli = _gioco.BersagliDisponibili;
        if (bersagli == null || bersagli.Length == 0) return;

        // Se il pannello esiste già, non ricreare
        if (_pannelloBersaglio != null) return;

        string azioneNome = _gioco.AzioneInAttesa == TipoAzioneInAttesa.ScegliBersaglioCongela
            ? "CONGELA" : "PESCA 3";

        // Crea pannello temporaneo per i bottoni bersaglio
        _pannelloBersaglio = new GameObject("PanelBersaglio");
        _pannelloBersaglio.transform.SetParent(_pannelloGioco.transform, false);
        var rt = _pannelloBersaglio.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);

        // Posiziona sopra i bottoni PESCA/PASSA
        float yBase = _isPortrait ? -350f : -280f;
        rt.anchoredPosition = new Vector2(0f, yBase);

        // Crea un bottone per ogni bersaglio disponibile
        _bersaglioButtons = new List<Button>();
        float startX = -(bersagli.Length - 1) * 45f;

        for (int i = 0; i < bersagli.Length; i++)
        {
            int targetIdx = bersagli[i];
            int giocatoreNum = targetIdx + 1;

            var btn = CreaBottone(
                $"BtnBersaglio{giocatoreNum}",
                _pannelloBersaglio.transform,
                $"G{giocatoreNum}",
                startX + i * 90f, 0f, 75f, 55f,
                _gioco.AzioneInAttesa == TipoAzioneInAttesa.ScegliBersaglioCongela
                    ? new Color(0.5f, 0.3f, 0.7f)  // Viola per Congela
                    : new Color(0.7f, 0.5f, 0.2f)   // Arancione per Pesca 3
            );

            btn.onClick.AddListener(delegate
            {
                if (_gioco != null)
                {
                    string azioneLog = _gioco.AzioneInAttesa == TipoAzioneInAttesa.ScegliBersaglioCongela
                        ? "congela" : "Pesca 3 su";
                    int mittente = _gioco.Round.GiocatoreCorrente + 1;
                    _gioco.SelezionaBersaglio(targetIdx);
                    _logEventi.Add($"Giocatore {mittente} {azioneLog} Giocatore {giocatoreNum}!");
                    AggiornaPulsanti();
                }
            });

            _bersaglioButtons.Add(btn);
        }
    }

    /// <summary>
    /// Pulisce la UI di selezione bersaglio (distrugge pannello e bottoni).
    /// </summary>
    private void PulisciBersagli()
    {
        if (_pannelloBersaglio != null)
        {
            Destroy(_pannelloBersaglio);
            _pannelloBersaglio = null;
        }
        if (_bersaglioButtons != null)
        {
            _bersaglioButtons.Clear();
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  GAME OVER — Schermata finale
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Mostra la schermata di fine partita:
    /// - Titolo "PARTITA FINITA!"
    /// - Nome del vincitore e punteggio
    /// - Punteggi di tutti i giocatori
    /// - Pulsante "MENU PRINCIPALE" per tornare al menu
    /// </summary>
    private void MostraGameOver()
    {
        DistruggiPannello(_pannelloGioco);
        _pannelloGameOver = CreaPannello("PanelGameOver", true);
        var CT = _pannelloGameOver.transform;

        // Trova vincitore
        int v = _gioco.VincitoreIndice.Value + 1;
        int p = _gioco.Partita.PunteggiTotali[_gioco.VincitoreIndice.Value];

        // Messaggi
        CreaTesto("Titolo", CT, "PARTITA FINITA!", 48, 0, 100, 600, 80);
        CreaTesto("Vincitore", CT, $"Giocatore {v} vince con {p} punti!", 32, 0, 0, 600, 80);

        // Tabella punteggi finali
        string pts = "";
        for (int i = 0; i < _gioco.Partita.PunteggiTotali.Length; i++)
            pts += $"Giocatore {i + 1}: {_gioco.Partita.PunteggiTotali[i]} pt\n";

        CreaTesto("Punteggi", CT, pts, 22, 0, -80, 500, 140);

        // Bottone per tornare al menu
        var btnMenu = CreaBottone("BtnMenu", CT, "MENU PRINCIPALE",
            0, -210, 320, 65, new Color(0.6f, 0.2f, 0.2f));  // Rosso scuro
        btnMenu.onClick.AddListener(CreaMenu);

        _schermoCorrente = Schermo.GameOver;
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  HELPER UI — Metodi di utilità per creare elementi UI
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Crea un pannello (GameObject con RectTransform) come figlio del Canvas.
    /// Il pannello riempie tutto lo schermo (anchorMin=0,0 anchorMax=1,1).
    /// </summary>
    /// <param name="nome">Nome del GameObject.</param>
    /// <param name="attivo">Se true, il pannello è visibile all'avvio.</param>
    /// <returns>Il GameObject del pannello creato.</returns>
    private GameObject CreaPannello(string nome, bool attivo)
    {
        var obj = new GameObject(nome);
        // I pannelli sono figli del SafeArea container (non del Canvas direttamente)
        obj.transform.SetParent(_safeAreaContainer.transform, false);
        obj.SetActive(attivo);

        var rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        return obj;
    }

    /// <summary>
    /// Distrugge un pannello se esiste (controllo null safe).
    /// </summary>
    private void DistruggiPannello(GameObject p)
    {
        if (p != null) Destroy(p);
    }

    /// <summary>
    /// Crea un GameObject con componente Text per visualizzare testo.
    /// Ancorato al centro con posizione e dimensione specificate.
    /// </summary>
    /// <param name="nome">Nome del GameObject.</param>
    /// <param name="parent">Transform genitore.</param>
    /// <param name="testo">Testo da visualizzare.</param>
    /// <param name="fontSize">Dimensione del font.</param>
    /// <param name="x">Posizione X (ancorata al centro).</param>
    /// <param name="y">Posizione Y (ancorata al centro).</param>
    /// <param name="w">Larghezza del rettangolo.</param>
    /// <param name="h">Altezza del rettangolo.</param>
    /// <returns>Il componente Text creato.</returns>
    private Text CreaTesto(string nome, Transform parent, string testo, int fontSize,
        float x, float y, float w, float h)
    {
        var obj = new GameObject(nome);
        obj.transform.SetParent(parent, false);

        var rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta = new Vector2(w, h);

        var txt = obj.AddComponent<Text>();
        txt.text = testo;
        txt.fontSize = fontSize;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = Color.white;

        if (_font != null) txt.font = _font;

        return txt;
    }

    /// <summary>
    /// Crea un pulsante completo (Image + Button + Text figlio).
    /// Il pulsante è ancorato al centro con posizione e dimensione specificate.
    /// </summary>
    /// <param name="nome">Nome del GameObject.</param>
    /// <param name="parent">Transform genitore.</param>
    /// <param name="testo">Testo del pulsante.</param>
    /// <param name="x">Posizione X.</param>
    /// <param name="y">Posizione Y.</param>
    /// <param name="w">Larghezza.</param>
    /// <param name="h">Altezza.</param>
    /// <param name="colore">Colore di sfondo del pulsante.</param>
    /// <returns>Il componente Button creato.</returns>
    private Button CreaBottone(string nome, Transform parent, string testo,
        float x, float y, float w, float h, Color colore)
    {
        var obj = new GameObject(nome);
        obj.transform.SetParent(parent, false);

        var rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta = new Vector2(w, h);

        // Sfondo del pulsante
        var img = obj.AddComponent<Image>();
        img.color = colore;

        // Componente Button con colori hover/pressed
        var btn = obj.AddComponent<Button>();
        var colors = btn.colors;
        colors.highlightedColor = new Color(1, 1, 1, 0.2f);  // Sfuma al passaggio del mouse
        colors.pressedColor = new Color(1, 1, 1, 0.4f);       // Più chiaro quando premuto
        btn.colors = colors;

        // Etichetta del pulsante (figlio "Testo" che riempie tutto il bottone)
        var to = new GameObject("Testo");
        to.transform.SetParent(obj.transform, false);

        var tr = to.AddComponent<RectTransform>();
        tr.anchorMin = Vector2.zero;
        tr.anchorMax = Vector2.one;
        tr.offsetMin = Vector2.zero;
        tr.offsetMax = Vector2.zero;

        var txt = to.AddComponent<Text>();
        txt.text = testo;
        txt.fontSize = 36;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = Color.white;
        txt.fontStyle = FontStyle.Bold;

        if (_font != null) txt.font = _font;

        return btn;
    }
}
