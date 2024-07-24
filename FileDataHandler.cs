using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FileDataHandler
{
    private string dataDirPath = "";
    private string dataFileName = "";
    private bool useEncryption = true;
    private readonly string encryptionCodeWord = "СЛАВА РОССИЙ!";
    private readonly string backupExt = ".bak";
    
    public FileDataHandler(string dataDirPath, string dataFileName, bool useEncryption) {
        this.dataDirPath = dataDirPath;
        this.dataFileName = dataFileName;
        this.useEncryption = useEncryption;
    }

    //! Sudden changes... Removeing profiles from the game
    //! with a single default profile

    public GameData Load(bool allowRestoreFromBackup = true) {
        // use Path.Combine to account for different OS's having different path separators
        string fullPath = Path.Combine(dataDirPath, dataFileName);
        GameData loadedData = null;
        if(File.Exists(fullPath)) {
            try {
                // Load the serialized data from file
                string dataToLoad = "";
                using(FileStream stream = new FileStream(fullPath, FileMode.Open)) {
                    using (StreamReader reader = new StreamReader(stream)) {
                        dataToLoad = reader.ReadToEnd();
                    }
                }

                // optionally decrypt the data
                if(useEncryption) 
                    dataToLoad = EncryptDecrypt(dataToLoad);

                // deserialize the data form Json back into the C# object
                loadedData = JsonUtility.FromJson<GameData>(dataToLoad);
            } catch(Exception e) {
                // since we're calling Load(...) recursively, we need tp account for the case where
                // the rollback succeeds, but data is still failing to load for some other reason,
                // which without this check may cause an infinite recursion loop.
                if(allowRestoreFromBackup) {
                    Debug.LogWarning("Failed to load data file. Attempting to roll back.\n" + e);
                    bool rollbackSuccess = AttemptRollback(fullPath);
                    if(rollbackSuccess) {
                        // try to load again recursively
                        loadedData = Load(false);
                    }
                } else {
                    // if we hit the else block, one possibility is that the backuo file is also corrupt
                    Debug.LogError("Error occured when trying to load file at pach: " + fullPath + " and bacup did not work.\n" + e);
                }
            }
        }
        return loadedData;
    }

    public void Save(GameData data) {

        // use Path.Combine to account for different OS's having different path separators
        string fullPath = Path.Combine(dataDirPath, dataFileName);
        string backupFilePath = fullPath + backupExt;

        try {
            // create the directory the file will be written to if it doesn't exist
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            // serialize the C# game data object to Json
            string dataToStore = JsonUtility.ToJson(data, true);

            // optionally encrypt the data
            if(useEncryption) 
                dataToStore = EncryptDecrypt(dataToStore);

            using(FileStream stream = new FileStream(fullPath, FileMode.Create)) {
                using(StreamWriter writer = new StreamWriter(stream)) {
                    writer.Write(dataToStore);
                }
            }

            // verify the newly saved file can be loaded successfully
            GameData verifiedGameData = Load();
            // if the data can be verified, back it up
            if(verifiedGameData != null) {
                File.Copy(fullPath, backupFilePath, true);
            } else {
                // otherwise, something went wrong and we should throw an exception
                throw new Exception("Save file could not be verified and backup could not be created");
            }
        } catch (Exception e) {
            Debug.LogError("Error occured when trying to save data to file: " + fullPath + "\n" + e);
        }
    }

    public void Delete(string profileId) {
        // base case - if the profile id is null, return right away
        if(profileId == null) return;

        string fullPath = Path.Combine(dataDirPath, profileId, dataFileName);
        try {
            // ensure the data file exists at this path before deleting the directory
            if(File.Exists(fullPath)) {
                // delete the profile folder and everything within it
                Directory.Delete(Path.GetDirectoryName(fullPath), true);
            } else {
                Debug.LogWarning("Tried to delete profile data, but data was not found at path: " + fullPath);
            }
        } catch(Exception e) {
            Debug.LogError("Failed to delete profile data for profileId: " + profileId + " at path: " + fullPath + "\n" + e);
        }
    }

    // the below is a simple implementation of XOR encryption
    private string EncryptDecrypt(string data) {
        string modifiedData = "";
        for(int i = 0; i < data.Length; i++) {
            modifiedData += (char)(data[i] ^ encryptionCodeWord[i % encryptionCodeWord.Length]);
        }
        return modifiedData;
    }

    private bool AttemptRollback(string fullPath) {
        bool success = false;
        string backupFilePath = fullPath + backupExt;
        try {
            // if the file exists, attempt to roll back to it by overwriting the original file
            if(File.Exists(backupFilePath)) {
                File.Copy(backupFilePath, fullPath, true);
                success = true;
                Debug.LogWarning("Had to roll back to backup file at: " + backupFilePath);
            } else {
                // otherwise, we don't yet have a backup file - so there's nothing to roll back to
                throw new Exception("Tried to roll back, but no backup file exists to roll back to.");
            }
        } catch(Exception e) {
            Debug.LogError("Error occured when trying to roll back backup file at: " + backupFilePath + "\n" + e);
        }

        return success;
    }
}
