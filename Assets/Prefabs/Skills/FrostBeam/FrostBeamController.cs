using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrostBeamController : MonoBehaviour
{
    [SerializeField] float liveTime = 2f;
    public float damage = 3f;
    GameObject tip;
    // Start is called before the first frame update
    void Start()
    {
        gameObject.name = "frostbeam";
        tip = GameObject.Find("tip");
        Destroy(gameObject, liveTime);
    }

    // Update is called once per frame
    void Update()
    {
        if(tip != null)
        {
            gameObject.transform.position = tip.transform.position - tip.transform.forward * 0.5f;
            gameObject.transform.forward = tip.transform.forward;
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        ApplyDamage(collision.collider);
    }
    private void ApplyDamage(Collider collider)
    {
        // Skip self or non-damageable objects
        if (collider.gameObject == gameObject) return;

        IDamageable damageable = collider.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage); // Apply damage
        }
    }
}
