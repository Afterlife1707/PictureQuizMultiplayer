using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonAnimation : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        LeanTween.scale(gameObject, new Vector3(1.155469f, 1.1875f, 1f), 1f).setEaseLinear().setLoopPingPong();   
    }
}
