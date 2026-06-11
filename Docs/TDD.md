# Flip Seven (Flip 7) — TDD (MVP Hotseat, Unity + GameCore)

Questo documento definisce l'MVP "minimo indispensabile" e la sequenza di test per sviluppare un GameCore in C# puro (senza dipendenze Unity), con presentazione Unity sottile.

## 1. Obiettivo MVP

- Modalità: **hotseat 3–8 giocatori**
- Target: riproduzione fedele delle regole; interfaccia minima ma leggibile
- Approccio: **GameCore testabile** + Unity come layer di input/rendering

## 2. Scopo (MVP)

### 2.1 Include

- Flusso round a turni: giocatore attivo sceglie `Pesca` oppure `Ferma`
- Gestione completa carte:
  - Numero 0–12 con distribuzione del mazzo "1..12 copie + 1 zero"
  - Modificatori `+2/+4/+6/+8/+10` e `x2`
  - Azioni `Congela`, `PescaTreCarte`, `SecondaChance`
- Regole complete:
  - Sballo su duplicato Numero
  - Bonus "Flip 7": 7 Numeri unici → fine round immediata +15
  - Calcolo punteggio round (ordine corretto)
  - Condizioni fine round e fine partita (200+ a fine round)
  - Mazziere che passa a sinistra; rimescolo quando il mazzo finisce

### 2.2 Setup "minimale"

- Nessuna "distribuzione iniziale" in interfaccia: il round parte già in stato `DecisioneGiocatore`
- I giocatori possono partire con **0 carte**
- Un giocatore può scegliere `Ferma` solo se ha almeno una carta davanti a sé (Numero o Modificatore)

## 3. Non inclusi (fuori scopo MVP)

- Multiplayer online, bot/IA
- Tutorial, salvataggi, replay
- Animazioni obbligatorie, effetti audio
- Interfaccia avanzata (drag&drop carte, layout tavolo complesso)

## 4. Architettura (confine per test)

### 4.1 GameCore (testabile)

- Libreria C# "pura": niente `UnityEngine`
- Responsabilità: regole, stato, risoluzione azioni, punteggio, gestione mazzo/scarti, determinismo

**Per tre motivi pratici** (entrambi legati al TDD), non perché "non si possa" usare `UnityEngine`:

1. **Test più semplici e veloci**: se il GameCore non dipende da `UnityEngine`, i test girano come normali unit test .NET (rapidissimi, senza scene, senza Play Mode, senza timing/frame)
2. **Codice più pulito**: Unity spinge verso `MonoBehaviour`, lifecycle (`Update`, `Start`), riferimenti a scene/Prefab e side effect. Per le regole di un gioco di carte conviene avere funzioni pure e stato esplicito: è più facile da debuggare e mantenere
3. **Separazione responsabilità**: Unity gestisce interfaccia/input/animazioni; il core gestisce solo "regole e stato". Così puoi cambiare interfaccia o engine senza riscrivere la logica
4. **Determinismo**: nel core puoi controllare il generatore casuale o il mazzo con seed o lista di carte nei test; con Unity spesso finisci a usare API o timing che complicano la riproducibilità

### 4.2 UnityPresentation (non testabile / minimale)

- Mostra stato e registro eventi
- Raccoglie input e invoca comandi del core

### 4.3 Scelta Implementativa: Ibrido Unity-first

Scelta adottata per questo progetto (didattica + portfolio pubblico):

Tutto il codice risiede dentro `Assets/Scripts/` di Unity, ma con **separazione logica** rigorosa:

```
Assets/Scripts/
├── Core/                          ← C# puro (nessun using UnityEngine)
│   ├── Carta.cs                    (enums + class, non MonoBehaviour)
│   ├── Mazzo.cs                    (class, mazzo 94 carte, Fisher-Yates)
│   ├── StatoRoundGiocatore.cs      (class, stato giocatore nel round)
│   ├── CalcolatorePunteggio.cs     (static class, calcolo punteggi)
│   └── StatoRound.cs                (class, orchestrazione round)
│
├── Presentation/                  ← MonoBehaviour, input, rendering
│   └── GameManager.cs             (MonoBehaviour → futura chiamata al GameEngine)
│
└── Tests/                         ← Test in Edit Mode (Unity Test Framework)
    └── GameCoreTests.cs           (NUnit, nessun Play Mode)

> **Nota:** i file `GameEngine.cs` e `MatchState.cs` verranno aggiunti in futuro seguendo gli stessi princìpi.

**Perché questa scelta:**

- Il GameCore rimane puro C# (niente `MonoBehaviour`, niente `Update()`) → testabile, deterministico
- Ma è tutto dentro Unity → il professore e chi guarda il portfolio vede un progetto Unity completo
- Il layer Presentation impara Unity sul serio (`MonoBehaviour`, `SerializeField`, canvas, event system)
- I test in Edit Mode girano senza Play Mode → quasi veloci come `dotnet test` ma dentro Unity

Tutti i princìpi delle sezioni 4.1 e 4.2 rimangono identici. Cambia solo la posizione fisica dei file.

## 5. Modello dati (alto livello)

### 5.1 Tipi base

- `Carta` con `TipoCarta`: `Numero | Modificatore | Azione`
- `ValoreNumero`: `Zero | Uno | Due | ... | Dodici` (0–12)
- `TipoModificatore`: `Aggiungi(+2/+4/+6/+8/+10) | Moltiplica(x2)`
- `TipoAzione`: `Congela | PescaTreCarte | SecondaChance`

### 5.2 Stato partita/round

- `StatoPartita` (futuro): `Giocatori[]`, `PunteggiTotali[]`, `PunteggioObiettivo=200`
- `StatoRound` (implementato):
  - `IndiceMazziere`, `IndiceTurno`
  - `Mazzo` (Stack), `PilaScarti` (List)
  - `StatiGiocatori[]` (per-round)
  - `CodaRisoluzione` per gestire azioni "posticipate" (specie Flip Three)
- `StatoRoundGiocatore` (implementato):
  - `Stato`: `Attivo | Congelato | Sballato | Fermo`
  - `Numeri` (insieme readonly dei valori Numero posseduti)
  - `ModificatoreSomma` (int)
  - `HaMoltiplicatore` (bool)
  - `ContoAzioni` (int)
  - `HaSecondaChance` (bool)

## 6. Comandi (API di alto livello del Core — futuri)

- `AvviaPartita(giocatori)`
- `AvviaRound(statoPartita, indiceMazziere, seedMazzo)`
- `PuòFermarsi(giocatore)` e `PuòPescare(giocatore)`
- `Pesca()` → pesca 1 carta e risolve effetti
- `Ferma()` → imposta giocatore come `Fermo`
- `OttieniStatoPubblico()` → istantanea leggibile per interfaccia

## 7. Regole (invarianti da testare)

- I duplicati si controllano solo tra Numeri del giocatore, non su Azione/Modificatore
- I Modificatori non possono causare sballo e non contano per Flip 7
- `x2` moltiplica solo la somma dei Numeri (non +X, non +15)
- Seconda Chance:
  - previene uno sballo da duplicato Numero una sola volta (consumo della carta)
  - max 1 per giocatore: se ne riceve un'altra e ci sono altri Attivi senza, può passarla; altrimenti viene scartata
  - viene scartata a fine round anche se non usata
- Flip Three (Pesca Tre):
  - forza 3 pescate (Numero/Azione/Modificatore contano come "una delle 3")
  - interrompe anticipatamente se il giocatore fa Flip 7 o sballa
  - eventuali Azioni uscite durante le 3 pescate si risolvono dopo aver completato le 3 pescate (se non sballato)
- Congela (Freeze):
  - forza l'uscita dal round (equivalente a `Ferma` forzato), con punti calcolati a fine round

## 8. Strategia di test

### 8.1 Framework: Unity Test Framework (Edit Mode)

- I test usano **Unity Test Framework** (NUnit) in **Edit Mode**
- Non serve Play Mode: le classi Core sono pure C#, si testano senza scene né MonoBehaviour
- Setup: Unity → Window → General → Test Runner → Create EditMode Test Assembly Folder in `Assets/Scripts/Tests/`
- Unity chiederà automaticamente di creare un assembly definition (`.asmdef`) per l'assembly dei test. Accetta. Se non lo fa, crealo manualmente: tasto destro su `Assets/Scripts/Tests/` → Create → Assembly Definition

### 8.2 Determinismo

- Per i test che coinvolgono il mazzo, si può creare un `Mazzo` partendo da una lista di carte predefinita
- Il costruttore deterministico può essere aggiunto in futuro quando servirà testare scenari specifici (es. `Mazzo(List<Carta> carte)` per simulare situazioni precise)

### 8.3 Granularità

- Test unitari su singole regole e transizioni di stato
- Test di "flusso" su round completo (mini-simulazioni)

## 9. Casi di Test (ordine consigliato)

### 9.1 Mazzo e Carte

- `Mazzo_CostruisceDistribuzioneCorretta_94Carte`
- `Mazzo_PescaSpostaAlGiocatore_NonAgliScarti`
- `Rimescola_QuandoMazzoVuoto_UsaPilaScarti`
- `SecondaChance_SeAttiva_PrevieneSballoSuDuplicato`

### 9.2 Punteggio

- `Punteggio_NessunaCarta_Zero`
- `Punteggio_SoloSommaNumeri`
- `Punteggio_X2_MoltiplicaSoloSommaNumeri`
- `Punteggio_ModificatoriAdd_DopoX2`
- `Punteggio_Flip7_Aggiunge15_DopoModificatori`

### 9.3 Regole Turno (Pesca/Ferma)

- `NonPuòFermarsi_SenzaCarte`
- `Ferma_ImpostaGiocatoreFermo`
- `RoundTermina_QuandoNessunGiocatoreAttivo`

### 9.4 Sballo e Seconda Chance

- `Sballo_NumeroDuplicato_ImpostaSballato_EPunteggioZero`
- `SecondaChance_PreventivaSballo_ConsumaCarta`
- `SecondaChance_NonPrevieneCongelaOAltriEventi`
- `SecondaChance_MaxUnoPerGiocatore_PassaOScarto`
- `SecondaChance_ScartataAFineRound`

### 9.5 Bonus Flip 7

- `Flip7_SetteNumeriUnici_RoundTerminaImmediatamente`
- `Flip7_Bonus15_AssegnatoSoloAChiCompleta`
- `Flip7_IgnoraModificatoriEAzioniPerConteggio`

### 9.6 Carte Azione

- `Congela_ForzaGiocatoreFuoriDalRound`
- `PescaTreCarte_PescaEsattamenteTreCarte_SalvoSballoOFlip7`
- `PescaTreCarte_RimandaAzioniDopoTrePescate`
- `PescaTreCarte_SeAzioneDurantePescata_PuòBersagliareQualsiasiAttivo`

### 9.7 Ciclo di Vita Round

- `FineRound_PunteggioSoloFermiOCongelati_NonSballati`
- `MazzierePassaSinistra_OgniRound`
- `PartitaTermina_QuandoGiocatoreRaggiunge200_FineRound`
- `PartitaTermina_PareggioOltre200_ContinuaFinchéSingoloVincitore`

## 10. Criteri di Accettazione (Definition of Done MVP)

- Avvio partita hotseat con 3–8 giocatori e tabellone punteggi totale persistente
- Round giocabile end-to-end con interfaccia minima (Pesca/Ferma) e registro eventi
- Tutte le regole elencate nello scopo sono implementate nel GameCore
- Test del GameCore:
  - coprono i casi di Sballo, Seconda Chance, PescaTreCarte, Congela, Flip 7, ordine punteggio, fine round, passaggio mazziere, rimescolo
  - sono deterministici (nessun test flaky)

> **Test flaky**: un test del software che restituisce risultati incoerenti (a volte positivi, a volte falliti) anche se il codice sottostante non è stato modificato. Poiché producono "falsi allarmi", minano la fiducia degli sviluppatori nelle pipeline di integrazione continua (CI) e sprecano tempo prezioso per il debug.

## 11. Verifica Manuale (controllo rapido)

- Simulare uno sballo e verificare 0 punti
- Simulare Seconda Chance su duplicato e verificare che non sballi
- Simulare PescaTreCarte con sballo alla 2ª pescata
- Simulare Flip 7 durante PescaTreCarte (alla 1ª o 2ª pescata) e verificare fine round immediata
- Verificare punteggio con Numero + x2 + +X + Flip7 (+15)
