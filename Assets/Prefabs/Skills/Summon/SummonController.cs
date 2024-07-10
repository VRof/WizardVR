using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class SummonController : MonoBehaviour
{
    [SerializeField] float lifeTime = 4f;
    [SerializeField] float spawnInterval = 1f;
    [SerializeField] GameObject minion;
    // Start is called before the first frame update
    void Start()
    {
        // Destroy the game object after a specified lifetime
        Destroy(gameObject, lifeTime);

        // Start spawning minions every second
        InvokeRepeating("SpawnMinion", 1f, spawnInterval);
    }

    // Method to spawn a minion
    void SpawnMinion()
    {
        Instantiate(minion, transform.position, transform.rotation);
    }

    // Update is called once per frame
    void Update()
    {
        // Optionally, you can add more functionality here
    }

    // This method is called when the game object is destroyed
    void OnDestroy()
    {
        // Cancel the InvokeRepeating when the object is destroyed
        CancelInvoke("SpawnMinion");
    }
}
