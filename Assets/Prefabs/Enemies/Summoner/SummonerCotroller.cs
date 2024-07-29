using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using Unity.VisualScripting;

public class SummonerController : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float attackRange = 20f;
    [SerializeField] private float summonCooldown = 5f;
    [SerializeField] private float detectionRange = 25f;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private GameObject minion;

    [Header("Wandering")]
    [SerializeField] private float wanderRadius = 10f;
    [SerializeField] private float minWanderWaitTime = 2f;
    [SerializeField] private float maxWanderWaitTime = 5f;

    private NavMeshAgent agent;
    private Animator animator;
    private bool isInCombat = false;
    private bool isSummoning = false;
    private bool isWandering = false;
    private Vector3 startPosition;
    private float nextWanderTime;

    // Animation parameters
    private static readonly int WalkSpeedParam = Animator.StringToHash("walkSpeed");
    private static readonly int IsSummoningParam = Animator.StringToHash("isSummoning");

    private void Start()
    {
        
        animator = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();
        startPosition = transform.position;
        SetNextWanderTime();
    }

    private void Update()
    {
        if (!isInCombat)
        {
            if (Vector3.Distance(target.position, transform.position) <= detectionRange)
            {
                isInCombat = true;
            }
            else if (!isWandering)
            {
                StartCoroutine(Wander());
            }
        }
        else if(!isSummoning) {
        
            StartCoroutine(CombatBehavior());
        }
        UpdateAnimationState();
    }

    private IEnumerator Wander()
    {
        if (Time.time >= nextWanderTime)
        {
            isWandering = true;
            Vector3 randomPoint = startPosition + Random.insideUnitSphere * wanderRadius;
            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
                while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
                {
                    if (isInCombat) yield break;
                    yield return null;
                }
            }

            yield return new WaitForSeconds(Random.Range(minWanderWaitTime, maxWanderWaitTime));
            SetNextWanderTime();
            isWandering = false;
        }
    }

    private IEnumerator CombatBehavior()
    {   transform.LookAt(target);
        if (!isSummoning)
        {
            float distanceToTarget = Vector3.Distance(transform.position, target.position);
            if (distanceToTarget <= attackRange && CanSeeTarget())
            {
                agent.SetDestination(transform.position);
                isSummoning = true;
                animator.SetTrigger(IsSummoningParam);
                yield return new WaitForSeconds(summonCooldown);
                isSummoning = false;

            }
        }

    }

    public void ResumeTarget() {
    agent.SetDestination(target.transform.position);
    }

    public void Summon() {

        Vector3 spawnPoint = transform.position + transform.forward * 1.1f;
        GameObject minionToSpawn = Instantiate(minion, spawnPoint, transform.rotation);
        
    }


    private void UpdateAnimationState()
    {
        float speed = agent.velocity.magnitude;
        animator.SetFloat(WalkSpeedParam, speed);
    }

    private bool CanSeeTarget()
    {
        // Implement line of sight check if needed
        return true;
    }

    private void SetNextWanderTime()
    {
        nextWanderTime = Time.time + Random.Range(minWanderWaitTime, maxWanderWaitTime);
    }

    public void TakeHit()
    {
        animator.SetTrigger("wound");
    }

    public void Die()
    {
        animator.SetTrigger("death");
        enabled = false;
        agent.enabled = false;
    }
}