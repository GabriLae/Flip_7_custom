# Flip Seven (Flip 7) — Bozza GDD (Digital)

Repo portfolio: riproduzione digitale fedele del gioco di carte “Flip 7”, con UX chiara e scorrevole, pronta da mostrare pubblicamente.

## 1. High Concept
- **Genere**: card game “push your luck”
- **Durata**: ~20 minuti
- **Giocatori**: 3+ (supporto opzionale a 2 con regola “sfida a round”)
- **Obiettivo**: essere il primo a raggiungere **200 punti** (si conteggiano a fine round)

## 2. Pillars (Design Goals)
- **Fedeltà**: regole e flusso identici al gioco fisico
- **Leggibilità**: sempre chiaro rischio attuale (duplicati) e punteggio potenziale
- **Ritmo**: turni rapidi, animazioni brevi, interazioni minime (“Hit” / “Stay”)
- **Multiplayer first**: hotseat come MVP, online come estensione

## 3. Target & Platform
- **Target**: casual, 8+, gruppi/party game
- **Piattaforma MVP consigliata**: desktop (forse web se capisco qualcosa)

## 4. Componenti di Gioco
### 4.1 Tipi di Carte
- **Number cards**: valori 0–12
  - Distribuzione: 1 carta “1”, 2 carte “2”, …, 12 carte “12”, più **1 carta “0”** (0 vale 0 punti ma aiuta a completare le 7 uniche)
- **Score Modifier cards**: non sono Number card e non contano per il bonus “Flip 7”
  - `+2`, `+4`, `+6`, `+8`, `+10`
  - `x2` (moltiplica solo la somma delle Number cards)
- **Action cards**:
  - `Freeze!`
  - `Flip Three!`
  - `Second Chance!`

## 5. Flusso di una Partita
### 5.1 Setup
- Mescolare il mazzo (94 carte totali)
- Scegliere un **Dealer** per il round
- Tenere traccia dei punteggi dei giocatori (scoreboard)

### 5.2 Inizio Round (deal iniziale)
- In ordine di turno, il Dealer distribuisce **1 carta scoperta** a ciascun giocatore (incluso se stesso)
- Se durante il deal iniziale esce una **Action card**, si mette in pausa il deal per risolverla subito; poi si continua finché tutti hanno ricevuto una carta
- Nota: dopo la risoluzione di Action card iniziali, alcuni giocatori potrebbero avere 0 carte o più carte (dipende dagli effetti)

### 5.3 Turno del Giocatore (Hit / Stay)
- Il Dealer offre a ciascun giocatore attivo (in ordine) la scelta:
  - **Hit**: pesca 1 carta scoperta
  - **Stay**: esce dal round e “banca” i punti che avrà accumulato a fine round
- Un giocatore può scegliere **Stay** finché ha almeno una carta davanti a sé (anche solo un Modifier)

## 6. Regole Chiave
### 6.1 Bust (sballare)
- Si può andare Bust solo sulle **Number cards**
- Se un giocatore ottiene una **Number card duplicata** rispetto a una Number card già nella propria linea, va **Bust** ed è **fuori dal round** (punteggio round = 0)

### 6.2 Bonus “Flip 7”
- Se un giocatore ha **7 Number cards uniche** scoperte davanti a sé:
  - Il round termina **immediatamente** per tutti
  - Il giocatore ottiene **+15 punti bonus**
- Action e Modifier **non contano** per arrivare a 7

### 6.3 Definizione di “Active Player”
- Active player = non è Bust e non ha scelto Stay

## 7. Carte Azione
Regola generale:
- Le Action cards possono essere giocate su **qualsiasi giocatore attivo**, anche su se stessi
- Se chi riceve l’Action card è l’**unico giocatore attivo**, deve giocarla **su se stesso**
- Le Action cards si tengono in un’area sopra la linea delle Number cards

### 7.1 Freeze!
- Il giocatore bersaglio **banca** tutti i punti raccolti e diventa **inattivo** (fuori dal round)

### 7.2 Flip Three!
- Il giocatore bersaglio deve accettare le **prossime 3 carte**, rivelandole una alla volta
- Durante queste 3 carte:
  - si applicano le regole normali (si può andare Bust su duplicati Number)
  - se il giocatore completa “Flip 7”, il round termina subito
  - Number, Action e Modifier contano nel conteggio delle “3 carte”
- Se tra queste carte appare una Action card (`Freeze` o `Flip Three`), viene risolta **dopo** che tutte e 3 le carte sono state pescate (solo se il giocatore non è già Bust)
- Se appare `Second Chance!`, può essere messa da parte e usata (vedi sotto)

### 7.3 Second Chance!
- Il giocatore mantiene la carta
- Se più avanti riceve una Number card duplicata:
  - scarta `Second Chance!` e la Number card duplicata (evita il Bust)
- Limitazioni:
  - massimo **una** `Second Chance!` davanti a sé alla volta
  - se un giocatore pesca una seconda `Second Chance!`, deve darla a un altro giocatore attivo; se non possibile, la carta viene scartata
  - tutte le `Second Chance!` vengono scartate **a fine round** anche se non usate

## 8. Carte Modificatore (Score Modifiers)
- Non causano Bust
- Non contano per “Flip 7”
- È possibile terminare il turno anche con soli Modifier e segnare punti, **tranne** nel caso di avere solo `x2`

## 9. Fine Round, Punteggio e Fine Partita
### 9.1 Condizioni di Fine Round
Il round termina quando:
1. Non ci sono più giocatori attivi (tutti Bust o Stay), oppure
2. Un giocatore completa “Flip 7”

### 9.2 Calcolo Punteggio Round
Ordine delle operazioni:
1. Sommare i valori delle **Number cards**
2. Se presente `x2`, moltiplicare la somma delle Number cards per 2
3. Sommare i modificatori `+X`
4. Se il giocatore ha fatto “Flip 7”, aggiungere **+15**

### 9.3 Start Round Successivo
- Mettere da parte tutte le carte del round (non rimescolare subito)
- Passare il mazzo rimanente a sinistra: il nuovo giocatore diventa Dealer
- Quando il mazzo termina, rimescolare gli scarti per formare un nuovo mazzo
- Se serve rimescolare a metà round, le carte davanti ai giocatori restano dove sono (anche per chi è Bust)

### 9.4 Fine Partita
- A fine round, se almeno un giocatore raggiunge **200+**, vince chi ha **più punti**
- Estensione consigliata: in caso di pareggio sopra 200, continuare a giocare altri round finché c’è un solo vincitore

## 10. UX / UI (MVP)
- Tavolo centrale con mazzo, scarti (opzionale), indicatore Dealer
- Per ogni giocatore:
  - “Linea” Number cards in fila
  - Modifier sopra la fila
  - Stato: Active / Stayed / Busted / Frozen
  - Punteggio potenziale live (con breakdown: sum, x2, +X, +15)
- CTA nel turno: `Hit` / `Stay`
- Event log sintetico (ultime 3–5 azioni)

## 11. Game States (Implementazione)
- `Match`: punteggi totali, ordine giocatori, condizione di fine partita
- `Round`: dealerIndex, deck, discardPile, activePlayers, turnIndex
- `PlayerRoundState`: numberCardsSet, modifierCards, hasSecondChance, status
- `ActionResolutionQueue`: per gestire interruzioni (deal iniziale, Flip Three chain, azioni emerse durante Flip Three)

## 12. Modalità (Roadmap)
- **MVP**: hotseat (pass-and-play), 3–8 giocatori
- **V1**: online lobby + matchmaking (turn-based o real-time “pass the turn”)
- **V2**: bot/AI, tutorial interattivo, statistiche partita

## 13. Fonti
- Regole estratte dal PDF caricato nel repo (`25_FLIP_7_TB_RULES_C_Rev_9_2_25_ND.pdf`)
- Approfondimenti pubblici:
  - https://www.asmodee.co.uk/blogs/news/how-to-play-flip-7
  - https://theop.games/pages/flip-7-faqs

