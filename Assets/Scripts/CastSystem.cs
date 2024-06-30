using UnityEngine;

public class CastSystem : MonoBehaviour
{
    [SerializeField] GameObject FireBallPrefab;
    [SerializeField] GameObject FrostBallPrefab;
    [SerializeField] GameObject MeteorPrefab;
    [SerializeField] GameObject Tip;
    [SerializeField] GameObject PlayerModel;

    [SerializeField] float SkillFlySpeed = 300f;

    GameObject currentSkill;
    //// Update is called once per frame

    //void Update()
    //{

    //    //if (currentSkill)
    //    //{

    //        //currentSkill.GetComponent<Rigidbody>().velocity = Tip.transform.forward * SkillFlySpeed * Time.deltaTime;
    //        //currentSkill.transform.position = tip.transform.position;
    //        //currentSkill.transform.rotation = tip.transform.rotation;
    //       // }
    //   // }
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
                    Instantiate(FrostBallPrefab, Tip.transform.position, Tip.transform.rotation);
                    break;
                case "meteor":
                //currentSkill = Instantiate(MeteorPrefab, new Vector3(Player.transform.position.x,Player.transform.position.y+1,Player.transform.position.z), Player.transform.rotation);
                    GameObject spawnPoint = GameObject.Find("MeteorSpawnPoint");
                    GameObject meteor = Instantiate(MeteorPrefab, spawnPoint.transform.position, new Quaternion(0,0,0,0));
                    meteor.transform.position += new Vector3(0,3,0);
                    break;
            default:
                break;
        }
        }, null);
    }
}
