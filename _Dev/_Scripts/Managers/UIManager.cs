using DG.Tweening;
using Game.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using System.Collections;
using System.Collections.Generic;
namespace Game.Managers
{
    public class UIManager : MonoBehaviour, IUIManager
    {
        [Header("Canvases")]
        [SerializeField] private GameObject canvasStartGame;
        [SerializeField] private GameObject canvasGameUI;
        [SerializeField] private GameObject canvasGameEnd;

        [Header("Components")]
        [SerializeField] private TextMeshProUGUI walletText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private Slider magazineBar;
        [SerializeField] private Image barTopUIImage;
        [SerializeField] private Image barBottomUIImage;
        [SerializeField] private Sprite[] gunSprites;

        [Inject] private IGameManager gameManager;

        public RectTransform WalletTextUI => walletText.rectTransform;
        public RectTransform MagazineBarUI => magazineBar.handleRect;
        public ParticleSystem confetti;

        #region UNITY EVENTS

        private void OnEnable()
        {
            gameManager.OnAfterStateChanged += AdjustCanvases;
        }


        private void OnDisable()
        {
          gameManager.OnAfterStateChanged -= AdjustCanvases;
        }


        private void Start()
        {
            AdjustCanvases(gameManager.State);
            SetLevelUI();
        }

        #endregion


        #region PUBLIC METHODS

        public void SetWalletUI(int value, bool withEffect = false)
        {
            walletText.text = $"${value}";

            if (withEffect)
            {
                walletText.rectTransform.DOComplete();
                walletText.rectTransform.DOScale(Vector3.one * 1.3f, 0.3f).From();
            }
        }


        public void SetMagazineBar(float value)
        {
            DOTween.Complete(this);
            DOTween.To(x => magazineBar.value = x, magazineBar.value, value, 0.3f)
                .SetEase(Ease.Linear).SetId(this);
            magazineBar.transform.DOShakeScale(0.3f, 0.8f).SetId(this);
        }


        public void SetGunUI(int weaponIndex)
        {
            barBottomUIImage.sprite = gunSprites[weaponIndex % gunSprites.Length];
            barTopUIImage.sprite = gunSprites[(weaponIndex + 1) % gunSprites.Length];
        }

        #endregion


        #region PRIVATE METHODS

        private void SetLevelUI()
        {
            levelText.text = $"Level {gameManager.GetLevel()}";
        }

        private void AdjustCanvases(GameState state)
        {
            canvasStartGame.SetActive(state == GameState.Start);
            canvasGameUI.SetActive(state != GameState.End);
            if(state == GameState.End)
            {
                confetti.Play();
                StartCoroutine(SummonCanvas());
            }
            
        }
        private IEnumerator SummonCanvas()
        {
            yield return new WaitForSeconds(1);
            canvasGameEnd.transform.localScale = Vector3.zero;
            canvasGameEnd.transform.DOScale(Vector3.one, .2f);
            canvasGameEnd.SetActive(true);

        }


        #endregion
    }
}