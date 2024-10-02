using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
        float sphereRadius = GetComponent<SphereCollider>().radius;   
        currentState = MonsterState.Roaming;
        sphereRadius = detectionDistance;
    }

    private void Update()
    {
        
        switch(currentState)
        {
            case MonsterState.Investigate:
                Debug.Log("playing walk animation");
                //spil noget animation mens den går til destination fx. animator.Play("Walk");
                break;
            case MonsterState.Idle:
                Debug.Log("playing idle animation");
                //spil noget animation fx. animator.Play("Idle") eller en kigge animation;
                break;
            case MonsterState.Roaming:
                Debug.Log("playing walk animation");
                Roam();
                //spil noget animation fx. animator.Play("walk");
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
        GetComponent<NavMeshAgent>().SetDestination(currentPatrolPoint.position);

        if (Vector3.Distance(currentPatrolPoint.position, transform.position) <= 1)
        {
            currentPatrolPointIndex++;
            if (currentPatrolPointIndex >= patrolPoints.Length)
            {
                currentPatrolPointIndex = 0;
            }
        }


    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") )
        {
            Investigate(other.transform, false, 5);
            StartCoroutine(PatrolWait());

        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && other.GetComponent<TestSneak>().isSneaking == false)
        {
            Investigate(other.transform, true, 1);
            detectedLumi = true;

        }
        if (other.CompareTag("Player") && other.GetComponent<TestSneak>().isWalking == true)
        {
            Investigate(other.transform, true, 1);
        }
        else if (other.CompareTag("Player") && other.GetComponent<TestSneak>().isSneaking == true)
        {
            detectedLumi = false;
        }

    }

    private void Investigate(Transform target, bool lumiDetected, int stoppingDistance)
    {
        GetComponent<NavMeshAgent>().stoppingDistance = stoppingDistance;

        if (lumiDetected == true)
        {
            detectedLumi = true;
            // play sound for suspence
        }
        else if (lumiDetected == false)
        {
            detectedLumi = false;
        }

        StartCoroutine(CheckReachedDestination(stoppingDistance));
        GetComponent<NavMeshAgent>().SetDestination(target.position);
        currentState = MonsterState.Investigate;
        
        if (Vector3.Distance(target.position, killBox.transform.position) <= killBoxRadius && detectedLumi == true)
        {
            Kill();
        }
    }

    private IEnumerator CheckReachedDestination(int stoppingDistance)
    {
       
        while (reached == false)
        {
            if (Vector3.Distance(GetComponent<NavMeshAgent>().destination, transform.position) <= stoppingDistance)
            {
                currentState = MonsterState.Idle;
                reached = true;
            } 
        }
        yield return true;
    }
    private IEnumerator PatrolWait()
    {
        yield return new WaitForSeconds(waitToResumeRoaming);
        GetComponent<NavMeshAgent>().stoppingDistance = 0;
        currentState = MonsterState.Roaming;
    }

    private void Kill()
    {
        // add en hasKilled bool eller noget så vi ikke kommer til at
        // kalde en method i playercontrolleren mega mange gange.
        Debug.Log("Lumi is dead");
        // call method from charactercontroller to kill lumi and maybe call game manager to restart level
    }
}

