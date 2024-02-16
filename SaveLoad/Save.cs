using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using System.Collections.Generic;
using System.Runtime.Serialization;

public class Save : MonoBehaviour
{

    //SAVING DATA
    public void SavePlayer(List<ImageClass> imageClassListObj)
    {
        string path = Application.persistentDataPath + "/playerSave.txt";
        FileStream file = new FileStream(path, FileMode.OpenOrCreate);
        try
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(file, imageClassListObj);
            file.Close();
        }
        catch (SerializationException e)
        {
            Debug.LogError("There was an issue serializing this data: " + e.Message);
        }
        finally
        {
            file.Close();
        }
    }
    //LOADING DATA
    public List<ImageClass> LoadPlayer()
    {
        string path = Application.persistentDataPath + "/playerSave.txt";
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);
            if (stream.Length == 0)
            {
                return null;
            }
            List<ImageClass> imageDataList2;
            imageDataList2 = (List<ImageClass>)formatter.Deserialize(stream);
            //set the data in the gamemanager 
            GameManager.instance.SetListOfImageClassPlayedObj(imageDataList2);
            stream.Close();
            return imageDataList2;
        }
        else
        {
           // Debug.Log("Save file not found in " + path);
            return null;
        }
    }
    //DELETE DATA
    public void DeleteSavedData()
    {
        string path = Application.persistentDataPath + "/playerSave.txt";
        if (File.Exists(path))
        {
            if(PlayerPrefs.HasKey("firstTime"))
            {
                PlayerPrefs.DeleteKey("firstTime");
            }
            File.Delete(path);
            Debug.Log("deleted");
        }
        else
            Debug.Log("no save file found");
    }
}
