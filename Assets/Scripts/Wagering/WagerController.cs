using System;
using UnityEngine;
using TheMonster.Common;

namespace TheMonster.Wagering
{
    /// <summary>
    /// Owns fuel, pot, raises, respin COST (half-pot, ceil), fold, and life-decay.
    /// Emits simple events so UI can update. Does NOT do gun logic.
    /// </summary>
    public class WagerController : MonoBehaviour, IWagerAPI
    {
        [Header("Config")]
        [SerializeField] int _playerStartFuel = 40;  // % (multiples of 10)
        [SerializeField] int _lifeCostPercent = 10;  // % lost between rounds (if alive)

        // State
        public PlayerState Player { get; private set; }
        public EnemyState  Enemy  { get; private set; }
        public RoundState  Round  { get; private set; } = new RoundState();

        // UI/Director hooks
        public event Action<int> OnPotChanged;
        public event Action<int> OnPlayerFuelChanged;
        public event Action<int> OnEnemyFuelChanged;
        public event Action<int> OnRaiseProposed; // 0 = cleared
        void Awake()
        {
            Player = new PlayerState(_playerStartFuel);
        }

        // IWagerAPI read-only mirrors (for external reads)
        public int PlayerFuelPercent => Player?.FuelPercent ?? 0;
        public int EnemyFuelPercent  => Enemy?.FuelPercent  ?? 0;
        public int PotCapsules       => Round?.PotCapsules  ?? 0;
        /// <summary>
        /// Called by GameDirector at the start of each round.
        /// - Creates/refreshes enemy
        /// - Clears RoundState
        /// - Collects ante: 1 capsule each (if they can)
        /// </summary>
        public void StartNewRound(bool playerStarts, int enemyFuelPercent)
        {
            Enemy = new EnemyState(enemyFuelPercent);
            Round.ResetForNewRound();

            // Ante: 1 capsule each (10%)
            AddCapsulesFromPlayer(1);
            AddCapsulesFromEnemy(1);
            // Notify UI about cleared raise and pot
            OnRaiseProposed?.Invoke(0);
            RaisePotChanged();
        }
        /// <summary>Proposer suggests "+N capsules" (both sides would add N).</summary>
        public bool TryProposeRaise(int capsules)
        {
            if (capsules <= 0) return false;
            Round.SetPendingRaise(capsules);
            OnRaiseProposed?.Invoke(capsules);
            return true;
        }

        /// <summary>Responder accepts; both must afford N. Pot += 2N.</summary>
        public bool TryAcceptRaise()
        {
            int n = Round.PendingRaiseCapsules;
            if (n <= 0) return false;

            // Check affordability first
            if (Player.Capsules < n || Enemy.Capsules < n) return false;

            AddCapsulesFromPlayer(n);
            AddCapsulesFromEnemy(n);
            Round.SetPendingRaise(0);
            OnRaiseProposed?.Invoke(0);
            RaisePotChanged();
            return true;
        }

        /// <summary>Responder declines; keep current pot.</summary>
        public void DeclineRaise()
        {
            Round.SetPendingRaise(0);
            OnRaiseProposed?.Invoke(0);
        }
        /// <summary>Returns true if player can pay ceil(pot/2). Also returns that cost.</summary>
        public bool CanAffordRespin(out int costCapsules)
        {
            costCapsules = (Round.PotCapsules + 1) / 2; // ceil
            return Player.Capsules >= costCapsules && !Round.PlayerUsedRespin && costCapsules > 0;
        }

        /// <summary>
        /// Charges player ceil(pot/2) capsules, adds to pot, and marks respin as used.
        /// The caller should then ask Roulette to actually spin.
        /// </summary>
        public bool ChargeRespinAndAddToPot()
        {
            if (!CanAffordRespin(out int cost)) return false;
            if (!Player.TrySpendCapsules(cost)) return false;

            Round.AddFromPlayer(cost);
            Round.UsePlayerRespin();
            OnPlayerFuelChanged?.Invoke(Player.FuelPercent);
            RaisePotChanged();
            return true;
        }
        /// <summary>
        /// Player abandons the pot. The enemy will receive the whole pot when
        /// GameDirector finalizes round. (We don't move chips here to keep a single settlement point.)
        /// </summary>
        public void PlayerFold()
        {
            // Intentionally minimal: GD will call ApplyRoundWinToEnemy()
            // and then ApplyBetweenRoundDecayToPlayer() if player survived the round.
        }
        /// <summary>Give pot to player, clamp to 100%, clear pot.</summary>
        public void ApplyRoundWinToPlayer()
        {
            Player.GainCapsules(Round.PotCapsules);
            Round.ClearPot();
            OnPlayerFuelChanged?.Invoke(Player.FuelPercent);
            RaisePotChanged();
        }

        /// <summary>Give pot to enemy, clamp to 100%, clear pot.</summary>
        public void ApplyRoundWinToEnemy()
        {
            Enemy.GainCapsules(Round.PotCapsules);
            Round.ClearPot();
            OnEnemyFuelChanged?.Invoke(Enemy.FuelPercent);
            RaisePotChanged();
        }

        /// <summary>Apply life cost (–10%) after surviving a round.</summary>
        public void ApplyBetweenRoundDecayToPlayer()
        {
            Player.RemoveFuelPercent(_lifeCostPercent);
            OnPlayerFuelChanged?.Invoke(Player.FuelPercent);
        }
        void AddCapsulesFromPlayer(int n)
        {
            if (n <= 0) return;
            if (!Player.TrySpendCapsules(n)) return;
            Round.AddFromPlayer(n);
            OnPlayerFuelChanged?.Invoke(Player.FuelPercent);
        }

        void AddCapsulesFromEnemy(int n)
        {
            if (n <= 0) return;
            if (!Enemy.TrySpendCapsules(n)) return;
            Round.AddFromEnemy(n);
            OnEnemyFuelChanged?.Invoke(Enemy.FuelPercent);
        }

        void RaisePotChanged() => OnPotChanged?.Invoke(Round.PotCapsules);
    }
}
