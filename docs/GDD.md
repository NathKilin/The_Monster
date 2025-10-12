# The Monster — Game Design Document (GDD)

## 1. Overview
Title: The Monster (so far)
Genre: Survival / Gambling Strategy / Psychological Thriller
Platform: Mobile
Perspective: First-person
Core Loop: Gamble → Risk → Win or Lose Resources → Continue Surviving

**Elevator Pitch:**
In a ravaged post-apocalyptic world where most survivors are half-machine, the only remaining economy is gambling — wagering your food, fuel, or even body parts. Survival isn’t about winning; it’s about lasting longer than despair allows.

## 2. Narrative

**Setting:**
A scorched, neon-stained wasteland — centuries after the collapse of civilization. The air is toxic, energy is scarce, and flesh is obsolete. Those who remain are Cyborg Drifters, scavenging both sustenance and parts. Humanity has been replaced by calculation; emotion reduced to probability.

**Theme:**
- Risk measurement.
- Survival and its cost.
- The illusion of control in a decayed world.

**Player Motivation:**
To survive as long as possible, balancing risk vs reward, and deciding how much of yourself you’re willing to lose to keep breathing another day.

## 3. Gameplay Mechanics

### Outcome & Resource System
**Wagers:**
- Food: Keep your main biological systems alive — runs out → death by starvation.
- Fuel: Powers your cybernetic body — runs out → system shutdown.
- Body Parts: Can be gambled; losing them alters gameplay (each body part lost means an extra 1/10 of your fuel decay).

**Post-Match Decay:**
- After each game: Lose 1/10 of your Food and Fuel reserves (base).
- Lose an extra 1/10 of fuel if body parts were wagered.

**Death:**
Occurs when:
- Food or Fuel = 0.
- You lose a round’s shot.
- Your core systems collapse (multiple body parts lost, maybe total of 5).

## 4. Progression & Replayability
There is no “win.” You play until survival ends. Each session records:
- Matches Played
- Wins / Folds
- Longest Survival Time
- Final Body Integrity %
- Fate Description (“You died starving with one arm and half a heart battery.”)

Unlocks or cosmetic rewards could appear as scars or cyber-upgrades from past lives.

## 5. Visual & Audio Direction
**Visual Style:**
Minimalist, high-contrast neon and chrome. Dusty wasteland background with dim flickering lights. Emphasis on the table — revolver, hands, and shadows.

**Audio Style:**
Low mechanical hums, metallic clinks. Tension pulses before each shot. Voice distortion representing decaying humanity.

## 6. AI Behavior
The opponent evaluates risk probabilistically:
- Early rounds: cautious (higher fold chance).
- Later rounds: desperate (more aggressive).
- Some AIs are reckless, others coldly calculating.
- AI personalities could be named (e.g., “The Mechanist,” “The Prophet,” “The Beast”).

## 7. Mood & Inspirations
**Inspirations:**
- Buckshot Roulette
- Papers, Please (for grim bureaucracy tone)
- Rain World (for atmosphere)
- Blame (manga)

**Tone Keywords:** Decay. Metallic. Coldheated.

## 8. Future Ideas
- Story Mode: Encounter unique characters, each with philosophies about risk and survival.
- Augment System: Replace lost body parts with weaker or stronger cybernetic upgrades.
- Resource Map: Travel to new settlements — gamble with locals, hunt, or scavenge.
- Multiplayer Mode: 1v1 risk duels online.

## 9. End Screen Concept
You lasted 17 matches.
Final resources: 12% Fuel, 8% Food.
Status: Missing Right Arm.
Cause of death: Misfire.
“In the end… you were happy to put an end to all of it.”

## 10. Development Notes
**Prototype Scope:** Single table encounter, one AI, simple UI.
**Tech Stack:** Unity.
**Core Loop First:** Risk system, resource loss, turn logic.

### Wagering & Status System
**Developer:** Nathan (Wagering Module Lead)
Integration Target: Roulette System (built by teammate)
Engine: Unity (C#)
Version: Final system rules confirmed (10% life cost, 40% start fuel, respin costs half pot, etc.)

**Main Objective**
Design and implement the Wagering & Status System, which governs:
- Player and enemy fuel management (life = energy).
- Capsule economy (10% per capsule).
- Pot control, raises, and folds.
- Respin cost logic (half the pot, unilateral payment).
- Between-round life cost (–10%).
- UI feedback and event communication with the Roulette System.

The system must be modular, event-driven, and ready to connect with the Roulette system through a clean interface.

### System Requirements
- Fuel System: Represent both life and currency as percentages (multiples of 10). Convert to capsules for gameplay.
- Pot System: Manage the total wager (shared between both sides).
- Raising System: Both players must agree for a raise to occur.
- Respin Cost: Half the current pot, rounded up, paid only by the player.
- Fold System: Allows retreat at a cost (lose pot + 1 capsule).
- Decay System: After each round, player loses 10% fuel automatically (if alive).
- Event Communication: Emit clean, simple signals to connect with the Roulette logic (no direct gun control).
- UI: Visualize player/enemy fuel, pot size, and available actions. Disable buttons when impossible.

### Step 1 — Project Structure Setup
Files to create/confirm:
- `WagerController.cs` – main logic brain.
- `PlayerState.cs` – tracks player’s fuel, conversions, and limits.
- `EnemyState.cs` – similar structure, simplified AI-driven.
- `RoundState.cs` – manages pot, contributions, raise proposals, and flags.
- `WagerUIController.cs` – handles sliders, texts, and buttons.

Scene Setup: `WagerSystemRoot` GameObject, attach controllers, `WagerCanvas`, UI elements (Fuel bars, Pot display, action buttons).

### Step 2 — Core Data Models
`PlayerState` example and `RoundState` example described.

### Step 3 — Implement Economic Logic
Details for Ante, Raise, Respin, Fold, Decay are provided.

### Step 4 — Event Flow with Roulette System
Defined method/event names for communication.

### Step 5 — UI Logic
Display and interactivity rules listed.

### Step 6 — Round Resolution
On Win, On Death, On Fold behavior defined.

### Step 7 — Testing & Debug Tools
God panel features and checkpoints listed.

### Step 8 — Integration Ready Conditions
Checklist of integration requirements.

