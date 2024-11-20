using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class MonsterController : MonoBehaviour
{
    [Header("Monster Settings")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float waitToResumeRoaming = 5f;
    [SerializeField] private float killRadius = 5f;
    [SerializeField] private float attackRadius = 2f;
    [SerializeField] private int stoppingDistance = 1;
    [SerializeField] private float viewRadius;
    [SerializeField] private float viewAngle;
    [SerializeField] private LayerMask playerLayer;

    private Animator animator;
    private NavMeshAgent agent;
    private Transform playerTransform;
    private Vector3 lastKnownPos;

    [Header("Monster State Settings")]
    public MonsterState currentState;
    public Transform[] patrolPoints;
    private bool detectedLumi = false;
    private int currentPatrolPointIndex = 0;
    [SerializeField] private AudioSource MonsterIdleSound;

    [Header("Proximity Effects")]
    [SerializeField] private Volume postProcessingVolume; // Reference to Post-Processing Volume
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float vignetteMaxIntensity = 0.5f;
    [SerializeField] private float vignetteEaseSpeed = 1f;

    private Vignette vignetteEffect;
    private Coroutine vignetteEaseCoroutine;
    private Coroutine cameraShakeCoroutine;

    public enum MonsterState
    {
        Idle,
        Roaming,
        Investigate
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        agent.speed = speed;

        currentState = MonsterState.Idle;

        if (postProcessingVolume != null && postProcessingVolume.profile.TryGet<Vignette>(out vignetteEffect))
        {
            vignetteEffect.intensity.Override(0); // Start with no vignette
        }
    }

    private void Update()
    {
        // Update proximity effects regardless of view angle
        UpdateProximityEffects();

        // Check if the player is in view
        if (InView())
        {
            // check if the player is sneaking and set the new target if not
            if (playerTransform.GetComponent<PlayerController>().isSneaking == false)
            {
                currentState = MonsterState.Investigate;
                lastKnownPos = playerTransform.position;
                detectedLumi = true;
            }
            else if (playerTransform.GetComponent<PlayerController>().isSneaking == true && lastKnownPos != Vector3.zero)
            {
                currentState = MonsterState.Investigate;
                detectedLumi = false;
            }
        }

        // Handle state behavior
        switch (currentState)
        {
            case MonsterState.Investigate:
                MonsterIdleSound.Stop();
                Investigate(lastKnownPos, stoppingDistance);
                
                break;
            case MonsterState.Idle:
                if (!MonsterIdleSound.isPlaying)
                {
                    MonsterIdleSound.Play();
                }
                if (patrolPoints != null && patrolPoints.Length > 1) // bare for lvl 3 så man kan give den en patrol point men bare står stille uden at gøre noget (Den resetter ikke efter investigate)
                {
                    currentState = MonsterState.Roaming;
                }
                else
                {
                    animator.SetBool("IsMoving", false);
                }
                break;
            case MonsterState.Roaming:
                if (!MonsterIdleSound.isPlaying)
                {
                    MonsterIdleSound.Play();
                }

                Roam();
                break;
        }
    }

    private void UpdateProximityEffects()
    {
        if (playerTransform == null)
            return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        if (distanceToPlayer <= viewRadius)
        {
            TriggerProximityEffects(distanceToPlayer);
        }
        else
        {
            ResetProximityEffects();
        }
    }

    private void Roam()
    {
        if (patrolPoints.Length == 0) return;

        Transform currentPatrolPoint = patrolPoints[currentPatrolPointIndex];
        agent.SetDestination(currentPatrolPoint.position);

        animator.SetBool("IsMoving", true);

        if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
        {
            currentPatrolPointIndex = (currentPatrolPointIndex + 1) % patrolPoints.Length;
        }
    }

    private void Investigate(Vector3 target, int stoppingDistance)
    {
        agent.SetDestination(target);

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

        if (Vector3.Distance(target, transform.position) <= killRadius && detectedLumi)
        {
            Kill();
        }
        if (Vector3.Distance(playerTransform.position, transform.position) <= attackRadius && !detectedLumi)
        {
            Kill();
        }
    }

    private IEnumerator InvestigateWait()
    {
        yield return new WaitForSeconds(waitToResumeRoaming);
        lastKnownPos = Vector3.zero;
        currentState = MonsterState.Roaming;
    }

    private bool killOnce = false;
    private void Kill()
    {
        animator.SetBool("Investigating", false);

        if (killOnce == false)
        {
            killOnce = true;
            animator.SetTrigger("Attack");

            playerTransform.GetComponent<PlayerController>().isDead = true;
            playerTransform.GetComponent<PlayerController>().deathEvent?.Invoke();

            currentState = MonsterState.Idle;
        }
    }

    private bool InView()
    {
        Collider[] targets = Physics.OverlapSphere(transform.position, viewRadius, playerLayer);

        foreach (Collider obj in targets)
        {
            Vector3 directionToTarget = (obj.transform.position - transform.position).normalized;
            Vector3 forward = transform.forward;

            // Calculating angle formula
            float dotProduct = Vector3.Dot(directionToTarget, forward);

            float angleInRadians = Mathf.Acos(dotProduct);

            float angleInDegrees = angleInRadians * Mathf.Rad2Deg;

            if (Mathf.Abs(angleInDegrees) <= viewAngle)
            {
                if (Physics.Raycast(transform.position, directionToTarget, out RaycastHit hit, viewRadius))
                {
                    if (hit.collider == obj)
                    {
                        playerTransform = hit.collider.gameObject.transform;
                        return true;
                    }
                }
            }

        }
        return false;
    }

    private void TriggerProximityEffects(float distance)
    {
        float normalizedDistance = Mathf.Clamp01(1 - (distance / viewRadius));

        // Adjust vignette intensity
        float targetVignetteIntensity = vignetteMaxIntensity * normalizedDistance;
        if (vignetteEaseCoroutine != null) StopCoroutine(vignetteEaseCoroutine);
        vignetteEaseCoroutine = StartCoroutine(EaseVignette(targetVignetteIntensity));
    }

    private void ResetProximityEffects()
    {
        if (vignetteEaseCoroutine != null) StopCoroutine(vignetteEaseCoroutine);
        vignetteEaseCoroutine = StartCoroutine(EaseVignette(0));
    }

    private IEnumerator EaseVignette(float targetIntensity)
    {
        float startIntensity = vignetteEffect.intensity.value;
        float elapsedTime = 0f;

        while (!Mathf.Approximately(vignetteEffect.intensity.value, targetIntensity))
        {
            elapsedTime += Time.deltaTime * vignetteEaseSpeed;
            vignetteEffect.intensity.Override(Mathf.Lerp(startIntensity, targetIntensity, elapsedTime));
            yield return null;
        }
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
