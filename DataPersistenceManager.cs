using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections;

public class DataPersistenceManager : MonoBehaviour {

    [Header("File Storage Config")]
    [SerializeField] private string fileName;
    [SerializeField] private bool useEncryption = true;

    [Header("AutoSave config")]
    [SerializeField] private float autoSaveTimeSeconds = 60f;

    private GameData gameData;
    private List<IDataPersistence> dataPersistencesObjects;
    private FileDataHandler dataHandler;

    // private string selectedProfileId = "";

    private Coroutine autosave;

    public static DataPersistenceManager instance {get; private set;}

    private void Awake() {
        if(instance != null) {
            Debug.Log("Found more than one Data Persistence Manager in the scene. Destroying the newest one.");
            Destroy(this.gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this.gameObject);

        this.dataHandler = new FileDataHandler(Application.persistentDataPath, fileName, useEncryption);

        // this.selectedProfileId = dataHandler.GetMostRecentlyUpdatedProfileId();
    }

    private void OnEnable() {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        this.dataPersistencesObjects = FindAllDataPersistenceObjects();
        LoadGame();

        // start up the auto save coroutine
        if(autosave != null) {
            StopCoroutine(autosave);
        }
        autosave = StartCoroutine(AutoSave());
    }

    // public void ChangeSelectedProfileId(string newProfileId) {
    //     // update the profile to use for saving and loading
    //     this.selectedProfileId = newProfileId;

    //     // Load the game, which will use that profile, updating our game data accordingly
    //     LoadGame();
    // }

    // public void DeleteProfileData(string profileId) {
    //     // delete the data for this profile id
    //     dataHandler.Delete(profileId);

    //     // initialize the selected profile id
    //     // InitializeSelectedProfileId();

    //     // reload the game so that our data matches the newly selected profile id
    //     LoadGame();
    // }

    // private void InitializeSelectedProfileId() {

    // }

    public void NewGame() {
        this.gameData = new GameData();
    }

    public void LoadGame() {
        // Load any saved data from a file using the data handler
        this.gameData = dataHandler.Load();

        // if no data ca be loaded, don't continue
        if(this.gameData == null) {
            Debug.Log("No data was found. A New Game needs to be started before data can be loaded.");
            NewGame();
            return;
        }

        // push the loaded data to all other scripts that need it
        foreach(IDataPersistence dataPersistenceObj in dataPersistencesObjects) {
            dataPersistenceObj.LoadData(gameData);
        }
    }

    public void SaveGame() {
        // if we don't have any data to save, log a warning here
        if(this.gameData == null) {
            Debug.LogWarning("No data was found. A New Game needs to be started before data can be loaded.");
            return;
        }
        // pass the data to other scripts so they can update it
        foreach(IDataPersistence dataPersistenceObj in dataPersistencesObjects) {
            dataPersistenceObj.SaveData(gameData);
        }

        // timestamp the data so we know when it was last updated
        gameData.lastUpdated = System.DateTime.Now.ToBinary();

        // save that data to a file using the data handler
        dataHandler.Save(gameData);
    }

    private void OnApplicationQuit() {
        SaveGame();
    }

    private List<IDataPersistence> FindAllDataPersistenceObjects() {
        IEnumerable<IDataPersistence> dataPersistences = FindObjectsOfType<MonoBehaviour>(true).OfType<IDataPersistence>();

        return new List<IDataPersistence>(dataPersistences);
    }

    private IEnumerator AutoSave() {
        while(true) {
            yield return new WaitForSeconds(autoSaveTimeSeconds);
            SaveGame();
            Debug.Log("Saving game...");
        }
    }
}