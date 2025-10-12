namespace TheMonster.Wagering
{
    /// <summary>
///Safely stores and updates enemy fuel.
///Guarantees multiples of 10, 0–100.
///Provides capsule spend/gain just like the player. and testing.
    /// </summary>
    public class EnemyState
    {
        public int FuelPercent { get; private set; }
        public bool IsAlive => FuelPercent > 0;
        public int Capsules => FuelPercent / 10;

        public EnemyState(int startPercent)
        {
            FuelPercent = ClampToStep10(startPercent);
        }

        public void AddFuelPercent(int percent)
        {
            FuelPercent = ClampToStep10(FuelPercent + percent);
        }

        public void RemoveFuelPercent(int percent)
        {
            FuelPercent = ClampToStep10(FuelPercent - percent);
        }

        public bool TrySpendCapsules(int capsules)
        {
            if (capsules <= 0) return false;
            int cost = capsules * 10;
            if (FuelPercent < cost) return false;
            FuelPercent = ClampToStep10(FuelPercent - cost);
            return true;
        }

        public void GainCapsules(int capsules)
        {
            if (capsules <= 0) return;
            FuelPercent = ClampToStep10(FuelPercent + capsules * 10);
        }

        public static int ClampToStep10(int value)
        {
            if (value < 0) value = 0;
            if (value > 100) value = 100;
            return (value / 10) * 10;
        }
    }
}
