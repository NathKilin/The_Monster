using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TheMonster.Wagering
{
    /// <summary>
    /// Pure UI glue: displays fuel and pot, enables/disables actions,
    /// forwards button clicks to WagerController.
    /// GameDirector (or Roulette) must call SetIsPlayerTurn().
    /// </summary>
    public class WagerUIController : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private WagerController wager;   // drag your WagerController here

        [Header("Bars")]
        [SerializeField] private Slider playerFuelBar;    // 0..100
        [SerializeField] private Slider enemyFuelBar;     // 0..100

        [Header("Texts")]
        [SerializeField] private TMP_Text potText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_Text respinCostText; // optional small label

        [Header("Buttons: Actions")]
        [SerializeField] private Button btnShootSelf;
        [SerializeField] private Button btnShootEnemy;
        [SerializeField] private Button btnRespin;
        [SerializeField] private Button btnFold;

        [Header("Buttons: Betting")]
        [SerializeField] private Button btnRaisePlus1;
        [SerializeField] private Button btnRaisePlus2;
        [SerializeField] private Button btnRaisePlus3;
        [SerializeField] private Button btnAcceptRaise;
        [SerializeField] private Button btnDeclineRaise;

        // State
        private bool _isPlayerTurn;

        void OnEnable()
        {
            if (wager == null) return;

            // Subscribe to wager events so UI auto-updates
            wager.OnPotChanged        += HandlePotChanged;
            wager.OnPlayerFuelChanged += HandlePlayerFuelChanged;
            wager.OnEnemyFuelChanged  += HandleEnemyFuelChanged;
            wager.OnRaiseProposed     += HandleRaiseProposed;

            // Initialize visuals from current data (in case we enter mid-round)
            HandlePlayerFuelChanged(wager.PlayerFuelPercent);
            HandleEnemyFuelChanged(wager.EnemyFuelPercent);
            HandlePotChanged(wager.PotCapsules);
            HandleRaiseProposed(0);
            RefreshRespinCost();
            RefreshButtons();
        }

        void OnDisable()
        {
            if (wager == null) return;

            wager.OnPotChanged        -= HandlePotChanged;
            wager.OnPlayerFuelChanged -= HandlePlayerFuelChanged;
            wager.OnEnemyFuelChanged  -= HandleEnemyFuelChanged;
            wager.OnRaiseProposed     -= HandleRaiseProposed;
        }

        // --------- Public hooks (called by GameDirector/Roulette) --------------

        /// <summary>Called by GameDirector on Roulette.OnTurnChanged</summary>
        public void SetIsPlayerTurn(bool isPlayerTurn)
        {
            _isPlayerTurn = isPlayerTurn;
            SetStatus(isPlayerTurn ? "Your turn." : "Opponent's turn…");
            RefreshButtons();
        }

        // --------- Button handlers (wire these in the Inspector) ----------------

        public void OnClick_ShootSelf()
        {
            // GameDirector should translate this to Roulette.PerformAction(ShootSelf)
            SetStatus("Action: Shoot Self");
        }

        public void OnClick_ShootEnemy()
        {
            // GameDirector should translate this to Roulette.PerformAction(ShootOpponent)
            SetStatus("Action: Shoot Enemy");
        }

        public void OnClick_Respin()
        {
            if (!wager.CanAffordRespin(out int cost))
            {
                SetStatus("Cannot respin (insufficient fuel).");
                return;
            }

            // Charge the pot here; then GameDirector should call Roulette.RequestRespin()
            bool charged = wager.ChargeRespinAndAddToPot();
            if (charged)
            {
                SetStatus($"Respin paid: {cost} capsule(s).");
                RefreshRespinCost(); // pot changed -> cost changed
                RefreshButtons();    // might disable after one use
            }
            else
            {
                SetStatus("Respin charge failed.");
            }
        }

        public void OnClick_Fold()
        {
            // Economically ends round; GameDirector will finalize settlement.
            wager.PlayerFold();
            SetStatus("You folded. Pot will go to the opponent.");
            RefreshButtons();
        }

        public void OnClick_RaisePlus1() => ProposeRaise(1);
        public void OnClick_RaisePlus2() => ProposeRaise(2);
        public void OnClick_RaisePlus3() => ProposeRaise(3);

        public void OnClick_AcceptRaise()
        {
            if (wager.TryAcceptRaise())
            {
                SetStatus("Raise accepted.");
                RefreshRespinCost(); // pot changed
                RefreshButtons();
            }
            else
            {
                SetStatus("Cannot accept raise (insufficient fuel for one of the sides).");
            }
        }

        public void OnClick_DeclineRaise()
        {
            wager.DeclineRaise();
            SetStatus("Raise declined.");
            RefreshButtons();
        }

        // --------- Internal helpers --------------------------------------------

        private void ProposeRaise(int n)
        {
            if (n <= 0) return;

            // Quick affordability check for *player* only (enemy is AI/other system)
            if (wager.PlayerFuelPercent < n * 10)
            {
                SetStatus($"Cannot propose +{n}: not enough capsules.");
                return;
            }

            bool ok = wager.TryProposeRaise(n);
            if (ok)
            {
                SetStatus($"Proposed raise: +{n} capsule(s) each.");
            }
            else
            {
                SetStatus("Raise proposal failed.");
            }
        }

        private void RefreshButtons()
        {
            // Action buttons depend on turn
            bool enableActions = _isPlayerTurn;

            if (btnShootSelf)  btnShootSelf.interactable  = enableActions;
            if (btnShootEnemy) btnShootEnemy.interactable = enableActions;
            if (btnFold)       btnFold.interactable       = enableActions;

            // Respin: also requires affordability
            if (btnRespin)
            {
                bool afford = wager.CanAffordRespin(out _);
                btnRespin.interactable = enableActions && afford;
            }

            // Betting: you can propose raises only on your turn
            bool canPropose = _isPlayerTurn;
            if (btnRaisePlus1) btnRaisePlus1.interactable = canPropose;
            if (btnRaisePlus2) btnRaisePlus2.interactable = canPropose;
            if (btnRaisePlus3) btnRaisePlus3.interactable = canPropose;

            // Accept/Decline shown when there is a pending proposal (from either side)
            // We simply allow pressing them on your turn.
            if (btnAcceptRaise)  btnAcceptRaise.interactable  = _isPlayerTurn;
            if (btnDeclineRaise) btnDeclineRaise.interactable = _isPlayerTurn;
        }

        private void RefreshRespinCost()
        {
            if (respinCostText == null) return;
            if (wager.CanAffordRespin(out int cost))
                respinCostText.text = $"Respin cost: {cost} capsule(s)";
            else
                respinCostText.text = "Respin cost: unavailable";
        }

        private void SetStatus(string msg)
        {
            if (statusText) statusText.text = msg;
        }

        // --------- Event handlers from WagerController -------------------------

        private void HandlePotChanged(int pot)
        {
            if (potText) potText.text = $"Pot: {pot} capsule(s)";
            RefreshRespinCost();
        }

        private void HandlePlayerFuelChanged(int percent)
        {
            if (playerFuelBar) playerFuelBar.value = percent;
        }

        private void HandleEnemyFuelChanged(int percent)
        {
            if (enemyFuelBar) enemyFuelBar.value = percent;
        }

        private void HandleRaiseProposed(int capsules)
        {
            if (capsules <= 0)
            {
                SetStatus("No active raise.");
            }
            else
            {
                SetStatus($"Pending raise: +{capsules} capsule(s) each. Accept or decline.");
            }
        }
    }
}
