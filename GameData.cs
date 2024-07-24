using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    #region Levels
    public int level;
    public int exp;
    #endregion

    #region Stats
    public string name;
    public long lastUpdated;
    public int deathCount;
    public int highscore;
    #endregion

    /// <summary>
    /// This is used for saving the collected coins, achievements?, quests, cosmetics and power-ups
    /// The name "coinsCollected" is just a placeholder
    /// </summary>
    public SerializableDictionary<string, bool> coinsCollected;

    #region Settings
    public int graphics; // 1 - good, 0 - fast
    public int trailSetting; // 1 - on, 0 - off
    public int joystickPosSetting; // 1 - left, 0 - right

    public bool tempGraph;
    #endregion

    #region Cosmetics
    public int selectedColour;
    #endregion

    public GameData()
    {
        this.name = "";
        this.level = 0;
        this.exp = 0;
        this.deathCount = 0;
        this.highscore = 0;
        this.graphics = 1;
        this.tempGraph = true;
        this.trailSetting = 1;
        this.joystickPosSetting = 1;
        this.selectedColour = 0;
        coinsCollected = new SerializableDictionary<string, bool>();
    }
}
