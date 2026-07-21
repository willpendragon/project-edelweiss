# System Specification: Deity Battle & Attunement Mechanism

## Core Flow & Lifecycle
1. **Trigger Condition:** Unlocked once Player unit satisfies Quest Requirements.
2. **Spawn Requirements:** Spawns `DeityUnit` and `DeityAltar` on the Battlefield grid.
3. **Altar Movement:** The `DeityAltar` updates its grid position at the start of every turn.
4. **End Conditions:**
   - **Player Captures Deity:** Win state -> End Battle.
   - **Player Kills Deity (Deicide):** Win state -> End Battle.
   - **Player Defeated:** Lose state -> End Battle.

## Attunement (Capture Attempt) Rules
- **Prerequisite:** Player unit must be on a tile adjacent to `DeityAltar`.
- **Action Cost:** Spending 1 Action/Move Point (OP).
- **UI Trigger:** Interacting with adjacent `DeityAltar` displays "Attempt Attunement" via `CursorController`.
- **Item Modifier (Tributes):**
  - Baked in Café beforehand.
  - Usable from anywhere on the battlefield (global target/no proximity required).
  - Each Tribute used applies a persistent **+10% modifier** to final capture success probability.

## Quick-Time Event (QTE) Logic
When Attunement is initiated, display QTE Overlay (slider with moving cursor).

### Base Success Rates by Timing
- **Miss (> 90% off-center):** Failure (0% base capture chance).
- **Normal Timing (40-50% tolerance):** 25% base capture chance.
- **Perfect Timing (10-20% tolerance):** 50% base capture chance.

### Final Chance Formula
$$ \text{Final Capture Chance} = \text{Base Rate} + \text{Health Modifier} + \text{Tribute Modifiers} $$

- **Health Modifier:** Scaled inversely with `DeityUnit` current HP percentage (Lower HP = higher modifier).
- **Tribute Modifier:** $+10\%$ per Tribute consumed during the battle.