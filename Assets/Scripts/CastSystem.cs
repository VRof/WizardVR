using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class CastSystem : MonoBehaviour
{
    [SerializeField] GameObject FireBallPrefab;
    [SerializeField] GameObject FrostBeamPrefab;
    [SerializeField] GameObject MeteorPrefab;
    [SerializeField] GameObject HealPrefab;
    [SerializeField] GameObject ShieldPrefab;
    [SerializeField] GameObject SummonPrefab;
    [SerializeField] GameObject PortalEnterPrefab;
    [SerializeField] GameObject PortalExitPrefab;
    [SerializeField] float teleportOffset = 5f;
    [SerializeField] GameObject Tip;
    [SerializeField] GameObject PlayerModel;
    [SerializeField] GameObject XRorigin;

    [SerializeField] LayerMask groundLayer;// Layer mask to filter ground objects
    [SerializeField] LayerMask worldLayer;

    GameObject currentSkill;
    //// Update is called once per frame

    //void Update()
    //{


    //}

    public void PrepareSkill(string SpellName)
    {
        Draw.syncContext.Post(_ => // This code here will run on the main thread
        {
            Debug.Log(SpellName);

            //if(currentSkill != null)
            //        Destroy(currentSkill);
            switch (SpellName)
            {
                case "fireball":
                    //currentSkill = Instantiate(FireBallPrefab, Tip.transform.position, Tip.transform.rotation);
                    Instantiate(FireBallPrefab, Tip.transform.position, Tip.transform.rotation);
                    break;
                case "frostbolt":
                    //currentSkill = Instantiate(FrostBallPrefab, Tip.transform.position, Tip.transform.rotation);
                    Instantiate(FrostBeamPrefab, Tip.transform.position, Tip.transform.rotation);
                    break;
                case "meteor":
                    //currentSkill = Instantiate(MeteorPrefab, new Vector3(Player.transform.position.x,Player.transform.position.y+1,Player.transform.position.z), Player.transform.rotation);
                    GameObject spawnPoint = GameObject.Find("MeteorSpawnPoint");
                    GameObject meteor = Instantiate(MeteorPrefab, spawnPoint.transform.position, new Quaternion(0, 0, 0, 0));
                    meteor.transform.position += new Vector3(0, 3, 0);
                    break;
                case "heal":
                    Instantiate(HealPrefab, new Vector3(PlayerModel.transform.position.x, PlayerModel.transform.position.y - 2 * PlayerModel.transform.localScale.y, PlayerModel.transform.position.z), PlayerModel.transform.rotation);
                    break;
                case "shield":
                    Instantiate(ShieldPrefab, new Vector3(PlayerModel.transform.position.x, PlayerModel.transform.position.y - 2 * PlayerModel.transform.localScale.y, PlayerModel.transform.position.z), PlayerModel.transform.rotation);
                    break;
                case "summon":
                    spawnPoint = GameObject.Find("MeteorSpawnPoint");
                    float raycastHeight = 100f; // Height from which to cast the ray
                                                // Start the raycast from high above the ground
                    Vector3 rayOrigin = new Vector3(spawnPoint.transform.position.x, raycastHeight, spawnPoint.transform.position.z);
                    //GameObject meteor = Instantiate(MeteorPrefab, spawnPoint.transform.position, new Quaternion(0, 0, 0, 0));
                    // Cast the ray downwards
                    if (!Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, Mathf.Infinity, worldLayer))
                    {
                        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, Mathf.Infinity, groundLayer))
                        {
                            // Get the position where the ray hit the ground
                            Vector3 spawnPosition = hit.point;
                            // Instantiate the object at the hit point
                            Instantiate(SummonPrefab, spawnPosition, Quaternion.identity);
                        }
                    }
                    else
                    {
                        Debug.Log("summon" + "No ground found at position: " + spawnPoint.transform.position);
                    }
                    break;
                case "teleport":
                    spawnPoint = GameObject.Find("MeteorSpawnPoint");
                    raycastHeight = 100f; // Height from which to cast the ray
                                          // Start the raycast from high above the ground
                    rayOrigin = new Vector3(spawnPoint.transform.position.x+Tip.transform.forward.x*2, raycastHeight, spawnPoint.transform.position.z + Tip.transform.forward.z * 2);
                    //GameObject meteor = Instantiate(MeteorPrefab, spawnPoint.transform.position, new Quaternion(0, 0, 0, 0));
                    // Cast the ray downwards
                    if (!Physics.Raycast(rayOrigin, Vector3.down, out hit, Mathf.Infinity, worldLayer))
                    {
                        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, Mathf.Infinity, groundLayer))
                        {
                            // Get the position where the ray hit the ground
                            Vector3 spawnPosition = hit.point;
                            // Instantiate the object at the hit point
                            if (CanPlaceExitPortal()) {
                            GameObject portalEnter = Instantiate(PortalEnterPrefab, spawnPosition, new Quaternion(0, Tip.transform.rotation.y, 0, Tip.transform.rotation.w));
                            portalEnter.transform.position += new Vector3(0, 1, 0);
                                portalEnter.name = "PortalEnter";
                        }

                        }
                    }
                    else
                    {
                        Debug.Log("teleport" + "No ground found at position: " + spawnPoint.transform.position);
                    }
                    break;
                default:
                    break;
            }
            
        }, null);
        
    }
    private bool CanPlaceExitPortal() {

        GameObject spawnPoint = GameObject.Find("MeteorSpawnPoint");
        float raycastHeight = 100f; // Height from which to cast the ray
                                    // Start the raycast from high above the ground
        float redPortalHeight = 10f;

        GameObject portalExit;
        Vector3 rayOrigin = new Vector3(spawnPoint.transform.position.x + Tip.transform.forward.x*teleportOffset, raycastHeight, spawnPoint.transform.position.z + Tip.transform.forward.z * teleportOffset);
        // Cast the ray downwards
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, Mathf.Infinity, worldLayer))
        {
            //GameObject hitObject = hit.collider.gameObject;
            Vector3 closestPoint = GetClosestPointOnBoundsInXZ(hit.collider.bounds, hit.point);
            portalExit = Instantiate(PortalExitPrefab, new Vector3(closestPoint.x, closestPoint.y + redPortalHeight, closestPoint.z) - new Vector3(Tip.transform.forward.x*2,0,Tip.transform.forward.z*2), PortalExitPrefab.transform.rotation);
            portalExit.name = "PortalExit";
            return true;
        }
        else if (Physics.Raycast(rayOrigin, Vector3.down, out hit, Mathf.Infinity, groundLayer))
        {
            // Get the position where the ray hit the ground
            Vector3 spawnPosition = hit.point;
            // Instantiate the object at the hit point
            Debug.Log("red portal on world object");
            portalExit = Instantiate(PortalExitPrefab, new Vector3(spawnPosition.x, spawnPosition.y + redPortalHeight, spawnPosition.z), PortalExitPrefab.transform.rotation);
            portalExit.name = "PortalExit";
            return true;
        }

        return false;
    }
    Vector3 GetClosestPointOnBoundsInXZ(Bounds bounds, Vector3 point)
    {
        Vector3 closestPoint = point;

        // Determine the closest x boundary
        if (Mathf.Abs(point.x - bounds.min.x) < Mathf.Abs(point.x - bounds.max.x))
        {
            closestPoint.x = bounds.min.x;
        }
        else
        {
            closestPoint.x = bounds.max.x;
        }

        // Determine the closest z boundary
        if (Mathf.Abs(point.z - bounds.min.z) < Mathf.Abs(point.z - bounds.max.z))
        {
            closestPoint.z = bounds.min.z;
        }
        else
        {
            closestPoint.z = bounds.max.z;
        }

        return closestPoint;
    }
}
