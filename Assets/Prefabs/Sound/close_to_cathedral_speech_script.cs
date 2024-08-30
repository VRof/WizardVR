using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class close_to_cathedral_speech_script : MonoBehaviour
{
    private AudioSource speech;

    void Start()
    {
        speech = GetComponent<AudioSource>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("PlayerTag"))
        {
            // Play the speech audio
            if (speech != null && !speech.isPlaying)
            {
                speech.Play();
            }
        }
    }
}
