using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerModelController : MonoBehaviour
{
    private Camera mainCamera;
    public LayerMask groundLayer; // Layer mask to specify which layers constitute the ground
    public float maxRaycastDistance = 100f; // Maximum distance for the raycast


    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (mainCamera != null)
        {
            Vector3 newPosition = mainCamera.transform.position;

            // Perform a sphere cast to determine the ground height
            RaycastHit hit;
            float sphereRadius = 0.5f; // Set this to the desired radius of the sphere

            if (Physics.SphereCast(newPosition, sphereRadius, Vector3.down, out hit, maxRaycastDistance, groundLayer))
            {
                newPosition.y = hit.point.y; // Set the y position to the ground height
            }
            else
            {
                Debug.LogWarning("Ground not found within sphere cast distance.");
                // Handle case where ground is not found, e.g., set to a default height or keep current height
                // newPosition.y = someDefaultHeight;
            }

            transform.position = newPosition + Vector3.up*1.2f;
        }
    }
}
