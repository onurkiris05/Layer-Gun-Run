using System;
using Game.Gates;
using Game.Interfaces;
using Game.Projectiles;
using UnityEngine;
using Zenject;

namespace Game.Player
{
    public class ShootHandler : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float fireRate;
        public float fireRange;

        [Space] [Header("Components")]
        [SerializeField] private Transform shootPoint;

        private PlayerController _player;
        private Action _currentShotType;

        public static ShootHandler instance;

        #region ENCAPSULATIONS

        public float FireRate
        {
            get => fireRate;
            private set
            {
                fireRate = value;
                _player.OnFireRateUpdate?.Invoke(fireRate);
            }
        }
        #endregion


        #region UNITY EVENTS

        private void Awake()
        {
            instance = this;
        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                RangeUpgrade();
            }
        }
        private void Start()
        {
            if (PlayerPrefs.GetFloat("StartFireRate") == 0)
            {
                PlayerPrefs.SetFloat("StartFireRate", 2);
            }
            if (PlayerPrefs.GetFloat("StartFireRange") == 0)
            {
                PlayerPrefs.SetFloat("StartFireRange", 16);
            }
            fireRange = PlayerPrefs.GetFloat("StartFireRange");
            FireRate = PlayerPrefs.GetFloat("StartFireRate");
            _currentShotType = StandardShot;
            _player.OnFireRateUpdate?.Invoke(FireRate);
        }

        #endregion


        #region PUBLIC METHODS

        public void Init(PlayerController player)
        {
            _player = player;
        }
        public void RateUpgrade()
        {
            PlayerPrefs.SetFloat("StartFireRate", PlayerPrefs.GetFloat("StartFireRate") + .1f);
            FireRate += .1f;
        }
        public void RangeUpgrade()
        {
            PlayerPrefs.SetFloat("StartFireRange", PlayerPrefs.GetFloat("StartFireRange") + .2f);
            fireRange += 1f;
        }

        public void ProcessGateReward(GateType gateType, float rewardAmount)
        {
            switch (gateType)
            {
                case GateType.Rate:
                    FireRate += rewardAmount;
                    break;
                case GateType.Range:
                    Debug.Log($"Fire Range = {fireRange} + {rewardAmount}");
                    fireRange += rewardAmount;
                    break;
            }
        }

        #endregion


        #region PRIVATE METHODS

        // CALLING FROM ANIMATION EVENTS
        private void Shoot()
        {
            _currentShotType?.Invoke();
            _player.PushBackMagazines();
        }


        private void StandardShot()
        {
            SpawnProjectile(transform.rotation);
        }


        private void DoubleShot()
        {
            float[] angleOffsets = { 10f, -10f };

            for (int i = 0; i < angleOffsets.Length; i++)
            {
                var angle = angleOffsets[i];
                var rotation = Quaternion.Euler(0, angle, 0);

                SpawnProjectile(transform.rotation * rotation);
            }
        }


        private void TripleShot()
        {
            float[] angleOffsets = { 0f, 15f, -15f };

            for (int i = 0; i < angleOffsets.Length; i++)
            {
                var angle = angleOffsets[i];
                var rotation = Quaternion.Euler(0, angle, 0);

                SpawnProjectile(transform.rotation * rotation);
            }
        }


        private void SpawnProjectile(Quaternion rotation)
        {
            // Set projectile data
            var scale = Vector3.one * 2.5f;
            float firePower = _player.GetFirePower(FireRate);
            var projectileData = new ProjectileData(fireRange, firePower, scale);

            // Get projectile from pool
            var projectile = ObjectPooler.Instance.Spawn("Bullet", shootPoint.position, rotation);
            Debug.Log(projectileData.Range + "RangeFromShootHandler");
            projectile.GetComponent<ProjectileBase>().Init(projectileData);
            projectile.gameObject.name = $"Bullet_{transform.position}";

            VFXSpawner.Instance.PlayVFX("MuzzleEffect", shootPoint.position, shootPoint);
        }

        #endregion
    }
}