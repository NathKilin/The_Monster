using System;

namespace TheMonster.Wagering
{
    /// <summary>
    /// Per-round data: pot (in capsules), who paid what, pending raise proposal,
    /// and flags like "player already used respin".
    /// </summary>

    public class RoundState
    {
        //pot and contributions (in capsules)

        //Total capsules in the pot:
        public int PotCapsules { get; private set; }

        // how many capsules the player contributed this round:
        public int PlayerContribution { get; private set; }

        // how many capsules the enemy contributed this round:
        public int EnemyContribution { get; private set; }


    // negotiation ------------------
    /// <summary>
    /// Pending raise proposal in capsules (equal amount both would add).
    /// 0 means no active proposal.
    /// </summary>
    public int PendingRaiseCapsules { get; private set; }

    // FLAGS -----------------------
    /// <summary>True if the player has already used the respin in this round.</summary>
    public bool PlayerUsedRespin { get; private set; }

    /// <summary>True if the enemy has already used the respin in this round.</summary>
    public bool EnemyUsedRespin { get; private set; }

        // Lifecycle --------------------
        public RoundState()
        {
            ResetForNewRound();
        }

        /// <summary>Clear everything for a brand-new round.</summary>
        public void ResetForNewRound()
        {
            PotCapsules = 0;
            PlayerContribution = 0;
            EnemyContribution = 0;
            PendingRaiseCapsules = 0;
            PlayerUsedRespin = false;
            EnemyUsedRespin = false;
        }

        // --- Mutators used by WagerController --------------------------------
        /// <summary>Adds capsules to the pot and to the player's contribution.</summary>
        public void AddFromPlayer(int capsules)
        {
            if (capsules <= 0) return;
            PotCapsules += capsules;
            PlayerContribution += capsules;
        }

        /// <summary>Adds capsules to the pot and to the enemy's contribution.</summary>
        public void AddFromEnemy(int capsules)
        {
            if (capsules <= 0) return;
            PotCapsules += capsules;
            EnemyContribution += capsules;
        }

        /// <summary> set or clear a raise proposal (in capsules, 0 = none)</summary>
        public void SetRaiseProposal(int capsules)
        {
            PendingRaiseCapsules = Math.Max(0, capsules);
        }

        /// <summary>Marks that the player has used their respin.</summary>
        public void UsePlayerRespin()
        {
            PlayerUsedRespin = true;
        }

        /// <summary>Marks that the enemy has used their respin.</summary>
        public void UseEnemyRespin()
        {
            EnemyUsedRespin = true;
        }

        /// <summary> Empties the pot (after win settlement); keeps flags as-is for summaries if needed.</summary>
        public void ClearPot()
        {
            PotCapsules = 0;
            PlayerContribution = 0;
            EnemyContribution = 0;
            PendingRaiseCapsules = 0;
        }

        internal void SetPendingRaise(int capsules)
        {
            // Internal API used by WagerController to set/clear the proposal.
            PendingRaiseCapsules = Math.Max(0, capsules);
        }
    }
}