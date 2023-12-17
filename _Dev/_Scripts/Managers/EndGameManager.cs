using System.Linq;
using Game.Collectables;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;

namespace Game.Managers
{
    public class EndGameManager : MonoBehaviour
    {
        [Header("Placement Settings")]
        [SerializeField] private int rowCount;
        [SerializeField] private float rowSpacing;
        [SerializeField] private int columnCount;

        [Space] [Header("Hit Settings")]
        [SerializeField] private int firstRowHitCount;
        [SerializeField] private float hitCountScaler;

        [Space] [Header("Reward Settings")]
        [SerializeField] private int firstRowMoneyReward;
        [SerializeField] private float moneyRewardScaler;

        [Space] [Header("Components")]
        [SerializeField] private EndGameBarrelCollectable endGameBarrelCollectablePrefab;
        [SerializeField] private Transform platformMin;
        [SerializeField] private Transform platformMax;

        private int _currentHitCount;
        private int _currentMoneyReward;


        #region EDITOR METHODS

#if UNITY_EDITOR

        [Button]
        private void CreateBarrels()
        {
            DeleteBarrels();
            
            _currentHitCount = firstRowHitCount;
            _currentMoneyReward = firstRowMoneyReward;

            var platformWidth = platformMax.position.x - platformMin.position.x;
            var columnSpacing = platformWidth / columnCount;
            var startPosition = new Vector3(platformMin.position.x + (columnSpacing / 2f),
                platformMin.position.y, platformMin.position.z);


            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < columnCount; j++)
                {
                    var barrel = (EndGameBarrelCollectable)PrefabUtility.InstantiatePrefab(endGameBarrelCollectablePrefab, transform);
                    barrel.transform.position = startPosition + new Vector3(j * columnSpacing, 0f, i * rowSpacing);
                    barrel.Set(_currentHitCount, _currentMoneyReward);
                }

                // Always set to nearest value which is multiple of ten
                _currentHitCount = (int)(Mathf.Ceil(_currentHitCount * hitCountScaler / 10f) * 10f);
                _currentMoneyReward = (int)(Mathf.Ceil(_currentMoneyReward * moneyRewardScaler / 10f) * 10f);
            }
        }

        [Button]
        private void DeleteBarrels()
        {
            var endGameBarrels = FindObjectsOfType<EndGameBarrelCollectable>().ToList();

            foreach (var barrel in endGameBarrels)
                DestroyImmediate(barrel.gameObject);
        }
#endif

        #endregion
    }
}