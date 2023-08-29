using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Transform LevelPrefab, LevelFolder;
    void Start()
    {
        if (!PlayerPrefs.HasKey("CurrentLevel"))
        {
            PlayerPrefs.SetInt("CurrentLevel", 1);
        }

        for (int i = 0; i < 50; i++)
        {
            Transform prefabInstantiate = Instantiate(LevelPrefab, LevelFolder);
            prefabInstantiate.GetChild(0).GetComponent<TextMeshProUGUI>().text = i + 1 + "";
            prefabInstantiate.GetChild(1).gameObject.SetActive(i>=PlayerPrefs.GetInt("CurrentLevel"));

        }

    }
    
    public void PlayGame() {SceneManager.LoadScene(1);}
}
