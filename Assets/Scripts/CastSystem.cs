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
    [SerializeField] GameObject Tip;
    [SerializeField] GameObject PlayerModel;

    [SerializeField] LayerMask groundLayer;// Layer mask to filter ground objects
    [SerializeField] LayerMask defaultLayer;

    GameObject currentSkill;
    //// Update is called once per frame

    //void Update()
    //{

  
    //}

    public void PrepareSkill(string SpellName) {
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
                    GameObject meteor = Instantiate(MeteorPrefab, spawnPoint.transform.position, new Quaternion(0,0,0,0));
                    meteor.transform.position += new Vector3(0,3,0);
                    break;
                case "heal":
                    Instantiate(HealPrefab, new Vector3(PlayerModel.transform.position.x, PlayerModel.transform.position.y - 2*PlayerModel.transform.localScale.y, PlayerModel.transform.position.z), PlayerModel.transform.rotation);
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
                    if (!Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, Mathf.Infinity, defaultLayer))
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
                    rayOrigin = new Vector3(spawnPoint.transform.position.x, raycastHeight, spawnPoint.transform.position.z);
                    //GameObject meteor = Instantiate(MeteorPrefab, spawnPoint.transform.position, new Quaternion(0, 0, 0, 0));
                    // Cast the ray downwards
                    if (!Physics.Raycast(rayOrigin, Vector3.down, out hit, Mathf.Infinity, defaultLayer))
                    {
                        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, Mathf.Infinity, groundLayer))
                        {
                            // Get the position where the ray hit the ground
                            Vector3 spawnPosition = hit.point;
                            // Instantiate the object at the hit point
                            GameObject portalEnter = Instantiate(PortalEnterPrefab, spawnPosition, new Quaternion(0, Tip.transform.rotation.y, 0, Tip.transform.rotation.w));
                            portalEnter.transform.position += new Vector3(0, 1, 0);

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
}
