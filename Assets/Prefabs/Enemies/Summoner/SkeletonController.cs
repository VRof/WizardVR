using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SkeletonController : MonoBehaviour
{
    [SerializeField] float hp;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float pathUpdateTime = 0.1f;
    [SerializeField] private LayerMask obstacleLayer; //Layer for obstacles

    private Transform target;
    private NavMeshAgent agent;
    private Animator animator;
    private bool isAttacking = false;
    private float lastAttackTime = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        target = GameObject.Find("PlayerModel").transform;
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        if (distanceToTarget <= attackRange && !isAttacking && Time.time >= lastAttackTime + attackCooldown)
        {
            if (CanSeeTarget())
            {
                StopMoving();
                StartCoroutine(Attack());
            }
        }
        else if (!isAttacking)
        {
            MoveTowardsTarget();
        }
    }

    private bool CanSeeTarget()
    {
        Vector3 directionToTarget = (target.position - transform.position).normalized;
        if (Physics.Raycast(transform.position, directionToTarget, out RaycastHit hit, attackRange, obstacleLayer))
        {
            return hit.transform == target;
        }
        return true;
    }

    private void StopMoving()
    {
        agent.SetDestination(transform.position);
        animator.SetBool("isRunning", false);
    }

    private void MoveTowardsTarget()
    {
        agent.SetDestination(target.position);
        animator.SetBool("isRunning", true);
    }

    private IEnumerator Attack()
    {
        isAttacking = true;
        animator.SetBool("isRunning", false);
        animator.SetTrigger("isAttacking");
        lastAttackTime = Time.time;

        // Wait for the attack animation to finish
        yield return new WaitForSeconds(attackCooldown);

        isAttacking = false;
    }
}