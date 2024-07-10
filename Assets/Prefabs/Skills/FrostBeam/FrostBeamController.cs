using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrostBeamController : MonoBehaviour
{
    [SerializeField] float liveTime = 2f;
    GameObject tip;
    // Start is called before the first frame update
    void Start()
    {
        tip = GameObject.Find("tip");
        Destroy(gameObject, liveTime);
    }

    // Update is called once per frame
    void Update()
    {
        gameObject.transform.position = tip.transform.position - tip.transform.forward*0.5f;
        gameObject.transform.forward = tip.transform.forward;
        
    }
}
