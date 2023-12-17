using UnityEngine;

namespace Game.Interfaces
{
    public interface IUIManager
    {
        void SetWalletUI(int value, bool withEffect = false);
        void SetMagazineBar(float value);
        void SetGunUI(int weaponIndex);
        RectTransform WalletTextUI { get; }
        RectTransform MagazineBarUI { get; }
    }
}