using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Game.Interfaces;
using Game.Player;
using Game.Projectiles;
using NaughtyAttributes;
using UnityEngine;
using Zenject;

namespace Game.Collectables
{
    public class BulletCollectable : BaseCollectable
    {
        [Header("Settings")]
        [SerializeField] [Range(1, 3)] private int bulletCount;
        [SerializeField] private float bulletSwapSpeed;
        [SerializeField] private float pointerSpeed;
        [SerializeField] private float pointerReturnSpeed;
        [SerializeField] private int[] wheelHealths;

        [Space] [Header("Components")]
        [SerializeField] private Transform pointer;
        [SerializeField] private Transform showOffPlace;
        [SerializeField] private BulletRewardData[] bulletPrefabs;

        [Space] [Header("Debug")]
        [SerializeField] [ReadOnly] private float currentDamageTaken;
        [SerializeField] [ReadOnly] private int currentPieceIndex;
        [SerializeField] [ReadOnly] private float anglePerPiece;

        [Inject] private IConveyorManager conveyorManager;

        private Dictionary<int, List<BulletRewardData>> _bullets = new();
        private List<BulletRewardData> _activeBullets = new();
        private bool _isKilled;

        private List<Vector3> bulletPositions = new List<Vector3>();
        private List<Vector3> bulletRotations = new List<Vector3>();

        private List<GameObject> bulletsInstd= new List<GameObject>();
        public int startBulletPower;
        #region UNITY EVENTS

        private void Start() => Init();


        protected override void OnTriggerEnter(Collider other)
        {
            if (_isKilled) return;

            if (other.TryGetComponent(out BulletProjectile bullet))
            {
                VFXSpawner.Instance.PlayVFX("BulletHit", bullet.transform.position);
                currentDamageTaken += bullet.Power;
                bullet.Kill();

                for (int i = 0; i < wheelHealths.Length; i++)
                {
                    if (currentDamageTaken >= wheelHealths[currentPieceIndex])
                    {
                        currentDamageTaken -= wheelHealths[currentPieceIndex];
                        currentPieceIndex = Mathf.Min(currentPieceIndex + 1, wheelHealths.Length - 1);

                        SetBullets();
                    }
                }

                RotatePointerToTarget();
            }
            else if (other.TryGetComponent(out PlayerController player))
            {
                StartCoroutine(SendBulletsToGate());
                KillCollectable();
            }
        }

        #endregion


        #region PRIVATE METHODS

        private void Init()
        {
            anglePerPiece = 180f / wheelHealths.Length;
            pointer.transform.localRotation = Quaternion.Euler(0, 0, 0);
            CreateBullets();
        }


        private IEnumerator SendBulletsToGate()
        {
            foreach (var bullet in _activeBullets)
            {
                bullet.transform.DOComplete();
                bullet.transform.parent = null;
                conveyorManager.SendBulletToUpgradeGate(bullet);
                yield return Helpers.BetterWaitForSeconds(0.1f);
            }
        }


        private void RotatePointerToTarget()
        {
            var targetAngle = GetTargetAngle();

            pointer.DOKill();
            pointer.DOLocalRotate(new Vector3(0, 0, targetAngle), pointerSpeed).SetEase(Ease.OutSine)
                .SetSpeedBased().OnComplete(RotatePointerToStart);
        }


        private void RotatePointerToStart()
        {
            pointer.DOLocalRotate(new Vector3(0, 0, 0), pointerReturnSpeed).SetEase(Ease.InSine)
                .SetSpeedBased().OnUpdate(() => { UpdateHealthFromAngle(pointer.localRotation.eulerAngles.z); });
        }


        private float GetTargetAngle()
        {
            var damageRatio = (float)currentDamageTaken / wheelHealths[currentPieceIndex];
            var targetAngle = currentPieceIndex * anglePerPiece + (anglePerPiece * damageRatio);
            return targetAngle;
        }


        private void UpdateHealthFromAngle(float angle)
        {
            if (_isKilled) return;

            var pieceIndex = Mathf.FloorToInt(angle / anglePerPiece);
            var damageRatio = (angle - (pieceIndex * anglePerPiece)) / anglePerPiece;

            currentDamageTaken = Mathf.CeilToInt(damageRatio * wheelHealths[pieceIndex]);

            // If piece index changed, set bullets
            if (pieceIndex != currentPieceIndex)
            {
                currentPieceIndex = pieceIndex;
                SetBullets();
            }
        }


        private void CreateBullets()
        {
            var spacing = 0.75f;

            for (int i = 0; i < bulletPrefabs.Length; i++)
            {
                List<BulletRewardData> newBullets = new();

                for (int j = 0; j < bulletCount; j++)
                {
                    var totalWidth = (bulletCount - 1) * spacing;
                    var offset = -totalWidth / 2f + j * spacing;
                    var spawnPosition = new Vector3(showOffPlace.position.x + offset,
                        showOffPlace.position.y, showOffPlace.position.z);

                    BulletRewardData bullet = null;
                    if (i + startBulletPower < bulletPrefabs.Length - 2)
                    {
                        bullet = Instantiate(bulletPrefabs[i + startBulletPower], spawnPosition, Quaternion.identity, transform);
                    }
                    else
                    {
                        bullet = Instantiate(bulletPrefabs[^1], spawnPosition, Quaternion.identity, transform);
                    }
                    bullet.SetPower(i + 1 + startBulletPower);
                    bullet.gameObject.SetActive(i == currentPieceIndex);
                    newBullets.Add(bullet);
                    bulletsInstd.Add(bullet.gameObject);
                    bulletPositions.Add(bullet.transform.position);
                    bulletRotations.Add(bullet.transform.eulerAngles);
                }
                
                _bullets.Add(i, newBullets);
            }

            _activeBullets = _bullets[currentPieceIndex];
        }


        private void SetBullets()
        {
            _activeBullets = _bullets[currentPieceIndex];

            DeactivateOldBullets();
            ActivateNewBullets();
        }


        private void DeactivateOldBullets()
        {
            foreach (var kvp in _bullets)
            {
                foreach (var bullet in kvp.Value)
                {
                    if (bullet.gameObject.activeSelf && kvp.Key != currentPieceIndex)
                    {
                        bullet.transform.DOKill();
                        /*
                        bullet.transform.DOScale(Vector3.zero, bulletSwapSpeed * 2f).SetEase(Ease.Linear)
                            .SetSpeedBased().OnComplete(() => { bullet.gameObject.SetActive(false); });
                        */
                        bullet.transform.DOJump(bullet.transform.position + new Vector3(0, -1, 3), 3, 1, .6f);
                        bullet.transform.DOScale(Vector3.zero, .2f).SetDelay(.4f).OnComplete(() => { bullet.gameObject.SetActive(false); }); ;
                        bullet.transform.DORotate(bullet.transform.eulerAngles + new Vector3(90, 0, Random.Range(-50, 50)), .3f);
                    }
                }
            }
        }


        private void ActivateNewBullets()
        {
            foreach (var bullet in _activeBullets)
            {
                bullet.gameObject.SetActive(true);
                bullet.transform.DOKill();
                bullet.transform.localScale = Vector3.zero;
                bullet.transform.DOScale(Vector3.one, bulletSwapSpeed).SetEase(Ease.Linear).SetSpeedBased();
                int bulletIndex = bulletsInstd.IndexOf(bullet.gameObject);
                bullet.transform.position = bulletPositions[bulletIndex];
                bullet.transform.eulerAngles= bulletRotations[bulletIndex];
            }
        }


        private void KillCollectable()
        {
            _isKilled = true;
            transform.DOComplete();
            transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InElastic)
                .OnComplete(() => Destroy(gameObject));
        }

        #endregion
    }
}