using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeteorMovement : MonoBehaviour
{
    // Start is called before the first frame update
    // Start is called before the first frame update
    public float speed = 15f;
    GameObject tip;
    Vector3 tipInitialPositionForward;
    bool collisionFlag = false;
    [SerializeField] float lifeTime = 2f;
    [SerializeField] GameObject flash;
    [SerializeField] GameObject hit;
    [SerializeField] float hitOffset = 0f;
    [SerializeField] bool UseFirePointRotation;
    [SerializeField] GameObject[] Detached;
    [SerializeField] Vector3 rotationOffset = new Vector3(0, 0, 0);
    private Rigidbody rb;
    void Start()
    {
        tip = GameObject.Find("tip");
        tipInitialPositionForward = tip.transform.forward;
        rb = GetComponent<Rigidbody>();
        if (flash != null)
        {
            var flashInstance = Instantiate(flash, transform.position, Quaternion.identity);
            flashInstance.transform.forward = gameObject.transform.forward;
            var flashPs = flashInstance.GetComponent<ParticleSystem>();
            if (flashPs != null)
            {
                Destroy(flashInstance, flashPs.main.duration);
            }
            else
            {
                var flashPsParts = flashInstance.transform.GetChild(0).GetComponent<ParticleSystem>();
                Destroy(flashInstance, flashPsParts.main.duration);
            }
        }
       Destroy(gameObject, lifeTime);
    }

    //void FixedUpdate()
    //{
    //    if (collisionFlag)
    //    {
    //        rb.velocity = new Vector3(tipInitialPositionForward.x, 0, tipInitialPositionForward.z) * speed;
    //        //transform.position += transform.forward * (speed * Time.deltaTime);         
    //    }
    //}
    void OnCollisionEnter(Collision collision)
    {
        if (collisionFlag == false)
            rb.velocity = new Vector3(tipInitialPositionForward.x, 0, tipInitialPositionForward.z) * speed;
        collisionFlag = true;
        //Lock all axes movement and rotation
        //rb.constraints = RigidbodyConstraints.FreezeAll;
        //speed = 0;

        //ContactPoint contact = collision.contacts[0];
        //Quaternion rot = Quaternion.FromToRotation(Vector3.up, contact.normal);
        //Vector3 pos = contact.point + contact.normal * hitOffset;

        //if (hit != null)
        //{
        //    var hitInstance = Instantiate(hit, pos, rot);
        //    if (UseFirePointRotation) { hitInstance.transform.rotation = gameObject.transform.rotation * Quaternion.Euler(0, 180f, 0); }
        //    else if (rotationOffset != Vector3.zero) { hitInstance.transform.rotation = Quaternion.Euler(rotationOffset); }
        //    else { hitInstance.transform.LookAt(contact.point + contact.normal); }

        //    var hitPs = hitInstance.GetComponent<ParticleSystem>();
        //    if (hitPs != null)
        //    {
        //        Destroy(hitInstance, hitPs.main.duration);
        //    }
        //    else
        //    {
        //        var hitPsParts = hitInstance.transform.GetChild(0).GetComponent<ParticleSystem>();
        //        Destroy(hitInstance, hitPsParts.main.duration);
        //    }
        //}
        //foreach (var detachedPrefab in Detached)
        //{
        //    if (detachedPrefab != null)
        //    {
        //        detachedPrefab.transform.parent = null;
        //    }
        //}
        //Destroy(gameObject);
        //Destroy(collision.gameObject);
    }
}
