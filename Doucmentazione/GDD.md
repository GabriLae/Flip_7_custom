# Flip 7 — Game Design Document (GDD)

**Autore:** Gabriele Laera
**Corso:** Game Developer, Modulo Code Design
**Piattaforma:** Unity 6 (C#)
**Versione:** 1.0

---

## 1. Concept Generale

- **Genere:** gioco di carte "tenta la fortuna" (push your luck)
- **Durata:** ~20 minuti
- **Giocatori:** da 3 a 8
- **Obiettivo:** essere il primo a raggiungere **200 punti** (si conteggia a fine round)

## 2. Regole del Gioco

### 2.1 Preparazione

- Il mazzo contiene **88 carte** (79 Numero + 6 Modificatore + 3 Azione)
- Si sceglie un **Mazziere** per il primo round
- Ogni round si gioca con un mazzo nuovo mescolato

### 2.2 Turno del Giocatore

Ogni giocatore attivo, in ordine di turno, sceglie:

- **Pesca:** pesca 1 carta scoperta e la aggiunge alla propria linea
- **Ferma:** esce dal round e conserva i punti accumulati (possibile solo con almeno una carta davanti)

### 2.3 Tipi di Carte

**Carte Numero (valori 0–12):**

- Distribuzione: 1 carta "0", 1 carta "1", 2 carte "2", 3 carte "3", …, 12 carte "12" (totale 79)
- Lo 0 vale 0 punti ma aiuta a completare 7 carte uniche per il bonus "Flip 7"

**Carte Modificatore:**

- `+2`, `+4`, `+6`, `+8`, `+10` (aggiungono punti)
- `x2` (raddoppia solo la somma delle carte Numero)
- Non causano sballo e non contano per il bonus Flip 7

**Carte Azione:**

- **Congela:** il giocatore bersaglio blocca i punti raccolti e diventa inattivo
- **Pesca Tre Carte:** il giocatore bersaglio deve pescare esattamente 3 carte (le Azioni emerse vengono risolte dopo le 3 pescate, solo se non sballato)
- **Seconda Chance:** previene uno sballo da duplicato Numero (una volta); massimo 1 per giocatore; scartata a fine round

### 2.4 Sballo

- Un giocatore **sballa** se pesca una carta Numero duplicata rispetto a quelle già possedute
- Sballato = fuori dal round, punteggio round = 0
- Si può sballare solo sulle carte Numero, non su Modificatore o Azione

### 2.5 Bonus "Flip 7"

- Se un giocatore ha **7 carte Numero uniche**, il round termina immediatamente
- Il giocatore ottiene **+15 punti bonus**
- Le carte Modificatore e Azione non contano per il conteggio

### 2.6 Fine Round e Punteggio

Il round termina quando:

1. Nessun giocatore è più nello stato "Attivo" (tutti sballati, fermi o congelati), oppure
2. Un giocatore completa "Flip 7"

**Calcolo punteggio (in ordine):**

1. Somma dei valori delle carte Numero
2. Se presente `x2`, moltiplica la somma per 2
3. Somma i modificatori `+X`
4. Se ha fatto "Flip 7", aggiungi +15

Se **Sballato** → punteggio 0.

### 2.7 Fine Partita

- A fine round, se un giocatore raggiunge **200+ punti**, vince chi ha il punteggio più alto
- In caso di pareggio, vince chi ha il punteggio massimo (controllato da `StatoPartita.TrovaIndicePunteggioMassimo()`)

## 3. Interfaccia Utente (MVP)

Il gioco si avvia premendo **Play** in Unity su qualsiasi scena (anche vuota). Tutta l'interfaccia viene creata a runtime:

- **Schermata Menu:** titolo "Flip 7" e bottoni per selezionare il numero di giocatori (da 3 a 8)
- **Schermata Gioco:** per ogni giocatore mostra stato, carte, modificatori e punteggio; bottoni `PESCA` e `FERMA`
- **Schermata GameOver:** mostra il vincitore e la classifica finale; bottone per tornare al menu

## 4. Stati di Gioco (Implementazione)

- `StatoPartita`: punteggi totali, round corrente, mazziere, condizione di fine partita (200+ punti)
- `StatoRound`: turni, giocatore corrente, pescate, gestione Pesca Tre Carte e azioni differite
- `StatoRoundGiocatore`: stato individuale (Attivo/Congelato/Sballato/Fermo), carte possedute, modificatori, Seconda Chance

## 5. Note Tecniche

- Il gioco usa il **nuovo Input System** (InputSystemUIInputModule) per la gestione dei click
- L'EventSystem viene creato automaticamente all'avvio
- Il mazzo usa l'algoritmo **Fisher-Yates** per mescolare e rimescola automaticamente dagli scarti quando finisce
