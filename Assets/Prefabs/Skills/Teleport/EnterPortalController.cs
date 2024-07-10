using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnterPortalController : MonoBehaviour
{
    [SerializeField] float lifeTime = 5f;
    // Start is called before the first frame update
    GameObject ExitPortal;
    void Start()
    {
        ExitPortal = GameObject.Find("PortalExit");
        Destroy(ExitPortal,lifeTime);
        Destroy(gameObject,lifeTime);
    }

    // Update is called once per frame
    //void Update()
    //{

    //}

    private void OnTriggerEnter(Collider other)
    {
        GameObject obj = other.gameObject;

        obj.transform.position = ExitPortal.transform.position;
    }
}
