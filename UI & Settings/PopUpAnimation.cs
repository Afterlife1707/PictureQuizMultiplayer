using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopUpAnimation : MonoBehaviour
{
    public static PopUpAnimation instance;
    [SerializeField]
    float time;
    [SerializeField]
    public LeanTweenType type;
    private void Start()
    {
        instance = this;
    }
    public void OnPopUp()
    {
        this.transform.localScale = new Vector3(0, 0, 0);
        LeanTween.scale(this.gameObject,new Vector3(1.2f,1.2f,1), 0.5f);
    }
}
