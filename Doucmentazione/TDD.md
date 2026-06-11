# Flip 7 — Technical Design Document (TDD)

**Autore:** Gabriele Laera
**Corso:** Game Developer, Modulo Code Design
**Piattaforma:** Unity 6 (C#)
**Versione:** 1.0

---

## 1. Architettura

Il progetto segue una **architettura a 3 livelli** con separazione netta tra UI e logica:

Assets/Scripts/
├── Carta.cs                   (enums + classe carta, C# puro)
├── Mazzo.cs                   (mazzo 88 carte, Fisher-Yates)
├── StatoRoundGiocatore.cs     (stato individuale di un giocatore nel round)
├── CalcolatorePunteggio.cs    (calcolo punteggi, classe statica)
├── StatoPartita.cs            (punteggi totali, fine partita)
├── StatoRound.cs              (orchestrazione round, turni, azioni)
├── MotoreDiGioco.cs           (FACADE — coordina tutte le classi core)
└── Avvio.cs                   (MonoBehaviour — crea UI, input utente)

### 1.1 Separazione UI / Logica

- **Core** (C# puro, nessun `using UnityEngine`): `Carta`, `Mazzo`, `StatoRoundGiocatore`, `CalcolatorePunteggio`, `StatoPartita`, `StatoRound`, `MotoreDiGioco`
- **Presentazione** (MonoBehaviour Unity): solo `Avvio.cs` dipende da Unity. Gestisce Canvas, input, rendering UI
- **Facade**: `MotoreDiGioco` è l'unico punto di contatto tra UI e logica

## 2. Modello Dati

### 2.1 Carta.cs

- `Carta` con `TipoCarta` enum: `Numero`, `Modificatore`, `Azione`
- `ValoreNumero` enum: `Zero` (0) a `Dodici` (12)
- `TipoModificatore` enum: `Aggiungi` (+X), `Moltiplica` (x2)
- `TipoAzione` enum: `Congela`, `PescaTreCarte`, `SecondaChance`
- Costruttore con parametri opzionali nominati
- `ToString()` per debug

### 2.2 StatoPartita.cs

- `NumeroGiocatori`
- `PunteggiTotali[]`
- `PunteggioObiettivo = 200`
- `RoundCorrente`
- `IndiceMazziere`
- `Stato` enum: `InCorso`, `Finita`
- `VincitoreIndice` (-1 se nessuno)

### 2.3 StatoRound.cs

- `GiocatoreCorrente`
- `NumeroGiocatori`
- `Flip7Attivato` (bool)
- Gestione `PescaTreCarte` con enum `StatoPescaTreCarte`: `Nessuno`, `InCorso`
- Coda azioni differite per risoluzione post-PescaTreCarte

### 2.4 StatoRoundGiocatore.cs

- `Stato` enum: `Attivo`, `Congelato`, `Sballato`, `Fermo`
- `Numeri` (lista delle carte Numero possedute)
- `ModificatoreSomma` (accumulo +X)
- `HaMoltiplicatore` (bool, x2)
- `ContoAzioni` (per bonus Flip 7)
- `HaSecondaChance` (bool)

### 2.5 Mazzo.cs

- 88 carte: 79 Numero + 6 Modificatore + 3 Azione
- Fisher-Yates shuffle
- Rimescolo automatico dagli scarti quando il mazzo finisce

## 3. API del Core (MotoreDiGioco.cs)

| Metodo                              | Descrizione |
| `AvviaPartita(int numeroGiocatori)` | Crea partita e avvia primo round |
| `AvviaRound()` | Nuovo mazzo + nuovo round |
| `PuòPescare()` | Controlla se il giocatore corrente può pescare |
| `PuòFermarsi()` | Controlla se il giocatore corrente può fermarsi |
| `Pesca()` | Pesca 1 carta e risolve effetti |
| `Ferma()` | Giocatore corrente si ferma volontariamente |
| `ConcludiRound()` | Calcola punteggi, aggiorna totali, controlla vincitore |
| `OttieniStatoPubblico()` → `StatoPubblico` (DTO) | Istantanea per l'UI |

## 4. Regole Implementate

- **Duplicati:** si controllano solo tra carte Numero del giocatore
- **Modificatori:** non causano sballo, non contano per Flip 7
- **x2:** moltiplica solo la somma dei Numeri (non +X, non +15)
- **Seconda Chance:** previene uno sballo da duplicato Numero; max 1 per giocatore; scartata a fine round
- **Pesca Tre Carte:** forza 3 pescate consecutive; eventuali Azioni durante si risolvono dopo le 3 (solo se il giocatore non è sballato)
- **Congela:** forza l'uscita dal round con punti calcolati a fine round
- **Flip 7:** 7 carte Numero uniche → round termina subito, bonus +15
- **Fine round:** quando nessun giocatore è più nello stato Attivo, oppure Flip 7 attivato
- **Fine partita:** un giocatore raggiunge 200+ punti a fine round
- **Mazziere:** passa a sinistra a ogni nuovo round

## 5. Diagramma di Flusso

[Avvio] → Menu → PLAY → MotoreDiGioco.AvviaPartita()
                              ↓
                    StatoPartita (creato)
                              ↓
                    AvviaRound() → Mazzo (nuovo) + StatoRound (creato)
                              ↓
                    [Loop turni] → Pesca() / Ferma()
                              ↓
                    ConcludiRound() → Calcola punteggi
                              ↓
                    AggiornaPunteggiTotali() → ControllaVincitore()
                              ↓
              ┌─── 200+? → GameOver ←──┐
              │                        │
              └── No → AvviaRound()  ──┘

## 6. Dipendenze tra le Classi

- **Avvio** → `MotoreDiGioco`, `StatoPubblico` (DTO)
- **MotoreDiGioco** → `StatoPartita`, `StatoRound`, `Mazzo`, `CalcolatorePunteggio`
- **StatoPartita** → nessuna dipendenza
- **StatoRound** → `Mazzo`, `StatoRoundGiocatore`, `Carta`, `CalcolatorePunteggio`
- **StatoRoundGiocatore** → `Carta`
- **Mazzo** → `Carta`
- **Carta** → nessuna dipendenza
- **CalcolatorePunteggio** → `StatoRoundGiocatore`

## 7. Note sull'Input System

Il progetto Unity ha "Active Input Handling" impostato su **Input System Package (New)**. Di conseguenza:

- L'EventSystem usa `InputSystemUIInputModule` anziché il vecchio `StandaloneInputModule`
- L'input legacy (`UnityEngine.Input`) non è disponibile

## 8. Criteri di Accettazione

- Il progetto si apre in Unity 6 e funziona premendo Play su qualsiasi scena
- Menu con selezione giocatori (3–8)
- Round giocabile con bottoni PESCA e FERMA
- Sballo su duplicato Numero
- Seconda Chance previene lo sballo
- Flip 7 termina il round e assegna bonus +15
- Congela e Pesca Tre Carte funzionano correttamente
- Transizioni Menu → Gioco → GameOver → Menu
- Partita termina quando un giocatore raggiunge 200+ punti
