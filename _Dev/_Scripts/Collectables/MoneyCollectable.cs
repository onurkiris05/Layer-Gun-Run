using DG.Tweening;
using Game.Interfaces;
using Game.Player;
using UnityEngine;
using Zenject;

namespace Game.Collectables
{
    public class MoneyCollectable : BaseCollectable
    {
        [Header("Settings")]
        [SerializeField] private int amount;
        [SerializeField] private bool isGoingToConveyor;

        [Inject] private IUIManager uiManager;
        [Inject] private IEconomyManager economyManager;

        public int Amount => amount;

        private BoxCollider _collider;
        private bool _isCollected;


        #region UNITY EVENTS

        private void Awake() => _collider = GetComponent<BoxCollider>();

        protected override void OnTriggerEnter(Collider other)
        {
            if (_isCollected) return;

            if (other.TryGetComponent(out PlayerController player))
            {
                _isCollected = true;

                if (isGoingToConveyor)
                {
                    // ConveyorManager.Instance.SendMoneyToCashBox(this);
                }
                else
                {
                    transform.DOScale(Vector3.zero, 0.2f).OnComplete(() =>
                    {
                        ImageSpawner.Instance.SpawnAndMove("Money", transform.position, uiManager.WalletTextUI, 3);
                        PlayerPrefs.SetInt("Coin", PlayerPrefs.GetInt("Coin")+(int)( 10* PlayerPrefs.GetFloat("IncomeMultiplier")));
                        Kill();
                    });
                }
            }
        }

        #endregion


        #region PUBLIC METHODS

        public void SetState(bool state) => _collider.enabled = state;

        public void SetReward(int reward) => amount = reward;

        public void Kill() => Destroy(gameObject);

        #endregion
    }
}