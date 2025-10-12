namespace Components
{
    public interface IOpponent
    {
        public int timesLeftToRespin { get; protected set; }
        public int timesLeftToShootOpponent { get; protected set; }

        // The logic with which either player / enemyAI decide what happens
        public void TurnLogic();

        // Action 1 - Shoot either player / enemyAI
        // Extend to provide logic for player & enemy AI
        public void ShootOpponent();

        // Action 2 - player / enemyAI Shoots themselves
        // Extend for each handler ( enemyAI / PlayerManager )
        public void ShootSelf();

        // Call From ShootSelf & ShootOpponent At the end
        // handles what happens when shot at -> ex : Play Death Animation ...
        public void OnShot();
    }
}