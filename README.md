# Flip 7 — Gioco di Carte

**Autore:** Gabriele Laera  
**Corso:** ITS Game Developer — Arese (MI)  
**Modulo:** Code Design  
**Versione Unity:** 6 (C#)  

---

## Descrizione

Flip 7 è un gioco di carte basato su meccaniche di pesca e gestione del rischio.  
Da 3 a 8 giocatori pescano carte da un mazzo da 88 carte cercando di totalizzare il punteggio più alto senza sballare.

Il gioco funziona **in qualsiasi scena Unity** — anche una scena vuota. Basta premere Play.

---

## Struttura del Progetto

```text
Assets/Scripts/
├── Avvio.cs                  ← Punto di ingresso, UI, eventi
├── MotoreDiGioco.cs          ← Facciata del sistema di gioco
├── StatoPartita.cs           ← Stato persistente su più round
├── StatoRound.cs             ← Turno corrente e azioni
├── StatoRoundGiocatore.cs    ← Stato individuale del giocatore
├── Carta.cs                  ← Modello dati delle carte
├── Mazzo.cs                  ← Costruzione e mescolamento mazzo
└── CalcolatorePunteggio.cs   ← Calcolo punteggio a fine round

Assets/Doucmentazione/
├── DOCUMENTAZIONE.md         ← Documentazione formale del progetto
├── GDD.md                    ← Game Design Document
└── TDD.md                    ← Technical Design Document
```

---

## Come Giocare

1. **Aprire il progetto** in Unity 6
2. **Premere Play** su qualsiasi scena
3. **Selezionare il numero di giocatori** (3–8)
4. **Cliccare PLAY** per iniziare
5. A turno, cliccare **PESCA** per pescare una carta o **FERMA** per conservare il punteggio
6. Vince chi raggiunge **200 punti** per primo

Tutta l'interfaccia viene generata automaticamente a runtime.  
Non è necessario creare scene, prefab o GameObjects.

---

## Architettura

Il codice segue il **pattern Facade**: l'interfaccia Unity interagisce solo con `MotoreDiGioco`, che coordina tutte le classi del core.

Le classi del core sono **C# puro** (nessuna dipendenza da Unity), facilmente riutilizzabili o testabili.

```text
Avvio (UI) → MotoreDiGioco (Facade) → StatoPartita / StatoRound / Mazzo / CalcolatorePunteggio
```

---

## Documentazione

La cartella `Doucmentazione/` contiene tre documenti:

- **DOCUMENTAZIONE.md** — Documentazione formale per il corso
- **GDD.md** — Game Design Document (regole e meccaniche)
- **TDD.md** — Technical Design Document (architettura e implementazione)

---

## Licenza

Progetto didattico a uso interno del corso ITS Game Developer.  
Non sono inclusi eseguibili, asset grafici o materiale coperto da copyright.
