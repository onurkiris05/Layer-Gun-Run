using Game.Interfaces;
using Game.Managers;
using UnityEngine;
using Zenject;

namespace Game.Player
{
    public class AnimationHandler : MonoBehaviour
    {
        [Inject] private IGameManager gameManager;

        private PlayerController _player;
        private Animator _animator;


        #region UNITY EVENTS

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }


        private void OnEnable()
        {
            gameManager.OnAfterStateChanged += UpdateAnimationState;
        }


        private void OnDisable()
        {
            gameManager.OnAfterStateChanged -= UpdateAnimationState;
            _player.OnFireRateUpdate -= UpdateAnimationSpeed;
        }

        #endregion


        #region PUBLIC METHODS

        public void Init(PlayerController player)
        {
            _player = player;
            _player.OnFireRateUpdate += UpdateAnimationSpeed;
        }

        #endregion


        #region PRIVATE METHODS

        private void UpdateAnimationState(GameState gameState)
        {
            _animator.SetBool("isShooting", gameState == GameState.Running);
        }


        private void UpdateAnimationSpeed(float fireRate)
        {
            _animator.SetFloat("fireRate", fireRate);
        }

        #endregion
    }
}