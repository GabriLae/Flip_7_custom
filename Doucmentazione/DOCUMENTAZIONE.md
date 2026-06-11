# Flip 7 — Documentazione del Progetto

**Autore:** Gabriele Laera
**Corso:** Game Developer, Modulo Code Design
**Piattaforma:** Unity 6 (C#)
**Versione:** 1.0

## Come eseguire il progetto

1. **Aprire il progetto** in Unity 6 (o superiore)
2. **Premere Play** su **qualsiasi scena** (anche una scena vuota)
3. Lo script `Avvio.cs` si auto-crea grazie a `[RuntimeInitializeOnLoadMethod]`
4. **Selezionare il numero di giocatori** cliccando i bottoni `[3]` `[4]` `[5]` `[6]` `[7]` `[8]`
5. **Cliccare PLAY** per iniziare la partita
6. Usare **PESCA** e **FERMA** durante il gioco

> **Nota:** Non è necessario creare scene, prefab o GameObjects manualmente. Il gioco funziona in una scena completamente vuota.

## 1. Architettura Generale

Il progetto è suddiviso in **tre livelli logici** ben separati:

- **Livello Presentazione (Interfaccia Unity):** `Avvio.cs` — Crea UI, gestisce input, mostra dati
- **Livello Facade:** `MotoreDiGioco.cs` — Coordina tutta la logica di gioco, unico punto di contatto per l'UI
- **Livello Core:** `StatoPartita` (persistente), `StatoRound` (turni), `StatoRoundGiocatore` (stato individuale), `Mazzo` (88 carte), `Carta` (modello), `CalcolatorePunteggio` (formula)

**Principio fondamentale:** L'interfaccia Unity (Avvio) interagisce **solo** con `MotoreDiGioco`. Mai direttamente con `StatoRound`, `Mazzo`, ecc. Questo è il **pattern Facade**.

## 2. Dipendenze tra le classi

- **Avvio** dipende da: `MotoreDiGioco`, `StatoPubblico`
- **MotoreDiGioco** dipende da: `StatoPartita`, `StatoRound`, `Mazzo`, `CalcolatorePunteggio`
- **StatoPartita** non dipende da nessuna classe
- **StatoRound** dipende da: `Mazzo`, `StatoRoundGiocatore`, `Carta`, `CalcolatorePunteggio`
- **StatoRoundGiocatore** dipende da: `Carta`
- **Mazzo** dipende da: `Carta`
- **Carta** non dipende da nessuna classe
- **CalcolatorePunteggio** dipende da: `StatoRoundGiocatore`

## 3. Avvio.cs

### Avvio.cs — Ruolo

Punto di ingresso del gioco. È l'unico script che interagisce con il motore Unity.

### Avvio.cs — Responsabilità

- **Auto-creazione:** Usa l'attributo `[RuntimeInitializeOnLoadMethod]` per creare il proprio GameObject all'avvio del gioco, anche in una scena vuota.
- **Creazione UI:** Genera a runtime l'intera interfaccia (Canvas, EventSystem, pannelli, testi, bottoni).
- **Gestione stati:** Transizione tra 3 schermate (Menu, Gioco, GameOver).
- **Collegamento eventi:** Associa listener `onClick` ai bottoni per chiamare i metodi del `MotoreDiGioco`.
- **Loop di aggiornamento:** In `Update()`, chiama `AggiornaDisplayGioco()` e controlla fine round.

### Avvio.cs — Metodi principali

- `AutoCreate()` — Statico, eseguito prima del caricamento della scena. Crea il GameObject
- `Start()` — Carica font, crea EventSystem, Canvas e Menu
- `Update()` — Aggiorna display e gestisce fine round
- `CreaEventSystem()` — Crea EventSystem + InputSystemUIInputModule
- `CreaCanvas()` — Crea Canvas con scaler, raycaster e sfondo scuro
- `CreaMenu()` — Costruisce schermata iniziale con titolo e bottoni
- `Seleziona(idx)` — Cambia numero giocatori e aggiorna colore bottoni
- `AvviaPartita()` — Inizializza MotoreDiGioco e crea interfaccia di gioco
- `OnPesca()` / `OnFerma()` — Callback pulsanti, delegano a MotoreDiGioco
- `AggiornaDisplayGioco()` — Legge StatoPubblico e aggiorna schermo
- `MostraGameOver()` — Costruisce schermata finale con vincitore e punteggi

### Avvio.cs — Perché esiste

Separa la logica di gioco (C# puro) dalla presentazione Unity. Per cambiare motore grafico basterebbe riscrivere solo questo file.

## 4. MotoreDiGioco.cs

### MotoreDiGioco.cs — Ruolo

**Facciata (Facade)** del sistema di gioco. Coordina tutte le classi del core.

### MotoreDiGioco.cs — Responsabilità

- Espone interfaccia unificata: `AvviaPartita`, `Pesca`, `Ferma`, `ConcludiRound`, `OttieniStatoPubblico`.
- Delega le operazioni a `StatoRound`, `StatoPartita`, `Mazzo`.
- Contiene la classe **`StatoPubblico`** (DTO — Data Transfer Object).

### MotoreDiGioco.cs — Metodi principali

- `AvviaPartita(n)` — Crea StatoPartita e avvia primo round
- `AvviaRound()` — Crea nuovo Mazzo + StatoRound
- `PuòPescare()` / `PuòFermarsi()` — Delegati a StatoRound
- `Pesca()` / `Ferma()` — Delegati a StatoRound
- `ConcludiRound()` — Calcola punteggi, aggiorna totali, controlla vincitore
- `OttieniStatoPubblico()` — Compila DTO con dati per UI

### MotoreDiGioco.cs — Perché esiste

Nasconde la complessità del sistema. L'UI parla solo con `MotoreDiGioco`, non con `StatoRound`, `StatoPartita`, `Mazzo`.

---

## 5. StatoPartita.cs

### StatoPartita.cs — Ruolo

Mantiene lo stato **persistente su più round**: punteggi totali, round corrente, mazziere, condizione di vittoria.

### StatoPartita.cs — Enum StatoPartitaEnum

- `InCorso` — la partita continua
- `Finita` — un giocatore ha raggiunto l'obiettivo (200 punti)

### StatoPartita.cs — Metodi principali

- `AggiornaPunteggi(punteggiRound)` — Somma punteggi round ai totali
- `ControllaVincitore()` — Verifica se qualcuno ha 200+ punti
- `TrovaIndicePunteggioMassimo()` — In caso di pareggio, chi ha il punteggio più alto

### StatoPartita.cs — Perché esiste

Separa la logica di fine partita (200 punti) dalla logica di turno. `StatoPartita` non sa nulla di carte, mazzi o pescate.

## 6. StatoRound.cs

### StatoRound.cs — Ruolo

Gestisce il **round corrente**: turni, pescate, azioni speciali e condizioni di fine round.

### StatoRound.cs — Enum StatoPescaTreCarte

- `Nessuno` — nessuno in modalità PescaTreCarte
- `InCorso` — il giocatore corrente è in modalità PescaTreCarte

### StatoRound.cs — Metodi principali

- `Pesca()` — Gestisce pescata normale o PescaTreCarte con azioni differite
- `Ferma()` — Ferma volontariamente il giocatore corrente
- `PuòPescare()` — Controlla se il giocatore puo pescare
- `PuòFermarsi()` — Controlla se può fermarsi
- `CalcolaPunteggi()` — Calcola punteggi finali di tutti i giocatori
- `PassaAlProssimoGiocatore()` — Cerca prossimo giocatore Attivo (ordine circolare)

### StatoRound.cs — Regole di fine round

Il round finisce quando:

1. Un giocatore ha **7 numeri unici** (Flip7!)
2. **Nessuno** è più nello stato "Attivo"

### StatoRound.cs — Perché esiste

Incapsula la logica del turno in una classe dedicata, separata dalla partita e dal singolo giocatore.

## 7. StatoRoundGiocatore.cs

### StatoRoundGiocatore.cs — Ruolo

Stato **individuale di un giocatore** durante il round. Tiene traccia di carte, modificatori, stato e SecondaChance.

### StatoRoundGiocatore.cs — Enum StatoGiocatore

- `Attivo` — può pescare o fermarsi
- `Congelato` — bloccato da Congela, esce con punti correnti
- `Sballato` — numero duplicato, esce con 0 punti
- `Fermo` — si è fermato volontariamente, conserva punti

### StatoRoundGiocatore.cs — Metodi principali

- `AggiungiCarta(carta)` — Applica effetto carta (Numero: aggiungi o sballo; Modificatore: somma/x2; Azione: Congela)
- `UsaSecondaChance()` — Consuma SecondaChance per tornare Attivo
- `Congela()` — Imposta stato Congelato
- `Ferma()` — Imposta stato Fermo

### StatoRoundGiocatore.cs — Regole importanti

- Se si pesca un **numero già posseduto** — Sballato
- **SecondaChance** consumata evita lo sballo
- **Modificatori** (+X, x2) non causano sballo
- **Azioni** Congela bloccano il giocatore

### StatoRoundGiocatore.cs — Perché esiste

Ogni giocatore ha bisogno di uno stato indipendente invece di array paralleli.

## 8. Carta.cs

### Carta.cs — Ruolo

Modello dati di una carta. Definisce gli enum per tipi di carta e la classe `Carta`.

### Carta.cs — Enum definiti

- `TipoCarta` — `Numero`, `Modificatore`, `Azione`
- `ValoreNumero` — Da `Zero` (0) a `Dodici` (12)
- `TipoModificatore` — `Aggiungi` (+X), `Moltiplica` (x2)
- `TipoAzione` — `Congela`, `PescaTreCarte`, `SecondaChance`

### Carta.cs — Metodi

- **Costruttore** — Parametri opzionali nominati per chiarezza
- `ToString()` — Rappresentazione testuale per debug e log

### Carta.cs — Perché esiste

Incapsula le informazioni di una carta in un unico oggetto, più leggibile di array paralleli o tuple.

## 9. Mazzo.cs

### Mazzo.cs — Ruolo

Costruisce, mescola e gestisce il mazzo da 88 carte del gioco Flip 7.

### Mazzo.cs — Composizione del mazzo

- **Numero:** 79 carte (1x0, 1x1, 2x2, 3x3, ..., 12x12)
- **Modificatore:** 6 carte (+2, +4, +6, +8, +10, x2)
- **Azione:** 3 carte (Congela, PescaTreCarte, SecondaChance)
- **Totale:** 88 carte

### Mazzo.cs — Metodi principali

- `Pesca()` — Preleva carta in cima (rimescola scarti se vuoto)
- `Scarta(carta)` — Aggiunge carta alla pila scarti
- `CostruisciCarte()` — Genera 88 carte con distribuzione corretta
- `Mescola()` — Fisher-Yates shuffle, O(n)
- `RimescolaDaScarti()` — Rimescola scarti nel mazzo

### Mazzo.cs — Perché esiste

Centralizza gestione mazzo, rendendo il codice indipendente dalla distribuzione carte.

## 10. CalcolatorePunteggio.cs

### CalcolatorePunteggio.cs — Ruolo

Classe **statica** che calcola il punteggio di un giocatore a fine round.

### CalcolatorePunteggio.cs — Metodi

- `CalcolaPunteggio(stato)` — Applica le 5 regole e restituisce il punteggio

### CalcolatorePunteggio.cs — Formula

Se Sballato -> 0
Altrimenti -> (SommaNumeri * (x2 ? 2 : 1)) + Modificatori + (Azioni >= 7 ? 15 : 0)

### CalcolatorePunteggio.cs — Regole (in ordine)

1. **Sballato** — punteggio 0
2. **Somma** valori carte Numero
3. **x2** — raddoppia la somma
4. **Aggiungi** modificatori +X
5. **7+ Azioni** — bonus +15 (Flip7!)

### CalcolatorePunteggio.cs — Perché esiste

Separa la logica di calcolo. Classe statica facilmente testabile e riutilizzabile.

## 11. Pattern e Scelte Progettuali

### Pattern Facade

`MotoreDiGioco` funge da facciata. L'UI chiama solo `MotoreDiGioco`, che coordina `StatoPartita`, `StatoRound`, `Mazzo` e `CalcolatorePunteggio`.

**Vantaggio:** Se la logica interna cambia, l'UI non va modificata.

### Pattern DTO (Data Transfer Object)

`StatoPubblico` trasporta dati dal motore all'UI senza esporre lo stato interno.

### Separazione UI / Logica

Le classi Core sono C# puro (nessun `using UnityEngine`). Solo `Avvio.cs` dipende da Unity.

**Vantaggio:** Il core è riutilizzabile su altre piattaforme.

### Auto-creazione con RuntimeInitializeOnLoadMethod

`Avvio` usa l'attributo per crearsi automaticamente — basta premere Play.

## 12. Flusso di Esecuzione

1. [RuntimeInitializeOnLoadMethod] -> AutoCreate()
2. Start() -> CreaEventSystem() -> CreaCanvas() -> CreaMenu()
3. [Utente clicca bottone giocatori] -> Seleziona(idx)
4. [Utente clicca PLAY] -> AvviaPartita()
5. MotoreDiGioco.AvviaPartita(giocatori) -> Crea StatoPartita + AvviaRound()
6. [Ogni frame] AggiornaDisplayGioco() <- OttieniStatoPubblico()
7. [Utente clicca PESCA/FERMA] -> MotoreDiGioco.Pesca()/Ferma()
8. [Round finito] -> ConcludiRound() -> Aggiorna Punteggi -> Controlla Vincitore
9. [Vincitore!] -> MostraGameOver() -> [MENU PRINCIPALE] -> CreaMenu()
