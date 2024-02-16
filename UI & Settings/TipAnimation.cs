using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TipAnimation : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        float initial = this.gameObject.transform.position.y;
        float moveTo = initial - 0.3f;
        LeanTween.moveY(this.gameObject, moveTo, 0.7f).setLoopPingPong();
    }
}
