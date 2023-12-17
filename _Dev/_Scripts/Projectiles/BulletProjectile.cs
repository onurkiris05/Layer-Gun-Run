using DG.Tweening;
using Game.Player;
using UnityEngine;

namespace Game.Projectiles
{
    public class BulletProjectile : ProjectileBase
    {
        [SerializeField] private Color[] bulletTextColors;

        public Magazine Magazine { get; private set; }
        public System.IFormatProvider _format;

        #region PUBLIC METHODS
        
        public override void Init(ProjectileData data)
        {
            Debug.Log(data.Range + "RangeFromData");
            _rigidbody.velocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            _rigidbody.AddForce(transform.forward * projectileSpeed, ForceMode.VelocityChange);

            _projectileBehaviour = Behaviour_Standard;
            _collider.enabled = true;
            _range = data.Range;
            _power = data.Power;
            transform.localScale = data.Scale;
            _startPos = transform.position;

            // SetPowerText(true);
            powerText.enabled = false;
            SetTrail(true);
        }


        public void InitMagazine(Magazine parentMagazine, float power)
        {
            Magazine = parentMagazine;
            _power = power;
            SetPowerText();
        }


        public override void Kill()
        {
            _collider.enabled = false;
            transform.localScale = Vector3.one;
            SetTrail(false);
            ObjectPooler.Instance.ReleasePooledObject("Bullet", gameObject);
        }


        public void SetHighlighted(bool isHighlighted)
        {
            var zValue = isHighlighted ? 0.1f : 0f;

            transform.DOComplete();
            transform.DOLocalMoveZ(zValue, 0.1f);
        }


        public void SetDiscarded()
        {
            transform.parent = null;
            _collider.isTrigger = false;
            _rigidbody.useGravity = true;
        }


        public void ProcessFireBulletSequence(float fireRate)
        {
            var t = transform;

            t.DOComplete();
            t.DOLocalMoveZ(0.2f, fireRate / 20f).OnComplete(() =>
            {
                t.localScale = Vector3.zero;
                t.localPosition = new Vector3(t.localPosition.x, t.localPosition.y, t.localPosition.z - 1.5f);

                t.DOLocalMoveZ(0f, fireRate / 5f);
                t.DOScale(Vector3.one, fireRate / 5f);
            });

            VFXSpawner.Instance.PlayVFX("FireBullet", transform.position, transform.parent);
        }

        #endregion


        #region PRIVATE METHODS

        private void SetPowerText(bool isFiring = false)
        {
            bool hasDecimals = false;
            if (Power % 1 == 0)
            {
                hasDecimals = false;
            }
            else
            {
                hasDecimals = true;
            }
            if (!hasDecimals)
            {
                
                    
                powerText.text = Power.ToString("0");
            }
            else
            {
                powerText.text = Power.ToString("0.0");
            }
            powerText.color = Color.white;

            if (isFiring)
            {
                powerText.fontSize = 2f;
                powerText.rectTransform.DOLocalMove(new Vector3(0f, 0.15f, 0f), 1f)
                    .SetEase(Ease.Flash);
            }
        }

        #endregion
    }
}