using System;

namespace TheMonster.Common
{
    // --- Shared types (no game logic here) ---

    // What kind of action a turn can perform.
    public enum ActionType { ShootSelf, ShootOpponent, Respin }

    // What happened when the trigger was pulled.
    public enum ShotOutcome { Empty, Fired }

    // How a round ended overall.
    public enum RoundResult { None, PlayerDied, EnemyDied, PlayerFolded, EnemyFolded }

    // ☝🏻 What the Roulette system must expose (Eithan´s part)
    public interface IRouletteAPI
    {
        void InitializeRound(bool playerStarts);
        bool CanRespin();          // Roulette-side availability (separate from cost logic)
        void RequestRespin();      // Perform the spin animation/logic.
        void PerformAction(ActionType action);

        // Events the roulette raises to notify others.
        event Action<bool> OnTurnChanged;                 // true = player's turn
        event Action<bool, ShotOutcome> OnShotResolved;   // targetIsPlayer, outcome
        event Action<RoundResult> OnRoundEnded;
        event Action OnRespinDone;
    }

    // What your Wagering system exposes 
    public interface IWagerAPI
    {
        void StartNewRound(bool playerStarts, int enemyFuelPercent);

        // Raising requires agreement from both sides.
        bool TryProposeRaise(int capsules);  // creates a pending proposal
        bool TryAcceptRaise();               // applies the raise if both can afford
        void DeclineRaise();                 // cancels the pending proposal

        // Respin: cost is handled by Wager; availability by Roulette.
        bool CanAffordRespin(out int costCapsules);
        bool ChargeRespinAndAddToPot();      // subtracts capsules from player and adds to pot

        // Exits/settlement
        void PlayerFold();
        void ApplyRoundWinToPlayer();
        void ApplyRoundWinToEnemy();
        void ApplyBetweenRoundDecayToPlayer();

        // Read-only status
        int PlayerFuelPercent { get; }
        int EnemyFuelPercent  { get; }
        int PotCapsules       { get; }
    }
}
