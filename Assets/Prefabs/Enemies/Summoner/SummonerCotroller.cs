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
    [SerializeField] private GameObject portal;

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
    private const float UPDATE_INTERVAL = 0.2f;

    // Animation parameters
    private static readonly int WalkSpeedParam = Animator.StringToHash("walkSpeed");
    private static readonly int IsSummoningParam = Animator.StringToHash("isSummoning");

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();
        startPosition = transform.position;
        SetNextWanderTime();
        InvokeRepeating(nameof(SlowUpdate), 0f, UPDATE_INTERVAL);
    }

    private void SlowUpdate()
    {
        float sqrDistanceToTarget = (target.position - transform.position).sqrMagnitude;
        float sqrDetectionRange = detectionRange * detectionRange;
        float sqrAttackRange = attackRange * attackRange;

        if (sqrDistanceToTarget <= sqrDetectionRange)
        {
            isInCombat = true;
            if (sqrDistanceToTarget <= sqrAttackRange && CanSeeTarget())
            {
                StopMovingAndAttack();
            }
            else if(!isSummoning)
            {
                agent.SetDestination(target.position);
            }
        }
        else
        {
            isInCombat = false;
            Wander();
        }

        UpdateAnimations();
    }

    private void Wander()
    {
        if (Time.time >= nextWanderTime)
        {
            Vector3 randomPoint = startPosition + Random.insideUnitSphere * wanderRadius;
            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
            SetNextWanderTime();
        }
    }
    private void StopMovingAndAttack()
    {
        agent.ResetPath();
        if (!isSummoning)
        {
            transform.LookAt(target);
            StartCoroutine(AttackCoroutine());
        }
    }
    private IEnumerator AttackCoroutine()
    {
        isSummoning = true;
        animator.SetTrigger(IsSummoningParam);
        GameObject portal = OpenPortal();
        yield return new WaitForSeconds(summonCooldown);
        isSummoning = false;
        agent.SetDestination(target.position);
        Destroy(portal);
    }


    public void Summon() {
        Vector3 spawnPoint = transform.position + transform.forward * 1.1f;
        GameObject minionToSpawn = Instantiate(minion, spawnPoint, transform.rotation);
    }

    public GameObject OpenPortal() {
        Vector3 spawnPoint = transform.position + transform.forward * 1.1f + Vector3.up;
        return Instantiate(portal, spawnPoint, transform.rotation);
    }

    private void UpdateAnimations()
    {
        float speed = agent.velocity.magnitude;
        animator.SetFloat(WalkSpeedParam, speed);
    }

    private bool CanSeeTarget()
    {
        if (isInCombat) {
            Vector3 direction = target.position - transform.position;
            return !Physics.SphereCast(
                transform.position,
                0.3f,
                direction.normalized,
                out RaycastHit hit,
                attackRange,
                obstacleLayer,
                QueryTriggerInteraction.Ignore
            ) || hit.transform == target;
        }
        return false;
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