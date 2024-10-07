using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MonsterController : MonoBehaviour
{
    [Header("Monster Settings")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float detectionDistance = 20f;
    [SerializeField] private float waitToResumeRoaming = 5f;
    [SerializeField] private GameObject killBox;
    [SerializeField] private float killBoxRadius = 5f;

    private Animator animator;
    private TestSneak testSneak;
    private NavMeshAgent agent;

    [Header("Monster State settings")]
    public MonsterState currentState;

    public Transform[] patrolPoints;

    public bool detectedLumi = false;
    public bool gotLatestPosition = false;
    public bool reached = false;

    public enum MonsterState
    {
        Idle,
        Roaming,
        Investigate
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
        testSneak = GetComponent<TestSneak>();
        agent = GetComponent<NavMeshAgent>(); // Cache the NavMeshAgent

        float sphereRadius = GetComponent<SphereCollider>().radius;
        currentState = MonsterState.Roaming;
        sphereRadius = detectionDistance;
    }

    private void Update()
    {
        // Check if the monster has reached its destination, and transition to idle if so.
        if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
        {
            // When the monster has reached the destination, set state to Idle
            if (currentState == MonsterState.Investigate)
            {
                SwitchToIdle();
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

    private int currentPatrolPointIndex = 0;

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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Investigate(other.transform, false, 5);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && other.GetComponent<PlayerController>().isSneaking == false)
        {
            Investigate(other.transform, true, 1);
            detectedLumi = true;
        }
        else if (other.CompareTag("Player") && other.GetComponent<PlayerController>().isMoving)
        {
            Investigate(other.transform, true, 1);
        }
        else if (other.CompareTag("Player") && other.GetComponent<PlayerController>().isSneaking)
        {
            detectedLumi = false;
        }
    }

    private void Investigate(Transform target, bool lumiDetected, int stoppingDistance)
    {
        agent.stoppingDistance = stoppingDistance;

        if (lumiDetected)
        {
            detectedLumi = true;
        }
        else
        {
            detectedLumi = false;
        }

        agent.SetDestination(target.position);
        currentState = MonsterState.Investigate;

        // Check for proximity to kill the player
        if (Vector3.Distance(target.position, killBox.transform.position) <= killBoxRadius && detectedLumi)
        {
            Kill();
        }
    }

    // Switch to idle state and handle logic for being idle
    private void SwitchToIdle()
    {
        currentState = MonsterState.Idle;
        animator.Play("Idle"); // Ensure Idle animation is triggered
        Debug.Log("Monster switched to Idle state");
        StartCoroutine(IdleWait()); // After waiting, resume patrol
    }

    private IEnumerator IdleWait()
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
}
