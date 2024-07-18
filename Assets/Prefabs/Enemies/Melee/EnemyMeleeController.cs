using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;


[RequireComponent(typeof(NavMeshAgent))]
public class EnemyMeleeController : MonoBehaviour
{
    [SerializeField] GameObject Target;
    [SerializeField] float PathUpdateSpeed = 0.1f;

    [SerializeField] float AttackRange = 2f;
    [SerializeField] float AttackCooldown = 1f;
    [SerializeField] float RunSpeedThreshold = 1.5f;
    [SerializeField] float DetectionRange = 20f;


    private NavMeshAgent navMeshAgent;
    private Animator animator;
    private bool isAttacking = false;
    private bool isWalking = false;
    private bool isRunning = false;
    private bool isInCombat = false;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (!isInCombat && (Target.transform.position - transform.position).magnitude <= DetectionRange) {
            isInCombat = true;
        }
        if (isInCombat) {
            StartCoroutine(FollowTarget());
        }
        UpdateAnimationState();
    }

    private IEnumerator FollowTarget()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(PathUpdateSpeed);
        while (enabled)
        {
            if (!isAttacking)
            {
                navMeshAgent.SetDestination(Target.transform.position);
            }
            yield return waitForSeconds;
        }
    }

    private void UpdateAnimationState()
    {
        if (Target == null) return;

        float distanceToTarget = Vector3.Distance(transform.position, Target.transform.position);

        if (distanceToTarget <= AttackRange && !isAttacking)
        {
            transform.LookAt(Target.transform.position);
            StartCoroutine(AttackTarget());
        }
        else
        {
            UpdateMovementAnimation();
        }
    }


    private void UpdateMovementAnimation()
    {
        float speed = navMeshAgent.velocity.magnitude;

        if (speed > 0.1f)
        {
            if (!isWalking)
            {
                animator.SetBool("Walk", true);
                isWalking = true;
            }

            if (speed > RunSpeedThreshold)
            {
                if (!isRunning)
                {
                    animator.SetBool("Run", true);
                    isRunning = true;
                }
            }
            else if (isRunning)
            {
                animator.SetBool("Run", false);
                isRunning = false;
            }
        }
        else if (isWalking)
        {
            animator.SetBool("Walk", false);
            animator.SetBool("Run", false);
            isWalking = false;
            isRunning = false;
        }
    }

    private IEnumerator AttackTarget()
    {
        isAttacking = true;
        navMeshAgent.isStopped = true;

        animator.SetTrigger("Attack");

        yield return new WaitForSeconds(AttackCooldown);

        isAttacking = false;
        navMeshAgent.isStopped = false;
    }

    public void TakeHit()
    {
        animator.SetTrigger("Hit");
    }

    public void CastSpell()
    {
        animator.SetTrigger("Cast Spell");
    }

    public void Die()
    {
        animator.SetTrigger("Death");
        enabled = false;
        navMeshAgent.enabled = false;
    }
}
