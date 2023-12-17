using DG.Tweening;
using Game.Player;
using Game.Projectiles;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Gates
{
    public enum GateType
    {
        Rate,
        Range
    }

    public class GenericGate : BaseGate
    {
        [Header("Settings")]
        [SerializeField] protected float amount;
        [SerializeField] protected float amountDivider;
        [SerializeField] protected GateType gateType;

        [Space] [Header("Unlock Settings")]
        [SerializeField] protected bool isLocked;
        [SerializeField] protected int unlockHealth;

        [Space] [Header("Components")]
        [SerializeField] protected Slider unlockBar;
        [SerializeField] protected GameObject normalGate;
        [SerializeField] protected GameObject lockGate;
        [SerializeField] protected TextMeshPro amountText;
        [SerializeField] protected TextMeshPro headerText;

        [Space] [Header("Debug")]
        [SerializeField] [ReadOnly] private float totalDamageTaken;

        private MultipleGate _parentGate;


        #region UNITY EVENTS

        protected override void OnTriggerEnter(Collider other)
        {
            if (_isKilled) return;

            if (other.TryGetComponent(out BulletProjectile bullet))
            {
                Taptic.Light();
                VFXSpawner.Instance.PlayVFX("BulletHit", bullet.transform.position);
                InteractEffect();
                var bulletPower = bullet.Power;
                bullet.Kill();

                if (isLocked)
                {
                    UpdateLockStatus(bulletPower);
                    return;
                }

                UpdateGate(bulletPower);
            }
            else if (other.TryGetComponent(out PlayerController player))
            {
                if (!isLocked) player.OnGateReward(gateType, amount / amountDivider);
                KillGate();
            }
        }

        #endregion


        #region PUBLIC METHODS

        public virtual void SetParentGate(MultipleGate parentGate) => _parentGate = parentGate;

        public virtual void DisableGate() => _isKilled = true;

        #endregion


        #region PROTECTED METHODS

        protected virtual void UpdateLockStatus(float bulletPower)
        {
            totalDamageTaken += bulletPower;

            DOTween.Complete(this);
            DOTween.To(x => unlockBar.value = x, unlockBar.value,
                (float)totalDamageTaken / unlockHealth, 0.3f).SetEase(Ease.Linear).SetId(this);

            if (totalDamageTaken >= unlockHealth)
            {
                isLocked = false;
                SetGates();
            }
        }


        protected override void InitGate()
        {
            SetGates();

            amountText.text = amount.ToString("0");

            switch (gateType)
            {
                case GateType.Rate:
                    headerText.text = "Rate";
                    break;
                case GateType.Range:
                    headerText.text = "Range";
                    break;
            }
        }


        protected override void KillGate()
        {
            _isKilled = true;

            if (_parentGate != null)
                _parentGate.DeactivateAll(this);

            transform.DOComplete();
            transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InElastic)
                .OnComplete(() => Destroy(gameObject));
        }


        protected virtual void SetGates()
        {
            normalGate.SetActive(!isLocked);
            lockGate.SetActive(isLocked);
            unlockBar.gameObject.SetActive(isLocked);
        }


        protected virtual void UpdateGate(float bulletPower)
        {
            amount += bulletPower;
            amountText.text = $"+{amount}";
        }


        protected virtual void InteractEffect()
        {
            transform.DOComplete();
            transform.DOShakeScale(0.15f, new Vector3(0.2f, 0.2f, 0.2f));
        }

        #endregion
    }
}