using System.Collections.Generic;
using Game.Incrementals;
using Game.Interfaces;
using UnityEngine;
using Zenject;

namespace Game.Managers
{
    public class IncrementalButtonManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private ButtonData[] buttonDatas;

        [Space] [Header("Components")]
        [SerializeField] private IncrementalButton[] buttons;

        [Inject] private IGameManager gameManager;

        private Dictionary<ButtonType, int> _buttonsToSave = new();
        private Dictionary<ButtonType, int> _maxedButtonsToSave = new();

        #region UNITY EVENTS

        private void OnEnable()
        {
//            gameManager.OnLevelCompleted += SaveButtons;
        }

        private void OnDisable()
        {
  //          gameManager.OnLevelCompleted -= SaveButtons;
        }

        private void Start() => InitializeButtons();

        #endregion

        #region PUBLIC METHODS

        public void UpgradeButton(IncrementalButton button)
        {
            var savedIndex = PlayerPrefs.GetInt($"{button.Type}Index", 0);
            var nextIndex = Mathf.Max(savedIndex + 1, _buttonsToSave[button.Type] + 1);

            // Check if button index exist. If not, keep increase level but keep same other values
            if (!IsIndexExists(nextIndex, button.Type))
            {
                var lastExceededLevel = PlayerPrefs.GetInt($"{button.Type}ExceededLevel", nextIndex);
                var exceededLevel = Mathf.Max(lastExceededLevel + 1, _maxedButtonsToSave[button.Type] + 1);
                nextIndex = Mathf.Max(savedIndex, _buttonsToSave[button.Type]);

                foreach (var data in buttonDatas)
                {
                    if (button.Type == data.Type)
                    {
                        button.Set(exceededLevel, data.Stats[nextIndex].Cost, data.Stats[nextIndex].Amount);
                        _maxedButtonsToSave[button.Type] = exceededLevel;
                        break;
                    }
                }
            }
            else
            {
                foreach (var data in buttonDatas)
                {
                    if (button.Type == data.Type)
                    {
                        button.Set(data.Stats[nextIndex].Level, data.Stats[nextIndex].Cost,
                            data.Stats[nextIndex].Amount);

                        _buttonsToSave[button.Type] = nextIndex;
                        break;
                    }
                }
            }

            CheckButtons();
        }

        #endregion

        #region PRIVATE METHODS

        private void InitializeButtons()
        {
            foreach (var button in buttons)
            {
                _buttonsToSave.Add(button.Type, 0);
                _maxedButtonsToSave.Add(button.Type, 0);

                foreach (var data in buttonDatas)
                {
                    if (button.Type == data.Type)
                    {
                        var exceededLevel = PlayerPrefs.GetInt($"{button.Type}ExceededLevel", 0);
                        var buttonIndex = PlayerPrefs.GetInt($"{button.Type}Index", 0);
                        var level = Mathf.Max(exceededLevel, data.Stats[buttonIndex].Level);

                        button.Set(level, data.Stats[buttonIndex].Cost, data.Stats[buttonIndex].Amount);
                        break;
                    }
                }
            }

            CheckButtons();
        }

        private void CheckButtons()
        {
            foreach (var button in buttons)
                button.CheckCost();
        }

        private bool IsIndexExists(int index, ButtonType type)
        {
            foreach (var data in buttonDatas)
            {
                if (type == data.Type)
                {
                    if (index < 0 || index >= data.Stats.Length)
                        return false;
                }
            }

            return true;
        }

        private void SaveButtons()
        {
            foreach (var button in _buttonsToSave)
            {
                var indexValue = Mathf.Max(button.Value, PlayerPrefs.GetInt($"{button.Key}Index", 0));
                PlayerPrefs.SetInt($"{button.Key}Index", indexValue);
            }

            foreach (var button in _maxedButtonsToSave)
            {
                var indexValue = Mathf.Max(button.Value, PlayerPrefs.GetInt($"{button.Key}ExceededLevel", 0));
                PlayerPrefs.SetInt($"{button.Key}ExceededLevel", indexValue);
            }
        }

        #endregion
    }
}