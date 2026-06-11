# Flip Seven (Flip 7) — GDD (Digitale)

Repository portfolio: riproduzione digitale fedele del gioco di carte "Flip 7", con interfaccia chiara e scorrevole, pronta da mostrare pubblicamente.

## 1. Concept Generale

- **Genere**: gioco di carte "tenta la fortuna" (push your luck)
- **Durata**: ~20 minuti
- **Giocatori**: 3+ (supporto opzionale a 2 giocatori con regola "sfida a round"), da 3 a 8 giocatori
- **Obiettivo**: essere il primo a raggiungere **200 punti** (si conteggiano a fine round)

## 2. Pilastri (Obiettivi di Design)

- **Fedeltà**: regole e flusso identici al gioco fisico
- **Leggibilità**: sempre chiaro il rischio attuale (duplicati) e il punteggio potenziale
- **Ritmo**: turni rapidi, animazioni brevi, interazioni minime ("Pesca" / "Ferma")
- **Multiplayer prima di tutto**: hotseat come MVP, online come estensione futura

## 3. Target e Piattaforma

- **Target**: casual, 8+, gruppi/party game
- **Piattaforma MVP consigliata**: desktop (possibile estensione web in futuro)

## 4. Componenti di Gioco

### 4.1 Tipi di Carte

- **Carte Numero (Number cards)**: valori 0–12
  - Distribuzione: 1 carta "0", 1 carta "1", 2 carte "2", 3 carte "3", …, 12 carte "12" (totale 79 carte Numero)
  - Lo 0 vale 0 punti ma aiuta a completare le 7 carte uniche per il bonus "Flip 7"
- **Carte Modificatore (Score Modifier cards)**: non sono carte Numero e non contano per il bonus "Flip 7"
  - `+2`, `+4`, `+6`, `+8`, `+10`
  - `x2` (moltiplica solo la somma delle carte Numero)
- **Carte Azione (Action cards)**:
  - `Freeze!` (Congela)
  - `Flip Three!` (Pesca Tre)
  - `Second Chance!` (Seconda Chance)

Il mazzo completo contiene **94 carte** (79 Numero + 6 Modificatore + 3 Azione).

## 5. Flusso di una Partita

### 5.1 Preparazione

- Mescolare il mazzo (94 carte totali)
- Scegliere un **Mazziere** per il round
- Tenere traccia dei punteggi dei giocatori (tabellone)

### 5.2 Inizio Round (distribuzione iniziale)

- In ordine di turno, il Mazziere distribuisce **1 carta scoperta** a ciascun giocatore (incluso se stesso)
- Se durante la distribuzione iniziale esce una **carta Azione**, si mette in pausa per risolverla subito; poi si continua finché tutti hanno ricevuto una carta
- Nota: dopo la risoluzione di carte Azione iniziali, alcuni giocatori potrebbero avere 0 carte o più carte (dipende dagli effetti)

### 5.3 Turno del Giocatore (Pesca / Ferma)

- Il Mazziere offre a ciascun giocatore attivo (in ordine) la scelta:
  - **Pesca (Hit)**: pesca 1 carta scoperta
  - **Ferma (Stay)**: esce dal round e "blocca" i punti accumulati a fine round
- Un giocatore può scegliere **Ferma** solo se ha almeno una carta davanti a sé (anche solo un Modificatore)

## 6. Regole Chiave

### 6.1 Sballo (Bust)

- Si può sballare solo sulle **carte Numero**
- Se un giocatore ottiene una **carta Numero duplicata** rispetto a una carta Numero già nella propria linea, **sballa** ed è **fuori dal round** (punteggio round = 0)

### 6.2 Bonus "Flip 7"

- Se un giocatore ha **7 carte Numero uniche** scoperte davanti a sé:
  - Il round termina **immediatamente** per tutti
  - Il giocatore ottiene **+15 punti bonus**
- Le carte Azione e Modificatore **non contano** per arrivare a 7

### 6.3 Definizione di "Giocatore Attivo"

- Giocatore attivo = non è sballato e non ha scelto Ferma

## 7. Carte Azione

Regola generale:

- Le carte Azione possono essere giocate su **qualsiasi giocatore attivo**, anche su se stessi
- Se chi riceve la carta Azione è l'**unico giocatore attivo**, deve giocarla **su se stesso**
- Le carte Azione si tengono in un'area sopra la linea delle carte Numero

### 7.1 Congela (Freeze)

- Il giocatore bersaglio **blocca** tutti i punti raccolti e diventa **inattivo** (fuori dal round)

### 7.2 Pesca Tre (Flip Three)

- Il giocatore bersaglio deve accettare le **prossime 3 carte**, rivelandole una alla volta
- Durante queste 3 carte:
  - si applicano le regole normali (si può sballare su duplicati Numero)
  - se il giocatore completa "Flip 7", il round termina subito
  - le carte Numero, Azione e Modificatore contano nel conteggio delle "3 carte"
- Se tra queste carte appare una carta Azione (`Freeze` o `Flip Three`), viene risolta **dopo** che tutte e 3 le carte sono state pescate (solo se il giocatore non è già sballato)
- Se appare `Second Chance!`, può essere messa da parte e usata (vedi sotto)

### 7.3 Seconda Chance (Second Chance)

- Il giocatore mantiene la carta
- Se più avanti riceve una carta Numero duplicata:
  - scarta `Second Chance!` e la carta Numero duplicata (evita lo sballo)
- Limitazioni:
  - massimo **una** `Second Chance!` davanti a sé alla volta
  - se un giocatore pesca una seconda `Second Chance!`, deve darla a un altro giocatore attivo; se non possibile, la carta viene scartata
  - tutte le `Second Chance!` vengono scartate **a fine round** anche se non usate

## 8. Carte Modificatore (Score Modifiers)

- Non causano sballo
- Non contano per "Flip 7"
- È possibile terminare il turno anche con soli Modificatori e segnare punti, **tranne** nel caso di avere solo `x2`

## 9. Fine Round, Punteggio e Fine Partita

### 9.1 Condizioni di Fine Round

Il round termina quando:

1. Non ci sono più giocatori attivi (tutti sballati o fermi), oppure
2. Un giocatore completa "Flip 7"

### 9.2 Calcolo Punteggio Round

Ordine delle operazioni:

1. Sommare i valori delle **carte Numero**
2. Se presente `x2`, moltiplicare la somma delle carte Numero per 2
3. Sommare i modificatori `+X`
4. Se il giocatore ha fatto "Flip 7", aggiungere **+15**

### 9.3 Inizio Round Successivo

- Mettere da parte tutte le carte del round (non rimescolare subito)
- Passare il mazzo rimanente a sinistra: il nuovo giocatore diventa Mazziere
- Quando il mazzo termina, rimescolare gli scarti per formare un nuovo mazzo
- Se serve rimescolare a metà round, le carte davanti ai giocatori restano dove sono (anche per chi è sballato)

### 9.4 Fine Partita

- A fine round, se almeno un giocatore raggiunge **200+**, vince chi ha **più punti**
- Estensione consigliata: in caso di pareggio sopra 200, continuare a giocare altri round finché c'è un solo vincitore

## 10. Interfaccia Utente (MVP)

- Tavolo centrale con mazzo, scarti (opzionale), indicatore Mazziere
- Per ogni giocatore:
  - "Linea" carte Numero in fila
  - Modificatori sopra la fila
  - Stato: **Attivo / Fermo / Sballato / Congelato**
  - Punteggio potenziale in tempo reale (con dettaglio: somma, x2, +X, +15)
- Pulsanti nel turno: `Pesca` / `Ferma`
- Registro eventi sintetico (ultime 3–5 azioni)

## 11. Stati di Gioco (Implementazione)

- `StatoPartita`: punteggi totali, ordine giocatori, condizione di fine partita
- `StatoRound`: indiceMazziere, mazzo, pilaScarti, giocatoriAttivi, indiceTurno
- `StatoRoundGiocatore`: `Numeri` (lista readonly), `ModificatoreSomma`, `HaMoltiplicatore`, `ContoAzioni`, `HaSecondaChance`, `Stato` (Attivo/Congelato/Sballato/Fermo)
- `CodaRisoluzioneAzioni`: per gestire interruzioni (distribuzione iniziale, catena Flip Three, azioni emerse durante Flip Three)

## 12. Modalità (Roadmap)

- **MVP**: hotseat (pass-and-play), 3–8 giocatori
- **V1**: lobby online + matchmaking (turn-based o real-time "passa il turno")
- **V2**: bot/IA, tutorial interattivo, statistiche partita

## 13. Fonti

- Approfondimenti pubblici:
  - <https://www.asmodee.co.uk/blogs/news/how-to-play-flip-7>
  - <https://theop.games/pages/flip-7-faqs>
