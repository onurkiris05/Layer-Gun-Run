using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Game.Collectables;
using Game.Projectiles;
using UnityEngine;

namespace Game.Player
{
    public class Magazine : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private BulletProjectile bulletPrefab;
        [SerializeField] private Transform[] bulletHolders;

        public List<BulletProjectile> CurrentBullets => _currentBullets;
        private List<BulletProjectile> _currentBullets = new();


        #region PUBLIC METHODS

        public void FillWithBullets(float bulletPower)
        {
            foreach (var holder in bulletHolders)
            {
                BulletProjectile bullet = Instantiate(bulletPrefab, holder.position, holder.rotation, holder.transform);
                bullet.InitMagazine(this, bulletPower);
                _currentBullets.Add(bullet);
            }
        }
        public void SetPowers(float newPower)
        {
            for(int i = 0; i < _currentBullets.Count; i++)
            {
                _currentBullets[i].InitMagazine(this, newPower);
            }
        }

        public void ClearBullets()
        {
            foreach (var bullet in _currentBullets)
            {
                bullet.transform.DOKill();
                Destroy(bullet.gameObject);
            }

            _currentBullets.Clear();
        }


        public void SetBullets() => StartCoroutine(ProcessSetBullets());


        public void Swap(BulletProjectile oldBullet, BulletRewardData newBulletData)
        {
            if (_currentBullets.Contains(oldBullet))
            {
                oldBullet.transform.DOLocalMoveZ(-0.1f, 0.5f).SetEase(Ease.Linear).SetRelative()
                    .OnComplete(() =>
                    {
                        CreateNewBullet(
                            oldBullet.transform.parent,
                            oldBullet.transform.localPosition,
                            newBulletData.Power);

                        oldBullet.SetDiscarded();
                        _currentBullets.Remove(oldBullet);
                    });
            }
        }

        #endregion


        #region PRIVATE METHODS

        private void CreateNewBullet(Transform holder, Vector3 localPos, int power)
        {
            BulletProjectile bullet = Instantiate(bulletPrefab, holder.position, holder.rotation, holder.transform);
            bullet.InitMagazine(this, power);
            bullet.transform.localPosition = localPos;
            bullet.transform.localScale = Vector3.zero;
            bullet.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.Linear);
            bullet.transform.DOLocalMove(Vector3.zero, 0.5f).SetEase(Ease.Linear);

            _currentBullets.Add(bullet);
        }


        private IEnumerator ProcessSetBullets()
        {
            yield return null;

            foreach (var bullet in _currentBullets)
                bullet.transform.localPosition = Vector3.zero;
        }

        #endregion
    }
}