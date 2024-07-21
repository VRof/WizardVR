using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldController : MonoBehaviour
{

    [SerializeField] float liveTime = 4f;
    GameObject player;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("PlayerModel");
        Destroy(gameObject, liveTime);
    }

    // Update is called once per frame
    void Update()
    {
        //gameObject.transform.position = new Vector3(player.transform.position.x, player.transform.position.y - 2 * player.transform.localScale.y, player.transform.position.z);
        gameObject.transform.position = player.transform.position - Vector3.up;
        gameObject.transform.rotation = player.transform.rotation;
    }
}
