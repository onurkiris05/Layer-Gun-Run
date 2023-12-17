using NaughtyAttributes;
using UnityEngine;
using TMPro;
namespace Game.Collectables
{
    public class BulletRewardData : MonoBehaviour
    {
        [SerializeField] [ReadOnly] private int power;

        public int Power => power;

        public void SetPower(int power)
        {
            this.power = power;
            if (power > 9)
            {
                GetComponentInChildren<TextMeshPro>().text = power.ToString("0");
            }
        } 
    }
}