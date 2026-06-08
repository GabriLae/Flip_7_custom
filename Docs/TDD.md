# Flip Seven (Flip 7) — TDD (MVP Hotseat, Unity + GameCore)

Questo documento definisce l’MVP “minimo indispensabile” e la sequenza di test per sviluppare un GameCore in C# puro (senza dipendenze Unity), con presentazione Unity sottile.

## 1. Obiettivo MVP
- Modalità: **hotseat 3–8 giocatori**
- Target: riproduzione fedele delle regole; UI minima ma leggibile
- Approccio: **GameCore testabile** + Unity come layer di input/rendering

## 2. Scope (MVP)
### 2.1 Include
- Flusso round a turni: giocatore attivo sceglie `Hit` oppure `Stay`
- Gestione completa carte:
  - Number 0–12 con distribuzione del mazzo “1..12 copie + 1 zero”
  - Modifiers `+2/+4/+6/+8/+10` e `x2`
  - Actions `Freeze`, `Flip Three`, `Second Chance`
- Regole complete:
  - Bust su duplicato Number
  - Bonus “Flip 7”: 7 Number uniche → fine round immediata +15
  - Calcolo punteggio round (ordine corretto)
  - Condizioni fine round e fine partita (200+ a fine round)
  - Dealer che passa a sinistra; reshuffle quando il deck finisce

### 2.2 Setup “minimale”
- Nessun “deal iniziale” in UI: il round parte già in stato `PlayerDecision`
- I giocatori possono partire con **0 carte**
- Un giocatore può scegliere `Stay` solo se ha almeno una carta davanti a sé (Number o Modifier)

## 3. Non-goals (fuori scope MVP)
- Multiplayer online, bot/AI
- Tutorial, salvataggi, replay
- Animazioni obbligatorie, effetti audio
- UI avanzata (drag&drop carte, layout tavolo complesso)

## 4. Architettura (boundary per test)
### 4.1 GameCore (testabile)
- Libreria C# “pura”: niente `UnityEngine`
- Responsabilità: regole, stato, risoluzione azioni, scoring, gestione deck/discard, determinismo

Per due motivi pratici (entrambi legati al TDD), non perché “non si possa” usare UnityEngine.

Test più semplici e veloci: se il GameCore non dipende da UnityEngine, i test girano come normali unit test .NET (rapidissimi, senza scene, senza Play Mode, senza timing/frame).
Codice più pulito: Unity ti spinge verso MonoBehaviour, lifecycle (Update, Start), riferimenti a scene/Prefab, e side effects. Per le regole di un gioco di carte conviene avere funzioni pure e stato esplicito: è più facile da debuggare e mantenere.
Separazione responsabilità: Unity gestisce UI/input/animazioni; il core gestisce solo “regole e stato”. Così puoi cambiare UI o engine senza riscrivere la logica.
Determinismo: nel core puoi controllare RNG/deck con seed o lista di carte nei test; con Unity spesso finisci a usare API o timing che complicano la riproducibilità.
Detto questo, posso comunque fare tutto “dentro Unity” e usare UnityEngine ovunque: funziona, solo che il TDD diventa più difficile.

### 4.2 UnityPresentation (non testabile / minimal)
- Mostra stato e log
- Raccoglie input e invoca comandi del core

## 5. Modello dati (alto livello)
### 5.1 Tipi base
- `Card` con `CardKind`: `Number | Modifier | Action`
- `NumberValue`: 0–12
- `ModifierKind`: `Add(+2/+4/+6/+8/+10) | Multiply(x2)`
- `ActionKind`: `Freeze | FlipThree | SecondChance`

### 5.2 Stato match/round
- `MatchState`: players (3–8), `totalScores`, `targetScore=200`
- `RoundState`:
  - `dealerIndex`, `turnIndex`
  - `deck` (stack/queue), `discardPile`
  - `playerStates[]` (per-round)
  - `resolutionQueue` per gestire azioni “posticipate” (specie Flip Three)
- `PlayerRoundState`:
  - `status`: `Active | Stayed | Busted`
  - `numbers` (insieme dei valori Number posseduti)
  - `modifiers` (lista)
  - `hasSecondChance` (bool)

## 6. Comandi (API di alto livello del Core)
- `StartMatch(players)`
- `StartRound(matchState, dealerIndex, deckSeedOrDeckList)`
- `CanStay(player)` e `CanHit(player)`
- `Hit()` → pesca 1 carta e risolve effetti
- `Stay()` → imposta player `Stayed`
- `GetPublicState()` → snapshot leggibile per UI

## 7. Regole (invarianti da testare)
- I duplicati si controllano solo tra Number del giocatore, non su Action/Modifier
- Modifier non possono causare Bust e non contano per Flip 7
- `x2` moltiplica solo la somma delle Number (non +X, non +15)
- Second Chance:
  - previene un bust da duplicato Number una sola volta (consumo della carta)
  - max 1 per player: se ne riceve un’altra e ci sono altri Active senza, può passarla; altrimenti viene scartata
  - viene scartata a fine round anche se non usata
- Flip Three:
  - forza 3 pescate (numero/action/modifier contano come “una delle 3”)
  - interrompe anticipatamente se il player fa Flip 7 o bust
  - eventuali Action uscite durante le 3 pescate si risolvono dopo aver completato le 3 pescate (se non bust)
- Freeze:
  - forza l’uscita dal round (equivalente a `Stay` forzato), con punti calcolati a fine round

## 8. Strategia di test
### 8.1 Determinismo
- Nei test si usa un `Deck` deterministico:
  - `DeckFromList([...])` per scenari
  - opzionale: `DeckFromSeed(seed)` per test non-scenario

### 8.2 Granularità
- Test unitari su singole regole e transizioni di stato
- Test di “flow” su round completo (mini-simulazioni)

## 9. Test Cases (ordine consigliato)
### 9.1 Deck & Cards
- `Deck_BuildsCorrectDistribution_94Cards`
- `Deck_DrawMovesToPlayer_NotToDiscardUnlessSpecified`
- `Reshuffle_WhenDeckEmpty_UsesDiscardPile`

### 9.2 Scoring
- `Score_NoCards_IsZero`
- `Score_NumberSumOnly`
- `Score_X2_MultipliesOnlyNumberSum`
- `Score_AddModifiers_AppliedAfterX2`
- `Score_Flip7_Adds15_AfterAllModifiers`

### 9.3 Turn rules (Hit/Stay)
- `CannotStay_WithNoCards`
- `Stay_MarksPlayerStayed`
- `RoundEnds_WhenNoActivePlayers`

### 9.4 Bust & Second Chance
- `Bust_OnDuplicateNumber_SetsBusted_AndRoundScoreZero`
- `SecondChance_PreventsOneBust_ConsumesCardAndDiscardsDuplicate`
- `SecondChance_DoesNotPreventFreezeOrOtherNonBustEvents`
- `SecondChance_MaxOnePerPlayer_PassOrDiscardBehavior`
- `SecondChance_DiscardedAtRoundEnd`

### 9.5 Flip 7 bonus
- `Flip7_WhenSevenUniqueNumbers_EndRoundImmediately`
- `Flip7_Bonus15_GrantedToAchieverOnly`
- `Flip7_IgnoresModifiersAndActionsForCounting`

### 9.6 Action cards
- `Freeze_ForcesPlayerOutOfRound`
- `Freeze_WhenOnlyActivePlayer_MustTargetSelf` (se modellate il target selection nel core)
- `FlipThree_DrawExactlyThreeCards_UnlessBustOrFlip7`
- `FlipThree_DefersActionsUntilAfterThreeDraws`
- `FlipThree_IfActionRevealedDuringFlipThree_CanTargetAnyActive` (se il target selection è nel core)

### 9.7 Round lifecycle
- `RoundEnd_ScoringAppliedOnlyToStayedOrFrozenPlayers_NotToBusted`
- `DealerPassesLeft_EachRound`
- `GameEnd_WhenAnyPlayerReaches200_AtRoundEnd`
- `GameEnd_TieOver200_ContinuesUntilSingleWinner` (se implementata come estensione)

## 10. Acceptance Criteria (Definition of Done MVP)
- Avvio partita hotseat con 3–8 giocatori e scoreboard totale persistente
- Round giocabile end-to-end con UI minima (Hit/Stay) e log eventi
- Tutte le regole elencate nello scope sono implementate nel GameCore
- Test del GameCore:
  - coprono i casi di Bust, Second Chance, Flip Three, Freeze, Flip 7, scoring order, fine round, passaggio dealer, reshuffle
  - sono deterministici (nessun test flaky)

## 11. Manual QA (check rapido)
- Simulare un bust e verificare 0 punti
- Simulare Second Chance su duplicato e verificare che non busti
- Simulare Flip Three con bust al 2° draw
- Simulare Flip 7 durante Flip Three (al 1° o 2° draw) e verificare fine round immediata
- Verificare scoring con Number + x2 + +X + Flip7 (+15)
