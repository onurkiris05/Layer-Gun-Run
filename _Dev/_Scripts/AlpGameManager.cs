using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ElephantSDK;
using Game.Player;
public class AlpGameManager : MonoBehaviour
{
    
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI coinText;
    public static AlpGameManager instance;
    public bool finished;
    public Transform levelBarWorldPositionObject;
    private void Awake()
    {
        instance = this;
        levelBarWorldPositionObject = FindObjectOfType<CanvasFollower>().gameObject.transform;
        Application.targetFrameRate = 60;
        if (PlayerPrefs.GetInt("Level") == 0)
        {
            PlayerPrefs.SetInt("Level", 1);
        }
        levelText.text = "Level "+PlayerPrefs.GetInt("Level").ToString();
        coinText.text = PlayerPrefs.GetInt("Coin").ToString();
    }
    public void RefreshCoinText()
    {
        coinText.text = PlayerPrefs.GetInt("Coin").ToString();
    }
}
