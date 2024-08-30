using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuPlayerLookat : MonoBehaviour
{
    public Transform playerCamera;
    public float distanceFromPlayer = 2.0f; // Distance to place the menu from the player

    private bool isRotated = false;

    private void Start()
    {
        // Start the coroutine to position the menu after a brief delay
        StartCoroutine(DelayedPositionMenu());
    }

    private IEnumerator DelayedPositionMenu()
    {
        yield return new WaitForSeconds(1);

        PositionMenu();
        isRotated = true;
    }

    private void PositionMenu()
    {
        // Set the position of the canvas in front of the player
        Vector3 newPosition = playerCamera.position + (playerCamera.forward * distanceFromPlayer);
        newPosition.y = transform.position.y;
        transform.position = newPosition;

        // Rotate the canvas to face the player
        transform.LookAt(playerCamera);
        transform.forward = -transform.forward;
        //transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0); // Optional: only rotate around the Y-axis
    }
}
