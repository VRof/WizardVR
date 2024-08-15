using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyCasterController : MonoBehaviour, IDamageable
{
    [Header("References")]
    private GameObject target;
    [SerializeField] private GameObject skillToCast;
    [SerializeField] private Transform enemyCasterCastPoint;
    private NavMeshAgent agent;
    private Animator animator;

    [Header("Stats")]
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float attackRange = 10f;
    [SerializeField] private float attackCooldown = 2f;

    [Header("Wandering")]
    [SerializeField] private float wanderRadius = 10f;
    [SerializeField] private float minWanderWaitTime = 2f;
    [SerializeField] private float maxWanderWaitTime = 5f;

    [Header("Raycasting")]
    [SerializeField] private float sphereCastRadius = 0.5f;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("HealthBar")]
    [SerializeField] float maxHealth = 100;
    float currentHealth;
    [SerializeField] private EnemyHealthBar healthBar;

    private Collider enemyCollider;
    private Vector3 startPosition;
    private bool isInCombat = false;
    private float nextWanderTime;
    private bool isAttacking = false;
    private bool tookDamage = false;
    private float nextUpdateTime;
    private const float UPDATE_INTERVAL = 0.2f;
    [SerializeField] public static int Damage = -5;

    // Animation parameters
    private static readonly int IsWalkingParam = Animator.StringToHash("isWalking");
    private static readonly int IsAttackingParam = Animator.StringToHash("isAttacking");
    private static readonly int IsInCombatParam = Animator.StringToHash("isInCombat");

    private bool isDying = false;
    private void Start()
    {
        target = GameObject.Find("PlayerModel");
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        startPosition = transform.position;
        enemyCollider = GetComponent<Collider>();
        currentHealth = maxHealth;
        healthBar.UpdateEnemyHealthBar(maxHealth, currentHealth);

        SetNextWanderTime();
        InvokeRepeating(nameof(SlowUpdate), 0f, UPDATE_INTERVAL);
    }


    private void SlowUpdate()
    {
        if(isDying) return;
        if (target == null) return;

        float sqrDistanceToTarget = (target.transform.position - transform.position).sqrMagnitude;
        float sqrDetectionRange = detectionRange * detectionRange;
        float sqrAttackRange = attackRange * attackRange;

        if (sqrDistanceToTarget <= sqrDetectionRange || tookDamage)
        {
            isInCombat = true;
            if (sqrDistanceToTarget <= sqrAttackRange && CanSeeTarget())
            {
                StopMovingAndAttack();
            }
            else
            {
                ChaseTarget();
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

    private void SetNextWanderTime()
    {
        nextWanderTime = Time.time + Random.Range(minWanderWaitTime, maxWanderWaitTime);
    }

    private void StopMovingAndAttack()
    {
        agent.ResetPath();
        animator.SetBool(IsInCombatParam, true);
        if (!isAttacking)
        {
            transform.LookAt(target.transform.position);
            StartCoroutine(AttackCoroutine());
        }
    }

    private void ChaseTarget()
    {
        if (isDying) return;
        agent.SetDestination(target.transform.position);
    }

    private bool CanSeeTarget()
    {
        if (isInCombat)
        {
            Vector3 direction = target.transform.position - 0.1f * Vector3.up - enemyCasterCastPoint.position;
            return !Physics.SphereCast(
                transform.position,
                sphereCastRadius,
                direction.normalized,
                out RaycastHit hit,
                attackRange,
                obstacleLayer,
                QueryTriggerInteraction.Ignore
            ) || hit.transform == target;
        }
    return false;
    }

    private IEnumerator AttackCoroutine()
    {
        isAttacking = true;
        animator.SetBool(IsAttackingParam, true);
        yield return new WaitForSeconds(attackCooldown);
        animator.SetBool(IsAttackingParam, false);
        isAttacking = false;
    }

    public void CastSpell()
    {
        if (skillToCast != null && enemyCasterCastPoint != null)
        {
            GameObject spellInstance = Instantiate(skillToCast, enemyCasterCastPoint.position, enemyCasterCastPoint.rotation);
            if (spellInstance.TryGetComponent(out Rigidbody spellRb))
            {
                Vector3 direction = (target.transform.position - 0.1f * Vector3.up - enemyCasterCastPoint.position).normalized;
                spellRb.AddForce(direction * 500f);
            }
        }
    }

    private void UpdateAnimations()
    {
        animator.SetBool(IsWalkingParam, agent.velocity.sqrMagnitude > 0.01f);
    }

    public void TakeDamage(float damage)
    {
        tookDamage = true;
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            healthBar.UpdateEnemyHealthBar(maxHealth, 0);
            Die();
        }
        else
        {
            animator.SetTrigger("isDamage"); 
            healthBar.UpdateEnemyHealthBar(maxHealth, currentHealth);
        }
    }
    public void Die()
    {
        isDying = true;
        StartCoroutine(DieCoroutine());
    }
    private IEnumerator DieCoroutine()
    {
        agent.enabled = false;
        animator.SetTrigger("isDead");
        Destroy(enemyCollider);

        yield return new WaitForSeconds(4.8f);

        enabled = false;
        Destroy(gameObject);
    }
}