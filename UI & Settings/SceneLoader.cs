using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
//THIS SCRIPT IS USED FOR LOADING THE INITIAL SCENE WITH THE LOADING BAR BUT CAN BE EXPANDED INTO MORE THINGS RELATED TO SCENES
public class SceneLoader : MonoBehaviour
{
    public static SceneLoader instance;
    public GameObject loadingScene;
    public Slider bar;
    public TMP_Text progressText;
    int randIncrement;
    private void Awake()
    {
        instance = this;
        LoadMainMenuWithDelay();
    }
    public void LoadMainMenuWithDelay()
    {
        loadingScene.SetActive(true);
        StartCoroutine(ManualDelay());
    }
   
    IEnumerator ManualDelay()
    {
        int randDelay = Random.Range(1, 2);
        for (float i = 0; i <= 10; i += randIncrement)
        {
            randIncrement = Random.Range(1, 3);
            yield return new WaitForSeconds(randDelay);
            bar.value = i * 10;
            progressText.text = i * 10 + "%";
        }
        SceneManager.LoadSceneAsync((int)SceneIndexes.MAINMENU);
    }
}
