using DG.Tweening;
using Game.Collectables;
using Game.Gates;
using Game.Interfaces;
using UnityEngine;

namespace Game.Managers
{
    public class ConveyorManager : MonoBehaviour, IConveyorManager
    {
        [Header("Settings")]
        [SerializeField] private float conveyorSpeed;
        [SerializeField] private UpgradeGate[] upgradeGates;

        private int _currentGateIndex;


        #region PUBLIC METHODS

        public void SendBulletToUpgradeGate(BulletRewardData bullet)
        {
            var targetPos = upgradeGates[_currentGateIndex].ConveyorEnd.position;
            var jumpPos = new Vector3(targetPos.x, targetPos.y, bullet.transform.position.z);

            bullet.transform.DOJump(jumpPos, 1, 1, 0.5f).OnComplete(() =>
            {
                bullet.transform.DOMove(targetPos, conveyorSpeed).SetSpeedBased(true).SetEase(Ease.Linear)
                    .OnComplete(() => { upgradeGates[_currentGateIndex].RegisterBullet(bullet); });
            });
        }


        public void MoveNextUpgradeGate() => _currentGateIndex++;

        #endregion
    }
}