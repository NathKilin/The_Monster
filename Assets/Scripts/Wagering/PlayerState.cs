namespace TheMonster.Wagering
{
    /// <summary>
    /// Holds a character's life/energy as PERCENT (0..100 in steps of 10).
    /// Provides capsule-based helpers (1 capsule = 10%).
    /// No UI, no game flow — just numbers and safety.
    /// </summary>
    public class PlayerState
    {
        // ---- Core value ------------------------------------------------------

        /// <summary>Current fuel as a percent (always multiple of 10).</summary>
        public int FuelPercent { get; private set; }

        /// <summary>Alive if fuel > 0.</summary>
        public bool IsAlive => FuelPercent > 0;

        /// <summary>How many 10% units the player has.</summary>
        public int Capsules => FuelPercent / 10;

        // ---- Construction ----------------------------------------------------

        public PlayerState(int startPercent)
        {
            FuelPercent = ClampToStep10(startPercent);
        }

        // ---- Public operations (used by WagerController) ---------------------

        /// <summary>Adds fuel (percent), clamps to 0..100 and to steps of 10.</summary>
        public void AddFuelPercent(int percent)
        {
            FuelPercent = ClampToStep10(FuelPercent + percent);
        }

        /// <summary>Removes fuel (percent), clamps to 0..100 and to steps of 10.</summary>
        public void RemoveFuelPercent(int percent)
        {
            FuelPercent = ClampToStep10(FuelPercent - percent);
        }

        /// <summary>Spend N capsules (10% each). Returns false if not enough.</summary>
        public bool TrySpendCapsules(int capsules)
        {
            if (capsules <= 0) return false;
            int cost = capsules * 10;
            if (FuelPercent < cost) return false;

            FuelPercent -= cost;
            FuelPercent = ClampToStep10(FuelPercent);
            return true;
        }

        /// <summary>Gain N capsules (10% each). Caps at 100%.</summary>
        public void GainCapsules(int capsules)
        {
            if (capsules <= 0) return;
            FuelPercent = ClampToStep10(FuelPercent + capsules * 10);
        }

        // ---- Helpers ---------------------------------------------------------

        /// <summary>Clamp to [0,100] and snap to multiples of 10.</summary>
        public static int ClampToStep10(int value)
        {
            if (value < 0) value = 0;
            if (value > 100) value = 100;
            // snap to nearest lower multiple of 10 (game only uses 0,10,20,...)
            return (value / 10) * 10;
        }
    }
}
