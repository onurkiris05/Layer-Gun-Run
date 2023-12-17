using Game.Interfaces;
using UnityEngine;
using Zenject;

namespace Game.Managers
{
    public class EconomyManager : MonoBehaviour, IEconomyManager
    {
        [Header("Settings")]
        [SerializeField] private int currentWallet;

        [Inject] private IGameManager gameManager;
        [Inject] private IUIManager uiManager;
        public void AddMoney(int amount)
        {
            PlayerPrefs.SetInt("Coin", PlayerPrefs.GetInt("Coin") + amount);
        }
    }
}