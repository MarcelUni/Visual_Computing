using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class MonsterController : MonoBehaviour
{
    [Header("Monster Settings")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float waitToResumeRoaming = 5f;
    [SerializeField] private float killRadius = 5f;

    [SerializeField] private float viewRadius;
    [SerializeField] private float viewAngle;
    [SerializeField] private LayerMask playerLayer;

    private Animator animator;
    private NavMeshAgent agent;
    private Transform playerTransform;
    private Vector3 lastKnowPos;

    [Header("Monster State settings")]
    public MonsterState currentState;
    public Transform[] patrolPoints;

    private bool detectedLumi = false;
    private int currentPatrolPointIndex = 0;

    public enum MonsterState
    {
        Idle,
        Roaming,
        Investigate
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>(); // Cache the NavMeshAgent
        agent.speed = this.speed;

        currentState = MonsterState.Roaming;
    }

    private void Update()
    {
        // If the player is in view
        if(InView())
        {
            // check if the player is sneaking and set the new target if not
            if(playerTransform.GetComponent<PlayerController>().isSneaking == false)
            {
                currentState = MonsterState.Investigate;
                lastKnowPos = playerTransform.position;
                detectedLumi = true;
            }
            else if(playerTransform.GetComponent<PlayerController>().isSneaking == true && lastKnowPos != Vector3.zero)
            {
                currentState = MonsterState.Investigate;
                detectedLumi = false;
            }
        }
        else
        {
            detectedLumi = false;
        }

        // Handle state behavior
        switch (currentState)
        {
            case MonsterState.Investigate:
                Investigate(lastKnowPos, 1);
                break;

            case MonsterState.Idle:

                break;

            case MonsterState.Roaming:
                Debug.Log("Roaming, playing walk animation");
                Roam();
                break;
        }
    }


    private void Roam()
    {
        if (patrolPoints.Length == 0)
        {
            Debug.LogError("No patrol points found");
            return;
        }
        Transform currentPatrolPoint = patrolPoints[currentPatrolPointIndex];
        agent.SetDestination(currentPatrolPoint.position);
        animator.SetBool("Investigating", false); 


        animator.SetBool("IsMoving", true);

        if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
        {
            // Switch to the next patrol point once reached
            currentPatrolPointIndex++;
            if (currentPatrolPointIndex >= patrolPoints.Length)
            {
                currentPatrolPointIndex = 0;
            }
        }
    }


    private void Investigate(Vector3 target, int stoppingDistance)
    {
        agent.SetDestination(target);

        // Check if the monster has reached its destination, and transition to idle if so.
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            animator.SetBool("IsMoving", false);
            StartCoroutine(InvestigateWait());
        }
        else
        {
            agent.stoppingDistance = stoppingDistance;
            animator.SetBool("IsMoving", true);
        }

        // Check for proximity to kill the player
        if (Vector3.Distance(target, transform.position) <= killRadius && detectedLumi)
        {
            Kill();
        }
    }

    private IEnumerator InvestigateWait()
    {
        yield return new WaitForSeconds(waitToResumeRoaming);
        lastKnowPos = Vector3.zero;
        currentState = MonsterState.Roaming; // Resume patrol after waiting
    }

    private bool killOnce = false;
    private void Kill()
    {
        animator.SetBool("Investigating", false); 
        if(killOnce == false)
        {
            killOnce = true;
            animator.SetTrigger("Attack");

            playerTransform.GetComponent<PlayerController>().isDead = true;

            currentState = MonsterState.Idle;
        }
    }

     private bool InView()
    {
        Collider[] targets = Physics.OverlapSphere(transform.position, viewRadius, playerLayer);
        
        foreach(Collider obj in targets)
        {
            Vector3 directionToTarget = (obj.transform.position - transform.position).normalized;
            Vector3 forward = transform.forward;
            
            // Calculating angle formula
            float dotProduct = Vector3.Dot(directionToTarget, forward);

            float angleInRadians = Mathf.Acos(dotProduct);

            float angleInDegrees = angleInRadians * Mathf.Rad2Deg;
            
            if(Mathf.Abs(angleInDegrees) <= viewAngle)    
            {
                if(Physics.Raycast(transform.position, directionToTarget, out RaycastHit hit, viewRadius))
                {
                    if(hit.collider == obj)
                    {
                        playerTransform = hit.collider.gameObject.transform;
                        return true;
                    }
                }
            }

        }
        return false;
    }
    private void OnDrawGizmos()
    {
        // Draw the detection range as a wire sphere
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        // Draw the 90-degree field of view
        Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle, 0) * transform.forward * viewRadius;
        Vector3 rightBoundary = Quaternion.Euler(0, viewAngle, 0) * transform.forward * viewRadius;

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
    }
}
