using System.Collections.Generic;
using DG.Tweening;
using Game.Projectiles;
using UnityEngine;

namespace Game.Player
{
    public class Pointer : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float angleLimit;
        [SerializeField] private float pointerSpeed;
        [SerializeField] private float sizeFactor;

        public Dictionary<Magazine, BulletProjectile> ActiveBullets => _activeBullets;
        private Dictionary<Magazine, BulletProjectile> _activeBullets = new();
        private Collider _collider;


        #region UNITY EVENTS

        private void Awake() => _collider = GetComponent<Collider>();


        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out BulletProjectile bullet))
            {
                if (!_activeBullets.ContainsValue(bullet))
                {
                    if (_activeBullets.ContainsKey(bullet.Magazine))
                        _activeBullets[bullet.Magazine].SetHighlighted(false);

                    _activeBullets[bullet.Magazine] = bullet;
                    bullet.SetHighlighted(true);
                }
            }
        }

        #endregion


        #region PUBLIC METHODS

        public void SetActive(bool isActive)
        {
            _collider.enabled = isActive;

            if (isActive)
            {
                transform.localRotation = Quaternion.Euler(0, 0, -angleLimit);
                transform.DOLocalRotate(new Vector3(0, 0, angleLimit), pointerSpeed)
                    .SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo).SetSpeedBased();
            }
            else
            {
                transform.DOKill();
                transform.DOLocalRotate(new Vector3(0, 0, -angleLimit), pointerSpeed * 2f)
                    .SetEase(Ease.Linear).SetSpeedBased();

                foreach (var kvp in _activeBullets)
                    kvp.Value.SetHighlighted(false);

                _activeBullets.Clear();
            }
        }


        public void SetSize(int magazineIndex)
        {
            transform.DOScaleY(sizeFactor + (magazineIndex * sizeFactor), 1f).SetEase(Ease.Linear);
        }


        public void Reset() => _activeBullets.Clear();

        #endregion
    }
}