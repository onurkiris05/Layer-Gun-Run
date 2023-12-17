using System.Collections.Generic;
using DG.Tweening;
using Game.Collectables;
using Game.Player;
using Game.Projectiles;
using NaughtyAttributes;
using UnityEngine;

namespace Game.Gates
{
    public class UpgradeGate : BaseGate
    {
        [Header("Settings")]
        [SerializeField] private float spacing;
        
        [Space][Header("Components")]
        [SerializeField] private Transform showOffPlace;
        [SerializeField] private Transform conveyorEnd;
        [SerializeField] private GameObject gateModel;
        
        [Space][Header("Debug")]
        [SerializeField] [ReadOnly] private List<BulletRewardData> registeredBullets = new();

        public Transform ConveyorEnd => conveyorEnd;


        #region UNITY EVENTS

        protected override void OnTriggerEnter(Collider other)
        {
            if (_isKilled) return;
            Taptic.Light();
            if (other.TryGetComponent(out BulletProjectile bullet))
            {
                bullet.Kill();
            }
            else if (other.TryGetComponent(out PlayerController player))
            {
                player.OnBulletReward(registeredBullets);
                KillGate();
            }
        }

        #endregion


        #region PUBLIC METHODS

        public virtual void RegisterBullet(BulletRewardData bullet)
        {
            registeredBullets.Add(bullet);
            ArrangeBullets();
        }

        #endregion


        #region PROTECTED METHODS

        protected override void KillGate()
        {
            _isKilled = true;
            gateModel.transform.DOComplete();
            gateModel.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.Linear)
                .OnComplete(() => Destroy(gameObject));
        }


        protected override void InitGate()
        {
        }


        protected virtual void ArrangeBullets()
        {
            for (int i = 0; i < registeredBullets.Count; i++)
            {
                var totalWidth = (registeredBullets.Count - 1) * spacing;
                var offset = -totalWidth / 2f + i * spacing;
                var pos = new Vector3(showOffPlace.position.x + offset,
                    showOffPlace.position.y, showOffPlace.position.z);

                registeredBullets[i].transform.DOJump(pos, 1, 1, 0.5f);
            }
        }

        #endregion
    }
}