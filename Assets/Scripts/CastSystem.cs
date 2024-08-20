using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.Windows;

public class CastSystem : MonoBehaviour
{
    [SerializeField] GameObject FireBallPrefab;
    [SerializeField] public static int FireBoltManaCost = 5;
    [SerializeField] GameObject FrostBeamPrefab;
    [SerializeField] public static int FrostBeamManaCost = 10;
    [SerializeField] GameObject MeteorPrefab;
    [SerializeField] public static int MeteorManaCost = 50;
    [SerializeField] GameObject HealPrefab;
    [SerializeField] public static int HealManaCost = 30;
    [SerializeField] GameObject ShieldPrefab;
    [SerializeField] public static int ShieldManaCost = 10;
    [SerializeField] GameObject SummonPrefab;
    [SerializeField] public static int SummonManaCost = 30;
    [SerializeField] GameObject PortalEnterPrefab;
    [SerializeField] public static int PortalManaCost = 10;
    [SerializeField] GameObject PortalExitPrefab;
    [SerializeField] float teleportOffset = 5f;
    [SerializeField] GameObject Tip;
    [SerializeField] GameObject PlayerModel;
    [SerializeField] GameObject XRorigin;

    [SerializeField] LayerMask groundLayer;// Layer mask to filter ground objects
    [SerializeField] LayerMask worldLayer;
    [SerializeField] GameObject PredictionMenuCanvas;
    [SerializeField] GameObject rayInteractor;

    [Header("Debug info")]
    [SerializeField] bool debug = false;
    [SerializeField] TMP_Text PredictionTextField;



    GameObject currentSkill;
    Player playerScript;
    TMP_Dropdown PredictionsDropdown;
    bool isPaused = false;
    //// Update is called once per frame

    void Update()
    {

    }

    private void Start()
    {
        playerScript = GameObject.Find("PlayerModel").GetComponent<Player>();
        if (debug && PredictionTextField != null) {
            PredictionTextField.text = "['fireball 0.00%', 'frostbeam 0.00%', 'heal 0.00%', 'meteor 0.00%', 'others 0.00%', 'shield 0.00%', 'summon 0.00%', 'teleport 0.00%']";
        }
        PredictionsDropdown = PredictionMenuCanvas.GetComponentInChildren<TMP_Dropdown>();
    }

    public void PrepareSkill(string PredictionArray)
    {
        Draw.syncContext.Post(_ => // This code here will run on the main thread
        {
            Debug.Log(PredictionArray);
            if(PredictionTextField && debug) PredictionTextField.text = PredictionArray;
            Dictionary<string, float> skillDict  = ParseSkillString(PredictionArray);
            var maxSkill = skillDict.Aggregate((l, r) => l.Value > r.Value ? l : r);
            var maxValue = maxSkill.Value;
            var maxSkillName = maxSkill.Key;
            if (maxValue > 90)
            {
                Cast(maxSkillName);
            }
            else 
            {
                //PredictionsDropdown.ClearOptions();
                //List<string> optionsList = new List<string>();
                //foreach (var skill in skillDict.OrderByDescending(skill => skill.Value))
                //{
                //    optionsList.Add(skill.Key + ": " + skill.Value);
                //}
                //PredictionsDropdown.AddOptions(optionsList);
                //PredictionsDropdown.RefreshShownValue();
                Pause();
            }

        }, null);
        
    }

    public void Resume()
    {
        rayInteractor.SetActive(false);
        PredictionMenuCanvas.SetActive(false); // Hide the custom menu
        Draw drawScript = gameObject.GetComponent<Draw>();
        drawScript.enabled = true;
        Time.timeScale = 1f; // Resume the game
        isPaused = false;
    }

    void Pause()
    {
        Draw drawScript = gameObject.GetComponent<Draw>();
        drawScript.enabled = false;
        rayInteractor.SetActive(true);
        PredictionMenuCanvas.SetActive(true); // Show the custom menu
        Time.timeScale = 0f; // Pause the game
        isPaused = true;
        //PredictionsDropdown.Show();

    }
    public void IsFireball() 
    {
        pythonConnector.SetDataToSend(Encoding.UTF8.GetBytes("fireball"));
        Resume();
        Cast("fireball");
    }
    public void IsFrostbeam()
    {
        pythonConnector.SetDataToSend(Encoding.UTF8.GetBytes("frostbeam"));
        Resume();
        Cast("frostbeam");
    }
    public void IsTeleport()
    {
        pythonConnector.SetDataToSend(Encoding.UTF8.GetBytes("teleport"));
        Resume();
        Cast("teleport");
    }
    public void IsHeal()
    {
        pythonConnector.SetDataToSend(Encoding.UTF8.GetBytes("heal"));
        Resume();
        Cast("heal");
    }
    public void IsShield()
    {
        pythonConnector.SetDataToSend(Encoding.UTF8.GetBytes("shield"));
        Resume();
        Cast("shield"); 
    }
    public void IsSummon()
    {
        pythonConnector.SetDataToSend(Encoding.UTF8.GetBytes("summon"));
        Resume();
        Cast("summon");
    }
    public void IsMeteor()
    {
        pythonConnector.SetDataToSend(Encoding.UTF8.GetBytes("meteor"));
        Resume();
        Cast("meteor");
    }
    public void closeButtonHandler() 
    {
        Resume();
    }

    private void Cast(string maxSkillName)
    {
        switch (maxSkillName)
        {
            case "fireball":
                //currentSkill = Instantiate(FireBallPrefab, Tip.transform.position, Tip.transform.rotation);
                if (TryUseMana(FireBoltManaCost))
                {
                    Instantiate(FireBallPrefab, Tip.transform.position, Tip.transform.rotation);
                }
                break;
            case "frostbeam":
                //currentSkill = Instantiate(FrostBallPrefab, Tip.transform.position, Tip.transform.rotation);
                if (TryUseMana(FrostBeamManaCost))
                {
                    Instantiate(FrostBeamPrefab, Tip.transform.position, Tip.transform.rotation);
                }
                break;
            case "meteor":
                //currentSkill = Instantiate(MeteorPrefab, new Vector3(Player.transform.position.x,Player.transform.position.y+1,Player.transform.position.z), Player.transform.rotation);
                GameObject spawnPoint = GameObject.Find("MeteorSpawnPoint");
                if (TryUseMana(MeteorManaCost))
                {
                    GameObject meteor = Instantiate(MeteorPrefab, spawnPoint.transform.position, new Quaternion(0, 0, 0, 0));
                    meteor.transform.position += new Vector3(0, 3, 0);
                }
                break;
            case "heal":
                if (TryUseMana(HealManaCost))
                {
                    Instantiate(HealPrefab, new Vector3(PlayerModel.transform.position.x, PlayerModel.transform.position.y - 2 * PlayerModel.transform.localScale.y, PlayerModel.transform.position.z), PlayerModel.transform.rotation);
                }
                break;
            case "shield":
                if (TryUseMana(ShieldManaCost))
                {
                    Instantiate(ShieldPrefab, new Vector3(PlayerModel.transform.position.x, PlayerModel.transform.position.y - 2 * PlayerModel.transform.localScale.y, PlayerModel.transform.position.z), PlayerModel.transform.rotation);
                }
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
                        if (TryUseMana(SummonManaCost))
                        {
                            // Get the position where the ray hit the ground
                            Vector3 spawnPosition = hit.point;
                            // Instantiate the object at the hit point
                            Instantiate(SummonPrefab, spawnPosition, Quaternion.identity);
                        }
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
                rayOrigin = new Vector3(spawnPoint.transform.position.x + Tip.transform.forward.x * 2, raycastHeight, spawnPoint.transform.position.z + Tip.transform.forward.z * 2);
                //GameObject meteor = Instantiate(MeteorPrefab, spawnPoint.transform.position, new Quaternion(0, 0, 0, 0));
                // Cast the ray downwards
                if (!Physics.Raycast(rayOrigin, Vector3.down, out hit, Mathf.Infinity, worldLayer))
                {
                    if (Physics.Raycast(rayOrigin, Vector3.down, out hit, Mathf.Infinity, groundLayer))
                    {
                        // Get the position where the ray hit the ground
                        Vector3 spawnPosition = hit.point;
                        // Instantiate the object at the hit point
                        if (CanPlaceExitPortal())
                        {
                            if (TryUseMana(PortalManaCost))
                            {
                                GameObject portalEnter = Instantiate(PortalEnterPrefab, spawnPosition, new Quaternion(0, Tip.transform.rotation.y, 0, Tip.transform.rotation.w));
                                portalEnter.transform.position += new Vector3(0, 1, 0);
                                portalEnter.name = "PortalEnter";
                            }
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
    }

    private bool TryUseMana(int amount) {
        if (playerScript.GetCurrentMana() - amount < 0) {
            return false;
        }
        playerScript.PlayerUpdateMana(-amount);
        return true;
    }
    public static Dictionary<string, float> ParseSkillString(string skillString)
    {
        // Remove the outer brackets and single quotes
        skillString = skillString.Trim('[', ']', '\'');

        // Split the string into individual skill entries
        string[] skillEntries = skillString.Split(new string[] { "', '" }, StringSplitOptions.RemoveEmptyEntries);

        // Create a dictionary to store the parsed skills and their percentages
        Dictionary<string, float> skillDict = new Dictionary<string, float>();

        // Regex pattern to extract the skill name and percentage
        string pattern = @"^(?<name>\w+)\s(?<value>\d+\.\d+)%$";
        Regex regex = new Regex(pattern);

        foreach (string entry in skillEntries)
        {
            Match match = regex.Match(entry);
            if (match.Success)
            {
                string name = match.Groups["name"].Value;
                float value = float.Parse(match.Groups["value"].Value);
                skillDict[name] = value;
            }
            else
            {
                Console.WriteLine($"Failed to parse entry: {entry}");
            }
        }

        return skillDict;
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
