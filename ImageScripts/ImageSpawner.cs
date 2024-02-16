using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;
using UnityEngine.UI;
using Photon.Realtime;
using ExitGames.Client.Photon;

//THIS IS THE SCRIPT FOR LOADING THE IMAGES
//USING PHOTON SERIEALIZE VIEW, MASTER UPDATES FOR ITSELF AS WELL AS FOR CLIENT
public class ImageSpawner : MonoBehaviourPun, IPunObservable
{
    [SerializeField] GameObject oneImagePanel, twoImagePanelFirst, twoImagePanelSecond, twoImagePanelTrans;
    [SerializeField] int deleteAtCount;
    Sprite imageLocation = null;
    public static ImageSpawner instance;
    private string oneImageLocation, twoImageLocationFirst, twoImageLocationSecond;
    [HideInInspector] public int imageNameLength, currentImageID, currentLevelID;
    public string imageName = null;
    bool isTwoImages = false;
    ImageClass finalImageData = new ImageClass();
    private Dictionary<int, ImageClass> fileteredImageClassData = new Dictionary<int, ImageClass>();
    Dictionary<int, ImageClass> dictAllDataOfImageClass;
    public static System.Action LoadImagesEvent;
    private int currentLevelType = 1; // For lower loop iterate in getLevelByImage fucntion

    private void OnEnable()
    {
        LoadImagesEvent += LoadNewImage;
    }
    private void OnDisable()
    {
        LoadImagesEvent -= LoadNewImage;
    }
    private void Start()
    {
        instance = this;
    }

    #region FETCHING IMAGES AND FILTERING
    public void setfilteredImageClassData(Dictionary<int, ImageClass> fileteredImageClassData)
    {
        this.fileteredImageClassData = fileteredImageClassData; 
    }

    private ImageClass GetLevelBasedImage(Dictionary<int, ImageClass> filteredImageClassData)
    {
        List<ImageClass> _imageClassDataList = GetLevelByImage(filteredImageClassData); // Create new list to store levelType Images
        // Get Random image
        if(_imageClassDataList==null)
        {
            return null;
        }

        Debug.Log($"LevelType: {currentLevelType} Count: {_imageClassDataList.Count}");
        int _randomImageID = Random.Range(0, _imageClassDataList.Count);
        ImageClass _imageData = _imageClassDataList[_randomImageID];

        // Remove Image from "fileteredImageClassData" 
        int _imageKeyinDic = fileteredImageClassData.FirstOrDefault(x => x.Value.imageID == _imageData.imageID).Key;
        fileteredImageClassData.Remove(_imageKeyinDic);
        //ImageClass _imageData = fileteredImageClassData[UnityEngine.Random.Range(0, fileteredImageClassData.Count)];
        return _imageData;
    }

    private List<ImageClass> GetLevelByImage(Dictionary<int, ImageClass> _fileteredImageClassData)
    {
        List<ImageClass> _imageClassList = new List<ImageClass>(); // Create new list to store levelType Images

        if (_fileteredImageClassData == null)
            return null;

        for (int levelType = currentLevelType; levelType <= 5; levelType++)    // Check for all levelType
        {
            currentLevelType = levelType;
            foreach (var kvp in _fileteredImageClassData)
            {
                if (kvp.Value.levelType == currentLevelType)  //GameManager.instance.RoundNumber "Backup"
                {
                    _imageClassList.Add(kvp.Value);
                }
            }

            // Restart  ImageCollection after all Image are Complete!
            if (currentLevelType == 5 && _imageClassList.Count <= deleteAtCount)
            {
                currentLevelType = 1;
                DeleteAndLoadAgain();
                return null;
            }

            if (_imageClassList.Count != 0)
            return _imageClassList;
            
        }
        return _imageClassList;
    }
    void DeleteAndLoadAgain()
    {
        Save save = new Save();
        save.DeleteSavedData();
        GameManager.instance.LoadImageData();
        GameManager.instance.LoadNewImage();
    }
    public Dictionary<int, ImageClass> GetFilteredAllImageClassObj()
    {
        List<ImageClass> getSavedUserImageData = GetComponent<Save>().LoadPlayer();
        //List<ImageClass> getSavedUserImageData = new List<ImageClass>();
        if (getSavedUserImageData == null)
        {
            return GameManager.instance.imageDict; //return null if this game is loaded first time
        }
        // Get all Images (indirect take from textJSON.txt in Asset folder)
        dictAllDataOfImageClass = GameManager.instance.imageDict;
        foreach (var ImageClassObj in getSavedUserImageData)
        {
            dictAllDataOfImageClass.Remove(ImageClassObj.playedImageID); // Remove all viewed Image by Player

            //var dictVariable = dictAllDataOfImageClass.First(kv => kv.Key == ImageClassObj.playedImageID);
            //dictAllDataOfImageClass.Remove(dictVariable.Key);
        }
        return dictAllDataOfImageClass;
    }
    #endregion

    #region LOADING IMAGES BY MASTER AND UPDATED ON CLIENT
    public void LoadNewImage()
    {
        finalImageData = GetLevelBasedImage(fileteredImageClassData);
        if (finalImageData == null)
        {
            GameManager.instance.LoadNewImage();//null returned when deleted, so images will be loaded again from beginning
            return;
        }
        //if (finalImageData.imageSpriteURL.Count()==2)
        //if (GameManager.instance.RoundNumber <= 5)
        //{
            isTwoImages = false;
            oneImagePanel.SetActive(true);
            twoImagePanelFirst.SetActive(false);
            twoImagePanelSecond.SetActive(false);

            finalImageData.imageSpriteURL[0] = finalImageData.imageSpriteURL[0].ToLowerInvariant();
            imageLocation = Resources.Load<Sprite>(finalImageData.imageSpriteURL[0]);
            oneImagePanel.GetComponent<Image>().sprite = imageLocation;
            oneImageLocation = finalImageData.imageSpriteURL[0];
       // }
        //BELOW PART NOT USED NOW
        //else//for images with 2 images
        //{
        //    isTwoImages = true;
        //    oneImagePanel.SetActive(false);
        //    twoImagePanelFirst.SetActive(true);
        //    twoImagePanelSecond.SetActive(true);

        //    twoImageLocationFirst = (finalImageData.imageSpriteURL[0]);
        //    twoImageLocationSecond = (finalImageData.imageSpriteURL[1]);

        //    twoImagePanelFirst.GetComponent<Image>().sprite = Resources.Load<Sprite>(twoImageLocationFirst);
        //    twoImagePanelSecond.GetComponent<Image>().sprite = Resources.Load<Sprite>(twoImageLocationSecond);
        //}
        Debug.Log("image loaded: " + imageLocation);
        imageName = finalImageData.imageName;
        imageNameLength = imageName.Length;
        currentImageID = finalImageData.imageID;
        currentLevelID = finalImageData.levelType;
    }
    void Update()
    {
        // update the client
        if (!PhotonNetwork.IsMasterClient)
        {
            if (isTwoImages)
            {
                oneImagePanel.SetActive(false);
                twoImagePanelFirst.SetActive(true);
                twoImagePanelSecond.SetActive(true);
            }
            oneImagePanel.GetComponent<Image>().sprite = Resources.Load<Sprite>(oneImageLocation);
            twoImagePanelFirst.GetComponent<Image>().sprite = Resources.Load<Sprite>(twoImageLocationFirst);
            twoImagePanelSecond.GetComponent<Image>().sprite = Resources.Load<Sprite>(twoImageLocationSecond);
        }
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            //We own this player: send the others our data
            stream.SendNext(oneImageLocation);
            stream.SendNext(twoImageLocationFirst);
            stream.SendNext(twoImageLocationSecond);
            stream.SendNext(isTwoImages);
        }
        else
        {
            //Network player, receive data
            oneImageLocation = (string)stream.ReceiveNext();
            twoImageLocationFirst = (string)stream.ReceiveNext();
            twoImageLocationSecond = (string)stream.ReceiveNext();
            isTwoImages = (bool)stream.ReceiveNext();
        }
    }
    #endregion
}
