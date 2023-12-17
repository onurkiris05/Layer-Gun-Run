using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Managers
{
    public class SceneController : MonoBehaviour
    {
        /*
        #region PUBLIC METHODS

        public void LoadNextScene()
        {
            var currentIndex = PlayerPrefs.GetInt("CurrentLevel", 0);
            LoadScene(currentIndex + 1);
        }

        public void LoadScene(int sceneIndex)
        {
            if (sceneIndex > SceneManager.sceneCount)
            {
                Debug.Log("Scene index out of range. Loading very last scene!");
                sceneIndex = SceneManager.sceneCount;
            }

            PlayerPrefs.SetInt("CurrentLevel", sceneIndex);
            SceneManager.LoadScene(sceneIndex);
        }

        public bool CheckIsSceneLoaded()
        {
            var currentIndex = PlayerPrefs.GetInt("CurrentLevel", 0);
            return SceneManager.GetActiveScene().buildIndex == currentIndex;
        }

        #endregion
        */
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.N))
            {
                LoadNextScene();
            }
        }
        int oldRandomLevel;
        private void Awake()
        {
            if (PlayerPrefs.GetInt("LevelPref") == 0)
            {
                PlayerPrefs.SetInt("LevelPref", 1);
            }
            if (PlayerPrefs.GetInt("LevelPref") != SceneManager.GetActiveScene().buildIndex)
            {
                SceneManager.LoadScene(PlayerPrefs.GetInt("LevelPref"));
            }
            Application.targetFrameRate = 60;
        }
        public void LoadNextScene()
        {
            if (PlayerPrefs.GetInt("LevelsFinished") == 0)
            {
                if (PlayerPrefs.GetInt("LevelPref") < SceneManager.sceneCountInBuildSettings - 1)
                {
                    PlayerPrefs.SetInt("LevelPref", PlayerPrefs.GetInt("LevelPref") + 1);
                }
                else
                {
                    PlayerPrefs.SetInt("LevelsFinished", 1);
                    List<int> levelList = new List<int>();
                    for (int i = 0; i < SceneManager.sceneCountInBuildSettings - 1; i++)
                    {
                        levelList.Add(i);
                    }
                    if (PlayerPrefs.GetInt("RandomizedLevels") == 0)
                    {
                        PlayerPrefs.SetInt("RandomizedLevels", 1);
                    }
                    levelList.Remove(oldRandomLevel);
                    int newLevel = levelList[Random.Range(1, levelList.Count)];
                    if (newLevel == 0)
                    {
                        newLevel = 1;
                    }
                    //oldRandomLevel = newLevel;
                    PlayerPrefs.SetInt("OldRandomLevel", newLevel);
                    PlayerPrefs.SetInt("LevelPref", newLevel);
                }
            }
            else
            {
                List<int> levelList = new List<int>();
                for (int i = 0; i < SceneManager.sceneCountInBuildSettings - 1; i++)
                {
                    levelList.Add(i);
                }
                if (PlayerPrefs.GetInt("RandomizedLevels") == 0)
                {
                    PlayerPrefs.SetInt("RandomizedLevels", 1);
                }
                levelList.Remove(PlayerPrefs.GetInt("OldRandomLevel"));
                int newLevel = levelList[Random.Range(1, levelList.Count)];
                PlayerPrefs.SetInt("OldRandomLevel", newLevel);
                if (newLevel == 0)
                {
                    newLevel = 1;
                }
                PlayerPrefs.SetInt("LevelPref", newLevel);
            }
            PlayerPrefs.SetInt("Level", PlayerPrefs.GetInt("Level") + 1);
            SceneManager.LoadScene(PlayerPrefs.GetInt("LevelPref"));
        }
    }
}