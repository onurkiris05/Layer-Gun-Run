using DG.Tweening;
using Game.Interfaces;
using Game.Managers;
using Game.Player;
using Game.Projectiles;
using TMPro;
using UnityEngine;
using Zenject;

namespace Game.Collectables
{
    public class EndGameBarrelCollectable : BaseCollectable
    {
        [Header("Settings")]
        [SerializeField] private float hitCount;

        [Space] [Header("Components")]
        [SerializeField] private GameObject barrelObject;
        [SerializeField] private MoneyCollectable moneyCollectablePrize;
        [SerializeField] private Transform collectablePos;
        [SerializeField] private TextMeshPro hitCountText;

        [Inject] private IGameManager gameManager;

        private bool _isKilled;

        
        #region UNITY EVENTS

        private void Start()
        {
            hitCountText.text = $"{hitCount}";
            moneyCollectablePrize.SetState(false);
        }

        protected override void OnTriggerEnter(Collider other)
        {
            if (_isKilled) return;

            if (other.TryGetComponent(out BulletProjectile bullet))
            {
                Taptic.Light();
                hitCount -= bullet.Power;
                hitCountText.text = $"{hitCount}";

                VFXSpawner.Instance.PlayVFX("BulletHit", bullet.transform.position);
                bullet.Kill();

                barrelObject.transform.DOComplete();
                barrelObject.transform.DOPunchScale(Vector3.one * 0.1f, 0.2f);

                if (hitCount <= 0)
                {
                    _isKilled = true;
                    ReleasePrize();
                }
            }
            else if (other.TryGetComponent(out PlayerController player))
            {
                gameManager.ChangeState(GameState.End);
            }
        }

        #endregion


        #region PUBLIC METHODS

        public void Set(int hitCount, int moneyReward)
        {
            this.hitCount = hitCount;
            hitCountText.text = hitCount.ToString("0");
            moneyCollectablePrize.SetReward(moneyReward);
        }

        #endregion

        
        #region PRIVATE METHODS

        private void ReleasePrize()
        {
            // VFXSpawner.Instance.PlayVFX("BarrelExplosion", transform.position);
            hitCountText.enabled = false;
            barrelObject.transform.DOScale(Vector3.zero, 0.1f).SetEase(Ease.InBack)
                .OnComplete(() => barrelObject.SetActive(false));
            moneyCollectablePrize.transform.DOJump(collectablePos.position, 3, 1, 0.5f)
                .OnComplete(() => moneyCollectablePrize.SetState(true));
        }

        #endregion
    }
}