using UnityEngine;

public class DataController : MonoBehaviour
{
    [SerializeField]
    TextAsset textJSON;
    private static DataController _dataController;
    public ImageClass[] imageData;
    private void Start()
    {
        _dataController = this;
        //DontDestroyOnLoad(this.gameObject);
        imageData = JsonHelper.GetArray<ImageClass>(textJSON.text); //load data from json
    }
    public static DataController GetDataController { get { return _dataController; } }

    public ImageClass[] LoadImageData()
    {
        return imageData;
    }
}
