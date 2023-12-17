using DG.Tweening;
using Game.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Game.Incrementals
{
    public class IncrementalButton : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private ButtonType type;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI costText;

        [Inject] private IEconomyManager economyManager;
        
        public ButtonType Type => type;
        public int Cost => _cost;
        public float Amount => _amount;

        private Button _button;
        private int _level;
        private int _cost;
        private float _amount;

        #region UNITY EVENTS

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(TapEffect);
        }

        #endregion

        #region PUBLIC METHODS

        public void Set(int level, int cost, float amount)
        {
            _level = level;
            _cost = cost;
            _amount = amount;
            levelText.text = $"Level {_level}";
            costText.text = $"${_cost}";
        }

        public void CheckCost()
        {
            if (PlayerPrefs.GetInt("Coin")>= _cost)
            {
                _button.interactable = true;
                costText.color = Color.white;
            }
            else
            {
                _button.interactable = false;
                costText.color = Color.gray;
            }
        }

        #endregion

        #region PRIVATE METHODS

        private void TapEffect()
        {
            transform.DOComplete();
            transform.DOPunchScale(Vector3.one * 0.1f, 0.1f);
            transform.DOShakeRotation(0.1f, 10f);
        }

        #endregion
    }
}