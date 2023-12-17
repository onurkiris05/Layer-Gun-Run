using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using ElephantSDK;
using Game;
using Game.Collectables;
using Game.Gates;
using Game.Incrementals;
using Game.Interfaces;
using Game.Projectiles;
using Game.Traps;
using Game.Player;



[System.Serializable]
public class UpgradeClass
{
    public string upgradeName;
    public Image upgradeImage;
    public List<Sprite> sprites = new List<Sprite>();
    public TextMeshProUGUI upgradeLevelText;
    public TextMeshProUGUI upgradePriceText;
    public List<int> upgradePrices = new List<int>();
}

[System.Serializable]
public class PriceList
{
    public List<int> yearPrices = new List<int>();
    public List<int> rangePrices = new List<int>();
}
public class UpgradeManager : MonoBehaviour
{
    [SerializeField]private List<UpgradeClass> upgrades = new List<UpgradeClass>();
    [SerializeField]private TextAsset defaultUpgradePrices;
    public PriceList up;
    private void Awake()
    {
        if (PlayerPrefs.GetFloat("UpgradeMultiplier") == 0)
        {
            PlayerPrefs.SetFloat("UpgradeMultiplier", 1);
        }
        string remoteText = RemoteConfig.GetInstance().Get("UpgradePrices", defaultUpgradePrices.text);
        string priceJson = remoteText;
        PriceList pl = new PriceList();
        pl = JsonUtility.FromJson<PriceList>(priceJson);
        up = pl;
        for (int i = 0; i < upgrades.Count; i++)
        {
            upgrades[i].upgradePrices.Clear();
            if (i == 0)
            {
                for (int b = 0; b < pl.yearPrices.Count; b++)
                {
                    upgrades[i].upgradePrices.Add(pl.yearPrices[b]);
                }
            }
            else
            {
                for (int b = 0; b < pl.rangePrices.Count; b++)
                {
                    upgrades[i].upgradePrices.Add(pl.rangePrices[b]);
                }
            }
        }





        RefreshUI();
        for(int i = 0; i < upgrades.Count; i++)
        {
            GameObject upgradeImager = upgrades[i].upgradeImage.gameObject;
            upgradeImager.transform.DOScale(Vector3.one * 1.1f, .2f);
            upgradeImager.transform.DOScale(Vector3.one , .2f).SetDelay(.2f);
        }
    }
    public void RefreshUI()
    {
        for(int i = 0; i < upgrades.Count; i++)
        {
            RefreshUpgrade(i);
        }
    }
    public void RefreshUpgrade(int upgradeNumber)
    {
        UpgradeClass uc = upgrades[upgradeNumber];
        int price = 0;
        if (PlayerPrefs.GetInt(uc.upgradeName+"Level") < uc.upgradePrices.Count)
        {
            price = uc.upgradePrices[PlayerPrefs.GetInt(uc.upgradeName+"Level")];
        }
        else
        {
            price = uc.upgradePrices[uc.upgradePrices.Count - 1];
        }
        if (PlayerPrefs.GetInt("Coin") >= price)
        {
            uc.upgradeImage.sprite = uc.sprites[0];
        }
        else
        {
            uc.upgradeImage.sprite = uc.sprites[1];
        }
        uc.upgradeLevelText.text = "lvl " + (PlayerPrefs.GetInt(uc.upgradeName+"Level") + 1).ToString();
        uc.upgradePriceText.text = price.ToString() + "$";
    }
    public void PurchaseUpgrade(int upgradeNum)
    {
        UpgradeClass uc = upgrades[upgradeNum];
        int price = 0;
        if (PlayerPrefs.GetInt(uc.upgradeName+"Level") < uc.upgradePrices.Count)
        {
            price = uc.upgradePrices[PlayerPrefs.GetInt(uc.upgradeName+"Level")];
        }
        else
        {
            price = uc.upgradePrices[uc.upgradePrices.Count - 1];
        }

        if (PlayerPrefs.GetInt("Coin") >= price)
        {
            PlayerPrefs.SetInt("Coin", PlayerPrefs.GetInt("Coin") - price);
            AlpGameManager.instance.RefreshCoinText();
            PlayerPrefs.SetInt(uc.upgradeName + "Level", PlayerPrefs.GetInt(uc.upgradeName + "Level") + 1);
            StartCoroutine(ShakeMainBar(uc.upgradeImage.gameObject));
            RefreshUI();
            switch (upgradeNum)
            {
                case 0:
                    MagazineHandler.instance.PowersUpgraded();
                    break;
                case 1:
                    ShootHandler.instance.RateUpgrade();
                    break;
                case 3:
                    PlayerPrefs.SetFloat("UpgradeMultiplier", PlayerPrefs.GetFloat("UpgradeMultiplier") + .1f);
                    break;
                case 4:
                    ShootHandler.instance.RangeUpgrade();
                    break;
            }
        }
    }
    private IEnumerator ShakeMainBar(GameObject mainBar)
    {
        yield return new WaitForSeconds(0);
        Vector3 startScale = Vector3.one;
        Vector3 startEuler = Vector3.zero;
        mainBar.transform.DOScale(startScale * 1.1f, .15f);
        mainBar.transform.DOLocalRotate(startEuler + new Vector3(0, 0, -5), .1f);
        yield return new WaitForSeconds(.1f);
        mainBar.transform.DOLocalRotate(startEuler + new Vector3(0, 0, 5), .05f);
        yield return new WaitForSeconds(.05f);
        mainBar.transform.DOLocalRotate(startEuler + new Vector3(0, 0, 0), .05f);
        yield return new WaitForSeconds(.05f);
        mainBar.transform.DOScale(startScale, .1f);
    }
}
