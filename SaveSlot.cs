using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveSlot : MonoBehaviour, IDataPersistence
{
    [Header("Content")]
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI highsoreText;
    [SerializeField] private TextMeshProUGUI deathsText;

    public void LoadData(GameData data)
    {
        levelText.text = "Level: " + data.level;
        highsoreText.text = "Highscore: " + data.highscore;
        deathsText.text = "Deaths: " + data.deathCount;
    }

    public void SaveData(GameData data)
    {
        // No implementation needed lol
    }
}
