using UnityEngine;

[System.Serializable]
public class ImageClass 
{
    public ImageAnswers answer;

    public int levelType;
    public int imageID;
    public string imageName;
    public string[] imageSpriteURL;
    public int playedImageID { get; set; }

    public ImageClass()
    {
    }

    public ImageClass(int currentImageID)
    {
        playedImageID = currentImageID;
    }
}

