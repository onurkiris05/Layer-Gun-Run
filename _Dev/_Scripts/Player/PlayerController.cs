using System;
using System.Collections.Generic;
using DG.Tweening;
using Game.Collectables;
using Game.Gates;
using Game.Interfaces;
using Game.Managers;
using UnityEngine;
using Zenject;

namespace Game.Player
{
    public class PlayerController : MonoBehaviour
    {
        [Inject] private IGameManager gameManager;

        public Action<float> OnFireRateUpdate;

        public bool IsTrapped => _isTrapped;

        private MovementHandler _movementHandler;
        private ShootHandler _shootHandler;
        private AnimationHandler _animationHandler;
        private MagazineHandler _magazineHandler;
        private bool _isTrapped;


        #region UNITY EVENTS

        private void Awake()
        {
            _movementHandler = GetComponent<MovementHandler>();
            _shootHandler = GetComponent<ShootHandler>();
            _animationHandler = GetComponent<AnimationHandler>();
            _magazineHandler = GetComponent<MagazineHandler>();

            _shootHandler.Init(this);
            _movementHandler.Init(this);
            _animationHandler.Init(this);
            _magazineHandler.Init(this);
        }

        #endregion


        #region PUBLIC METHODS

        public float GetFirePower(float fireRate) => _magazineHandler.ProcessFirePower(fireRate);

        public void OnMagazineReward(float experience) => _magazineHandler.ProcessMagazineExperience(experience);

        public void OnGateReward(GateType gateType, float amount) => _shootHandler.ProcessGateReward(gateType, amount);

        public void OnBulletReward(List<BulletRewardData> bullets) => _magazineHandler.ProcessBulletReward(bullets);

        public void PushBackMagazines() => _magazineHandler.ProcessPushBackMagazines();

        public void ExitFromUpgradeStage() => gameManager.ChangeState(GameState.Running);

        public void EnterUpgradeStage() => gameManager.ChangeState(GameState.Upgrade);

        public void PushBack(float pushBackDistance, float pushBackDuration)
        {
            _isTrapped = true;
            transform.DOMoveZ(transform.position.z - pushBackDistance, pushBackDuration)
                .OnComplete(() => { _isTrapped = false; });
        }

        #endregion
    }
}