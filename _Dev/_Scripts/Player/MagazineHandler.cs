using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Game.Collectables;
using Game.Interfaces;
using Game.Projectiles;
using NaughtyAttributes;
using UnityEngine;
using Zenject;

namespace Game.Player
{
    public class MagazineHandler : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] [Range(1, 4)] private int startMagazineCount = 1;
        [SerializeField] private float startBulletPower;
        [SerializeField] private float magazinePushbackSpeed;
        [SerializeField] private float magazinePushbackEndValue;
        [SerializeField] private float magazinePushbackValueStep;
        [SerializeField] private float[] magazineExperienceToUpgrade;

        [Space] [Header("Components")]
        [SerializeField] private Magazine[] magazines;
        [SerializeField] private GameObject[] gunModels;

        [Space] [Header("Debug")]
        [SerializeField] [ReadOnly] private int activeMagazineIndex;
        [SerializeField] [ReadOnly] private int currentLevel;
        [SerializeField] [ReadOnly] private float currentMagazineExperience;

        [Inject] private IUIManager uiManager;
        [Inject] private ICameraManager cameraManager;

        private Pointer _pointer;
        private PlayerController _player;
        private List<Magazine> _activeMagazines = new();
        public static MagazineHandler instance;

        #region UNITY EVENTS

        private void Awake()
        {
            _pointer = GetComponentInChildren<Pointer>();
            instance = this;
        }
        private void Start()
        {
            if (PlayerPrefs.GetFloat("StartBulletPower") == 0)
            {
                PlayerPrefs.SetFloat("StartBulletPower", 1);
            }
            startBulletPower = PlayerPrefs.GetFloat("StartBulletPower");
            if (startBulletPower == 0)
            {
                startBulletPower = 1;
            }
            activeMagazineIndex = startMagazineCount - 1;
            currentLevel = startMagazineCount;
            InitMagazines();
        }

        #endregion

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.U))
            {
                PowersUpgraded();
            }
        }
        public void PowersUpgraded()
        {
            PlayerPrefs.SetFloat("StartBulletPower", PlayerPrefs.GetFloat("StartBulletPower") + .1f);
            for(int i = 0; i < _activeMagazines.Count; i++)
            {
                _activeMagazines[i].SetPowers(PlayerPrefs.GetFloat("StartBulletPower"));
            }
        }
        #region PUBLIC METHODS

        public void Init(PlayerController player) => _player = player;


        public void ProcessPushBackMagazines()
        {
            for (int i = 0; i < _activeMagazines.Count; i++)
            {
                var pushBackEndValue = magazinePushbackEndValue - magazinePushbackValueStep * i;

                DOTween.Complete($"{i}PushBack");
                _activeMagazines[i].transform.DOLocalMoveZ(-pushBackEndValue, magazinePushbackSpeed)
                    .SetEase(Ease.OutExpo).SetLoops(2, LoopType.Yoyo).SetSpeedBased().SetId($"{i}PushBack");
            }
        }


        public float ProcessFirePower(float fireRate)
        {
            float shotPower = 0;

            foreach (var bullet in _pointer.ActiveBullets)
            {
                shotPower += bullet.Value.Power;
                bullet.Value.ProcessFireBulletSequence(fireRate);
            }

            return shotPower;
        }


        public void ProcessMagazineExperience(float experience)
        {
            currentMagazineExperience += experience;

            for (int i = 0; i < magazineExperienceToUpgrade.Length; i++)
            {
                if (currentMagazineExperience >= magazineExperienceToUpgrade[i]
                    && i >= currentLevel - 1)
                {
                    _player.EnterUpgradeStage();

                    currentMagazineExperience -= magazineExperienceToUpgrade[i];
                    activeMagazineIndex++;
                    currentLevel = i + 2;

                    if (activeMagazineIndex >= magazines.Length)
                        MergeMagazines();
                    else
                        ActivateMagazines();

                    _pointer.SetSize(activeMagazineIndex);
                }
            }

            uiManager.SetMagazineBar(GetExperiencePercentage());
        }


        public void ProcessBulletReward(List<BulletRewardData> newBullets)
        {
            _pointer.SetActive(false);
            _player.EnterUpgradeStage();
            StartCoroutine(HandleNewBullets(newBullets));
        }

        #endregion


        #region PRIVATE METHODS

        private IEnumerator HandleNewBullets(List<BulletRewardData> newBullets)
        {
            Dictionary<Magazine, List<BulletProjectile>> currentBulletsDict = new();
            List<BulletProjectile> allBullets = new();
            List<BulletProjectile> bulletsToBeDiscarded = new();

            // Initialize dictionary and lists
            foreach (var magazine in _activeMagazines)
            {
                currentBulletsDict.Add(magazine, magazine.CurrentBullets);
                allBullets.AddRange(magazine.CurrentBullets);
            }

            // Sort bullets by power
            allBullets = allBullets.OrderBy(bullet => bullet.Power).ToList();
            newBullets = newBullets.OrderByDescending(bullet => bullet.Power).ToList();

            // Adjust bullets to be discarded
            for (int i = 0; i < newBullets.Count; i++)
            {
                for (int j = 0; j < allBullets.Count; j++)
                {
                    if (newBullets[i].Power >= allBullets[j].Power)
                    {
                        bulletsToBeDiscarded.Add(allBullets[j]);
                        allBullets.RemoveAt(j);
                        break;
                    }
                }
            }

            // Return early if there are no bullets to be discarded
            if (bulletsToBeDiscarded.Count <= 0)
            {
                Debug.Log("There are no bullets suitable to upgrade!");
                yield return null;
            }

            // Discard bullets
            for (int i = 0; i < bulletsToBeDiscarded.Count; i++)
            {
                foreach (var kvp in currentBulletsDict)
                {
                    if (kvp.Value.Contains(bulletsToBeDiscarded[i]))
                    {
                        kvp.Key.Swap(bulletsToBeDiscarded[i], newBullets[i]);
                        DestroyShowOffBullet(newBullets[i]);
                        yield return Helpers.BetterWaitForSeconds(0.2f);
                        break;
                    }
                }
            }

            yield return Helpers.BetterWaitForSeconds(0.5f);
            _pointer.SetActive(true);
            _player.ExitFromUpgradeStage();
        }


        private void DestroyShowOffBullet(BulletRewardData showOffBullet)
        {
            showOffBullet.transform.DOScale(new Vector3(0.2f, 0.2f, 0.2f), 0.4f).SetEase(Ease.Linear);
            showOffBullet.transform.DORotate(new Vector3(90f, 0f, 0f), 0.4f).SetEase(Ease.Linear);
            showOffBullet.transform.DOJump(magazines[0].transform.position, 1, 1, 0.4f)
                .SetEase(Ease.Linear).OnComplete(() => Destroy(showOffBullet.gameObject));
        }


        private void InitMagazines()
        {
            ResetMagazines();
            SetMagazines();
            _pointer.SetActive(true);
            _pointer.SetSize(activeMagazineIndex);
        }


        private void MergeMagazines()
        {
            float allBulletsPower = 0;
            float bulletPowerPerHolder = 0;

            // Calculate total bullet power
            foreach (var magazine in _activeMagazines)
            {
                foreach (var bullet in magazine.CurrentBullets)
                    allBulletsPower += bullet.Power;
            }

            // Divide total bullet power with holder count in first magazine
            bulletPowerPerHolder = allBulletsPower / 3;
            activeMagazineIndex = 0;

            _pointer.Reset();
            ResetMagazines();

            // Set first magazine with calculated bullet power
            for (int i = 0; i < activeMagazineIndex + 1; i++)
            {
                magazines[i].FillWithBullets(bulletPowerPerHolder);
                magazines[i].gameObject.SetActive(true);
                _activeMagazines.Add(magazines[i]);
            }

            SetGunModel();
            uiManager.SetGunUI(activeMagazineIndex);
        }


        private void ActivateMagazines()
        {
            _pointer.SetActive(false);

            for (int i = 0; i < activeMagazineIndex + 1; i++)
            {
                if (!_activeMagazines.Contains(magazines[i]))
                {
                    magazines[i].FillWithBullets(startBulletPower);
                    magazines[i].transform.localScale = Vector3.zero;
                    magazines[i].transform.localPosition = new Vector3(0f, 1f + (i * 1f), 0f);
                    magazines[i].gameObject.SetActive(true);
                    magazines[i].SetBullets();

                    // Process upgrade sequence animation
                    magazines[i].transform.DOKill();
                    magazines[i].transform.DOScale(Vector3.one, 0.75f).SetEase(Ease.InExpo);
                    magazines[i].transform.DOLocalMove(Vector3.zero, 0.75f).SetEase(Ease.InExpo)
                        .OnComplete(() =>
                        {
                            SetGunModel();
                            _player.ExitFromUpgradeStage();
                            _pointer.SetActive(true);

                            // Process effects for upgrade sequence
                            var vfxPos = new Vector3(transform.position.x, transform.position.y + (0.4f * i),
                                transform.position.z - 1f);
                            VFXSpawner.Instance.PlayVFX("MagazineUpgrade", vfxPos, transform);
                            cameraManager.ShakeCamera(1f, 1f, 0.3f);
                        });

                    _activeMagazines.Add(magazines[i]);
                }
            }

            uiManager.SetGunUI(activeMagazineIndex);
        }


        private void SetMagazines()
        {
            for (int i = 0; i < activeMagazineIndex + 1; i++)
            {
                if (!_activeMagazines.Contains(magazines[i]))
                {
                    magazines[i].FillWithBullets(startBulletPower);
                    magazines[i].gameObject.SetActive(true);
                    _activeMagazines.Add(magazines[i]);
                }
            }

            SetGunModel();
            uiManager.SetGunUI(activeMagazineIndex);
        }


        private void ResetMagazines()
        {
            foreach (var magazine in _activeMagazines)
                magazine.ClearBullets();

            foreach (var magazine in magazines)
                magazine.gameObject.SetActive(false);

            _activeMagazines.Clear();
        }


        private void SetGunModel()
        {
            for (int i = 0; i < gunModels.Length; i++)
                gunModels[i].SetActive(activeMagazineIndex >= i);
        }


        private float GetExperiencePercentage()
        {
            return currentMagazineExperience / magazineExperienceToUpgrade[activeMagazineIndex];
        }

        #endregion
    }
}