using UnityEngine;
using TheMonster.Common;     // enums + IRouletteAPI
using TheMonster.Wagering;   // IWagerAPI (WagerController)

namespace TheMonster.Systems
{
    /// <summary>
    /// Orchestrates the round lifecycle and glues:
    ///   UI (buttons)  → Roulette actions
    ///   Roulette events → Wager settlements + UI state
    /// Also creates each new round (enemy fuel, coin flip who starts).
    /// This class NEVER does pot/fuel math and NEVER decides gun outcomes.
    /// </summary>
    public class GameDirector : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private WagerController wager;          // on WagerSystemRoot
        [SerializeField] private WagerUIController ui;           // on Canvas (WagerCanvas)
        [SerializeField] private MonoBehaviour rouletteMono;     // MockRoulette or your friend's controller

        private IRouletteAPI roulette;   // typed interface to roulette system
        private System.Random rng;

        // ------------------------- Unity lifecycle -----------------------------

        private void Awake()
        {
            rng = new System.Random();

            // Validate references early to catch setup issues in the editor.
            if (!wager)        Debug.LogError("[GameDirector] WagerController not assigned.");
            if (!ui)           Debug.LogError("[GameDirector] WagerUIController not assigned.");
            if (!rouletteMono) Debug.LogError("[GameDirector] Roulette MonoBehaviour not assigned.");

            // Get the IRouletteAPI from the provided MonoBehaviour.
            roulette = rouletteMono as IRouletteAPI;
            if (roulette == null)
            {
                Debug.LogError("[GameDirector] rouletteMono does not implement IRouletteAPI.");
                return;
            }

            // Subscribe to roulette events so we can drive UI and settlements.
            roulette.OnTurnChanged  += HandleTurnChanged;
            roulette.OnShotResolved += HandleShotResolved;
            roulette.OnRoundEnded   += HandleRoundEnded;
            roulette.OnRespinDone   += HandleRespinDone;
        }

        private void Start()
        {
            // Kick off the very first round.
            StartNewRound();
        }

        // ------------------------- Round lifecycle -----------------------------

        /// <summary>
        /// Creates a new round:
        ///  - Enemy fuel randomized (10..100 in steps of 10)
        ///  - Coin flip who starts
        ///  - Wager collects ante (1 capsule each) and resets pot state
        ///  - Roulette initializes its cylinder/turn state
        ///  - UI reflects whose turn it is
        /// </summary>
        public void StartNewRound()
        {
            if (roulette == null || wager == null || ui == null) return;

            // Enemy fuel in {10,20,...,100}
            int enemyFuelPercent = rng.Next(1, 11) * 10;

            // 50/50 who starts
            bool playerStarts = rng.NextDouble() < 0.5;

            // Wager side: reset pot & take ante; set enemy fuel snapshot
            wager.StartNewRound(playerStarts, enemyFuelPercent);

            // Roulette side: randomize chambers/turn ownership
            roulette.InitializeRound(playerStarts);

            // UI: buttons on/off for the current actor
            ui.SetIsPlayerTurn(playerStarts);
        }

        /// <summary>
        /// Called after a round ends (win/fold/etc.) to apply life decay and
        /// either start the next round or end the session if the player died.
        /// </summary>
        private void FinalizeAndMaybeContinue()
        {
            // Apply the "cost of life" only if the player is still alive.
            if (wager.PlayerFuelPercent > 0)
            {
                wager.ApplyBetweenRoundDecayToPlayer();
            }

            // If still alive after decay, start a fresh round; else announce death.
            if (wager.PlayerFuelPercent > 0)
            {
                StartNewRound();
            }
            else
            {
                // We used SendMessage to avoid exposing a public method solely for status updates.
                ui.SendMessage("SetStatus", "Player dead. Session over.", SendMessageOptions.DontRequireReceiver);
            }
        }

        // ------------------------- Roulette → Director events ------------------

        /// <summary>Toggle UI and player input when turn changes.</summary>
        private void HandleTurnChanged(bool isPlayerTurn)
        {
            ui.SetIsPlayerTurn(isPlayerTurn);
        }

        /// <summary>
        /// Not strictly needed for settlements (RoundEnded handles it),
        /// but useful for effects/logs if you want later.
        /// </summary>
        private void HandleShotResolved(bool targetIsPlayer, ShotOutcome outcome)
        {
            // Optional: add VFX/SFX hooks here.
            // If outcome == Fired, Roulette will immediately trigger OnRoundEnded.
        }

        /// <summary>Final authority on how the round concluded.</summary>
        private void HandleRoundEnded(RoundResult result)
        {
            switch (result)
            {
                case RoundResult.EnemyDied:
                    // Player takes the pot, then pay life cost and continue
                    wager.ApplyRoundWinToPlayer();
                    FinalizeAndMaybeContinue();
                    break;

                case RoundResult.PlayerDied:
                    // Enemy takes the pot; no decay is applied; session ends.
                    wager.ApplyRoundWinToEnemy();
                    ui.SendMessage("SetStatus", "You died. Session over.", SendMessageOptions.DontRequireReceiver);
                    break;

                case RoundResult.PlayerFolded:
                    // Pot goes to enemy, then life cost and continue (if alive).
                    wager.ApplyRoundWinToEnemy();
                    FinalizeAndMaybeContinue();
                    break;

                case RoundResult.EnemyFolded:
                    // Rare (AI feature later). Pot to player, decay, continue.
                    wager.ApplyRoundWinToPlayer();
                    FinalizeAndMaybeContinue();
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Called when roulette finishes a respin (animation/logic). We don't
        /// need extra handling here—the turn continues naturally.
        /// </summary>
        private void HandleRespinDone()
        {
            // No-op for now. Keep for future VFX/lockouts if needed.
        }

        // ------------------------- UI → Director button hooks ------------------

        /// <summary>Player pressed "Shoot Self".</summary>
        public void OnClick_ShootSelf()
        {
            roulette.PerformAction(ActionType.ShootSelf);
        }

        /// <summary>Player pressed "Shoot Enemy".</summary>
        public void OnClick_ShootEnemy()
        {
            roulette.PerformAction(ActionType.ShootOpponent);
        }

        /// <summary>
        /// Player pressed "Respin": Wager must first charge the **half-pot (ceil)**
        /// unilateral cost and add it to the pot. If payment succeeds, we ask
        /// Roulette to actually respin the cylinder.
        /// </summary>
        public void OnClick_Respin()
        {
            if (wager.ChargeRespinAndAddToPot())
            {
                roulette.RequestRespin();
            }
            else
            {
                ui.SendMessage("SetStatus", "Cannot respin (insufficient fuel).", SendMessageOptions.DontRequireReceiver);
            }
        }

        /// <summary>
        /// Player pressed "Fold": we end the round as a fold. Wager has already
        /// recorded the player's intention; we now need Roulette to issue a
        /// canonical round end so all systems react uniformly.
        /// </summary>
        public void OnClick_Fold()
        {
            wager.PlayerFold();

            // Try to ask the roulette to end as "PlayerFolded".
            // Our MockRoulette has a helper "ForcePlayerFold()".
            // If a real roulette controller doesn't provide this, we fall back
            // to directly invoking HandleRoundEnded to keep the prototype playable.
            var m = rouletteMono.GetType().GetMethod("ForcePlayerFold");
            if (m != null)
            {
                m.Invoke(rouletteMono, null);
            }
            else
            {
                // Fallback so the flow keeps working during integration
                HandleRoundEnded(RoundResult.PlayerFolded);
            }
        }
    }
}
