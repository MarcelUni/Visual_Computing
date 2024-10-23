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
                Investigate(playerTransform, 1);
                detectedLumi = true;
            }
            else if(playerTransform.GetComponent<PlayerController>().isSneaking)
            {
                detectedLumi = false;
            }
        }
        else
        {
            detectedLumi = false;
        }

        // Check if the monster has reached its destination, and transition to idle if so.
        if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
        {
            // When the monster has reached the destination, set state to Idle
            if (currentState == MonsterState.Investigate)
            {
                InvestigatingAtLastKnowPos();
            }
        }

        // Handle state behavior
        switch (currentState)
        {
            case MonsterState.Investigate:
                Debug.Log("Investigating, playing walk animation");
               // animator.Play("Walk"); // Play walk animation while investigating
                break;
            case MonsterState.Idle:
                Debug.Log("Idle, playing idle animation");
               // animator.Play("Idle"); // Play idle animation
                break;
            case MonsterState.Roaming:
                Debug.Log("Roaming, playing walk animation");
                Roam();
               // animator.Play("Walk"); // Play walk animation while roaming
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


    private void Investigate(Transform target, int stoppingDistance)
    {
        agent.stoppingDistance = stoppingDistance;
        agent.SetDestination(target.position);
        currentState = MonsterState.Investigate;

        // Check for proximity to kill the player
        if (Vector3.Distance(target.position, transform.position) <= killRadius && detectedLumi)
        {
            Kill();
        }
    }

    // Switch to idle state and handle logic for being idle
    private void InvestigatingAtLastKnowPos()
    {
        currentState = MonsterState.Idle;
        animator.Play("Idle"); // Ensure Idle animation is triggered
        Debug.Log("Monster switched to Idle state");
        StartCoroutine(InvestigateWait()); // After waiting, resume patrol
    }

    private IEnumerator InvestigateWait()
    {
        yield return new WaitForSeconds(waitToResumeRoaming);
        currentState = MonsterState.Roaming; // Resume patrol after waiting
        Debug.Log("Resuming roaming after idle");
    }

    private void Kill()
    {
        Debug.Log("Lumi is dead");
        // Call method from charactercontroller to kill Lumi or restart the level
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
