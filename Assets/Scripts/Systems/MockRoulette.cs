using System;
using System.Collections;
using UnityEngine;
using TheMonster.Common;

namespace TheMonster.Systems
{
    /// <summary>
    /// Minimal mock implementation of IRouletteAPI used for local testing.
    /// - Deterministic/simple outcomes suitable for manual testing.
    /// - Fires events quickly so WagerController/GameDirector can react.
    /// This is a temporary stub; replace with the real Roulette later.
    /// </summary>
    public class MockRoulette : MonoBehaviour, IRouletteAPI
    {
        public event Action<bool> OnTurnChanged;
        public event Action<bool, ShotOutcome> OnShotResolved;
        public event Action<RoundResult> OnRoundEnded;
        public event Action OnRespinDone;

        // Basic config for deterministic testing
        [SerializeField] bool _playerStarts = true;
        [SerializeField] float _actionResolveDelay = 0.5f; // seconds

        bool _currentIsPlayerTurn;

        public void InitializeRound(bool playerStarts)
        {
            _currentIsPlayerTurn = playerStarts;
            OnTurnChanged?.Invoke(_currentIsPlayerTurn);
        }

        public bool CanRespin() => true; // allow respin for testing

        public void RequestRespin()
        {
            // Simulate respin completion after a tiny delay
            StartCoroutine(DoRespin());
        }

        IEnumerator DoRespin()
        {
            yield return new WaitForSeconds(_actionResolveDelay);
            OnRespinDone?.Invoke();
        }

        public void PerformAction(ActionType action)
        {
            // Simulate action resolution after a delay
            StartCoroutine(ResolveAction(action));
        }

        IEnumerator ResolveAction(ActionType action)
        {
            yield return new WaitForSeconds(_actionResolveDelay);

            // Simple deterministic rules for test:
            // - If ShootSelf: player gets 'Fired' (loses a shot)
            // - If ShootOpponent: enemy gets 'Fired' with 50% chance
            bool targetIsPlayer = false;
            ShotOutcome outcome = ShotOutcome.Empty;

            switch (action)
            {
                case ActionType.ShootSelf:
                    targetIsPlayer = true;
                    outcome = ShotOutcome.Fired;
                    break;
                case ActionType.ShootOpponent:
                    targetIsPlayer = false;
                    outcome = (UnityEngine.Random.value < 0.5f) ? ShotOutcome.Fired : ShotOutcome.Empty;
                    break;
                case ActionType.Respin:
                    // Shouldn't reach here; respin handled by RequestRespin
                    break;
            }

            OnShotResolved?.Invoke(targetIsPlayer, outcome);

            // Decide round end: if Fired happened and target died the round ends.
            // The Mock doesn't track health — leave it to GameDirector to call ApplyRoundWinToPlayer/Enemy
            // We'll emit a RoundEnded signal with None so GameDirector can make settlement decisions.
            OnRoundEnded?.Invoke(RoundResult.None);

            // Toggle turn for next action
            _currentIsPlayerTurn = !_currentIsPlayerTurn;
            OnTurnChanged?.Invoke(_currentIsPlayerTurn);
        }
    }
}
