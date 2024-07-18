using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyCasterController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject target;
    [SerializeField] private GameObject skillToCast;
    [SerializeField] private GameObject EnemyCasterCastPoint;
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
    [SerializeField] private bool visualizeSphereCast = false;
    [SerializeField] private Color sphereCastColor = Color.red;

    private Vector3 startPosition;
    private bool isInCombat = false;
    private float nextWanderTime;
    private bool isAttacking = false;

    // Animation parameters
    private static readonly int IsWalkingParam = Animator.StringToHash("isWalking");
    private static readonly int IsDeadParam = Animator.StringToHash("isDead");
    private static readonly int IsDamagedParam = Animator.StringToHash("isDamaged");
    private static readonly int IsAttackingParam = Animator.StringToHash("isAttacking");
    private static readonly int IsInCombatParam = Animator.StringToHash("isInCombat");


    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        startPosition = transform.position;
        SetNextWanderTime();
    }

    private void Update()
    {
        float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);

        if (distanceToTarget <= detectionRange)
        {
            isInCombat = true;
        }
        else if (distanceToTarget > detectionRange && !isInCombat)
        {
            Wander();
        }
        if (isInCombat)
        {
            
            ChaseAndAttackTarget(distanceToTarget);
        }

        UpdateAnimations();

        if (visualizeSphereCast)
        {
            VisualizeSphereCast();
        }

    }

    private void Wander()
    {
        if (Time.time >= nextWanderTime)
        {
            Vector3 randomPoint = startPosition + Random.insideUnitSphere * wanderRadius;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, wanderRadius, NavMesh.AllAreas))
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

    private void ChaseAndAttackTarget(float distanceToTarget)
    {
        if (distanceToTarget <= attackRange)
        {
            animator.SetBool(IsInCombatParam, isInCombat);
            StopMoving();
            if (!isAttacking && CanSeeTarget())
            {
                transform.LookAt(target.transform.position);
                StartCoroutine(AttackCoroutine());
            }
            else if (!CanSeeTarget())
            {
                agent.SetDestination(target.transform.position);
            }
        }
        else
        {
            agent.SetDestination(target.transform.position);
        }
    }

    private void StopMoving()
    {
        agent.SetDestination(transform.position);
    }

    private bool CanSeeTarget()
    {
        Vector3 direction = target.transform.position - transform.position - Vector3.up;
        Vector3 start = transform.position + Vector3.up;

        // Using SphereCast for a thicker ray
        if (Physics.SphereCast(
            start,
            sphereCastRadius,
            direction.normalized,
            out RaycastHit hit,
            attackRange,
            obstacleLayer,
            QueryTriggerInteraction.UseGlobal
        ))
        // If we hit something, check if it's the target
        return hit.transform.gameObject == target;
        else
        // If we didn't hit anything, it means there's a clear line of sight to the target
        return true;
    }

    private IEnumerator AttackCoroutine()
    {

        isAttacking = true;
        animator.SetBool(IsAttackingParam, true);

        // Wait for the attack cooldown
        yield return new WaitForSeconds(attackCooldown);

        //Debug.Log("Attack cooldown finished");
        animator.SetBool(IsAttackingParam, false);
        isAttacking = false;
    }

    // This function will be called by the animation event at the end of the attack animation
    public void CastSpell()
    {
        //Debug.Log("Mage is casting a spell!");

        // Instantiate the skill prefab and apply force to it
        if (skillToCast != null && EnemyCasterCastPoint != null)
        {
            GameObject spellInstance = Instantiate(skillToCast, EnemyCasterCastPoint.transform.position, EnemyCasterCastPoint.transform.rotation);
            Rigidbody spellRb = spellInstance.GetComponent<Rigidbody>();
            if (spellRb != null)
            {
                Vector3 direction = (new Vector3(target.transform.position.x, target.transform.position.y-0.5f, target.transform.position.z) - EnemyCasterCastPoint.transform.position).normalized;
                spellRb.AddForce(direction * 500f); // Adjust the force value as needed
            }
            else
            {
                Debug.LogWarning("The skill prefab does not have a Rigidbody component.");
            }
        }
        else
        {
            Debug.LogWarning("Skill Prefab or EnemyCasterCastPoint is not assigned.");
        }
    }

    private void UpdateAnimations()
    {
        bool isMoving = agent.velocity.magnitude > 0.3f;
        animator.SetBool(IsWalkingParam, isMoving);
    }

    public void TakeDamage(float damage)
    {
        animator.SetTrigger(IsDamagedParam);
        // Implement damage logic
    }

    public void Die()
    {
        animator.SetBool(IsDeadParam, true);
        agent.enabled = false;
        enabled = false;
    }

    private void VisualizeSphereCast()
    {
        if (target == null) return;

        Vector3 direction = target.transform.position - transform.position;
        float distance = direction.magnitude;
        Vector3 start = transform.position + Vector3.up;
        Vector3 end = start + Vector3.up;

        // Draw the ray
        Debug.DrawRay(start, direction.normalized * attackRange, Color.yellow);

        // Draw start sphere
        DrawWireSphere(start, sphereCastRadius, sphereCastColor);

        // Draw end sphere
        DrawWireSphere(end, sphereCastRadius, sphereCastColor);

        // Draw the capsule shape
        Debug.DrawLine(start, end, sphereCastColor);

        // Perform the actual SphereCast
        if (Physics.SphereCast(
            start,
            sphereCastRadius,
            direction.normalized,
            out RaycastHit hit,
            attackRange,
            obstacleLayer,
            QueryTriggerInteraction.UseGlobal
        ))
        {
            // Draw hit point
            DrawWireSphere(hit.point, sphereCastRadius, Color.green);
            Debug.DrawLine(start, hit.point, Color.green);
        }
        else
        {
            // Draw full path if nothing was hit
            Vector3 endPoint = start + direction.normalized * attackRange;
            Debug.DrawLine(start, endPoint, sphereCastColor);
        }
    }

    private void DrawWireSphere(Vector3 center, float radius, Color color)
    {
        float theta = 0;
        float x = radius * Mathf.Cos(theta);
        float y = radius * Mathf.Sin(theta);
        Vector3 pos = center + new Vector3(x, y, 0);
        Vector3 newPos = pos;
        Vector3 lastPos = pos;

        for (theta = 0.1f; theta < Mathf.PI * 2; theta += 0.1f)
        {
            x = radius * Mathf.Cos(theta);
            y = radius * Mathf.Sin(theta);
            newPos = center + new Vector3(x, y, 0);
            Debug.DrawLine(pos, newPos, color);
            pos = newPos;
        }
        Debug.DrawLine(pos, lastPos, color);

        pos = center + new Vector3(x, 0, y);
        newPos = pos;
        lastPos = pos;

        for (theta = 0.1f; theta < Mathf.PI * 2; theta += 0.1f)
        {
            x = radius * Mathf.Cos(theta);
            y = radius * Mathf.Sin(theta);
            newPos = center + new Vector3(x, 0, y);
            Debug.DrawLine(pos, newPos, color);
            pos = newPos;
        }
        Debug.DrawLine(pos, lastPos, color);

        pos = center + new Vector3(0, x, y);
        newPos = pos;
        lastPos = pos;

        for (theta = 0.1f; theta < Mathf.PI * 2; theta += 0.1f)
        {
            x = radius * Mathf.Cos(theta);
            y = radius * Mathf.Sin(theta);
            newPos = center + new Vector3(0, x, y);
            Debug.DrawLine(pos, newPos, color);
            pos = newPos;
        }
        Debug.DrawLine(pos, lastPos, color);
    }

}
