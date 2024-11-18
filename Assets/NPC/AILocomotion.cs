using UnityEngine;
using UnityEngine.AI;
using ECM2;
using System.Collections;

public class AILocomotion : MonoBehaviour
{
    [Header("References")]
    public Transform playerTransform;
    private NavMeshAgent agent;
    private Animator animator;
    private Character characterScript;
    private NPCController npcController;

    [Header("Distance Thresholds")]
    public float walkDistance = 1f;
    public float runDistance = 5f;
    public float sightRange = 15f;
    public LayerMask playerLayer;

    [Header("Patrol Settings")]
    public float patrolWalkSpeed = 3f;
    public float patrolRadius = 20f;
    public float minPatrolWaitTime = 2f;
    public float maxPatrolWaitTime = 5f;
    private Vector3 patrolPoint;
    private bool patrolPointSet;
    private float patrolWaitTimer;

    [Header("Speed Settings")]
    public float walkSpeed = 3.5f;
    public float runSpeed = 8f;

    [Header("Rotation Settings")]
    public float closeRotationSpeed = 720f;
    public float normalRotationSpeed = 420f;
    public float closeDistance = 2f;
    public float facingThreshold = 30f;

    [Header("Reaction Settings")]
    public float reactionDuration = 4f;
    public float reactionTransitionTime = 0.2f;
    private bool isReacting = false;
    private const int REACTION_COUNT = 3;

    [Header("Animation Blend Values")]
    private const float WALK_BLEND_VALUE = 0.5f;
    private const float RUN_BLEND_VALUE = 1.0f;

    private bool isCloseToPlayer;
    private bool isFacingPlayer;
    private bool playerInSightRange;
    private bool shouldSnapToPlayer;

    private float originalStoppingDistance;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        characterScript = GetComponent<Character>();
        npcController = GetComponent<NPCController>();
        originalStoppingDistance = agent.stoppingDistance;

        if (!agent || !animator || !characterScript || !npcController)
        {
            Debug.LogError("Missing required components on " + gameObject.name);
        }
    }

    void Update()
    {
        if (npcController != null && npcController.IsKnockedDown)
        {
            DisableNavigation();
            return;
        }
        else
        {
            EnableNavigation();
        }

        // Don't process movement if currently reacting
        if (isReacting)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            animator.SetFloat("Forward", 0f);
            animator.SetFloat("Turn", 0f);
            return;
        }

        bool previousSightStatus = playerInSightRange;
        playerInSightRange = false;

        if (playerTransform != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer <= sightRange)
            {
                playerInSightRange = true;

                // Simple instant rotation when first seeing player
                if (!previousSightStatus)
                {
                    Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
                    transform.rotation = Quaternion.LookRotation(directionToPlayer);
                }
            }
        }

        if (previousSightStatus && !playerInSightRange)
        {
            OnPlayerLostSight();
            return;
        }

        if (playerInSightRange)
        {
            ChasePlayer();
        }
        else if (!isReacting)
        {
            if (agent.velocity.magnitude < 0.1f)
            {
                Vector3 patrolDirection = (patrolPointSet ? patrolPoint - transform.position : transform.forward);
                transform.rotation = Quaternion.LookRotation(patrolDirection);
            }
            Patrol();
        }
    }


    void OnPlayerLostSight()
    {
        if (!isReacting)
        {
            StartCoroutine(PlayLostSightReaction());
        }
    }

    IEnumerator PlayLostSightReaction()
    {
        isReacting = true;

        // Ensure NPC stops completely
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        animator.SetFloat("Forward", 0f);
        animator.SetFloat("Turn", 0f);

        float randomIndex = Random.Range(0, REACTION_COUNT) / (float)(REACTION_COUNT - 1);
        animator.SetFloat("ReactionIndex", randomIndex);
        animator.SetTrigger("PlayerLost");

        // Wait for the full animation duration
        yield return new WaitForSeconds(reactionDuration);

        // Additional transition time to ensure smooth blend
        yield return new WaitForSeconds(reactionTransitionTime);

        yield return new WaitForSeconds(2f);

        isReacting = false;
        agent.isStopped = false;
    }


    private void DisableNavigation()
    {
        if (agent.enabled)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            agent.enabled = false;
            animator.SetFloat("Forward", 0);
            animator.SetFloat("Turn", 0);
        }
    }

    private void EnableNavigation()
    {
        if (!agent.enabled)
        {
            agent.enabled = true;
            agent.isStopped = false;
        }
    }

    void Patrol()
    {
        agent.stoppingDistance = 0.1f;

        if (!patrolPointSet)
        {
            if (patrolWaitTimer <= 0)
            {
                SearchPatrolPoint();
            }
            else
            {
                // When waiting at patrol point
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
                animator.SetFloat("Forward", 0f);
                animator.SetFloat("Turn", 0f);
                patrolWaitTimer -= Time.deltaTime;
            }
        }
        else
        {
            // Continue to patrol point
            agent.isStopped = false;
            agent.speed = patrolWalkSpeed;
            characterScript.maxWalkSpeed = patrolWalkSpeed;
            agent.destination = patrolPoint;

            // Calculate patrol movement animation
            if (agent.velocity.magnitude > 0.1f)
            {
                animator.SetFloat("Forward", WALK_BLEND_VALUE, 0.1f, Time.deltaTime);

                // Calculate turn amount for patrol
                Vector3 directionToPatrolPoint = (patrolPoint - transform.position).normalized;
                float angleToPatrolPoint = Vector3.SignedAngle(transform.forward, directionToPatrolPoint, Vector3.up);
                float turnAmount = Mathf.Clamp(angleToPatrolPoint / 180f, -1f, 1f);
                animator.SetFloat("Turn", turnAmount, 0.1f, Time.deltaTime);
            }

            float distanceToPoint = Vector3.Distance(transform.position, patrolPoint);
            if (distanceToPoint < 1f)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
                animator.SetFloat("Forward", 0f);
                animator.SetFloat("Turn", 0f);
                patrolPointSet = false;
                patrolWaitTimer = Random.Range(minPatrolWaitTime, maxPatrolWaitTime);
            }
        }
    }

    void SearchPatrolPoint()
    {
        float randomX = Random.Range(-patrolRadius, patrolRadius);
        float randomZ = Random.Range(-patrolRadius, patrolRadius);

        patrolPoint = transform.position + new Vector3(randomX, 0f, randomZ);

        NavMeshHit hit;
        if (NavMesh.SamplePosition(patrolPoint, out hit, patrolRadius, NavMesh.AllAreas))
        {
            patrolPoint = hit.position;
            patrolPointSet = true;
        }
    }

    void ChasePlayer()
    {
        agent.stoppingDistance = originalStoppingDistance;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        isCloseToPlayer = distanceToPlayer <= closeDistance;

        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
        float angleToPlayer = Mathf.Abs(Vector3.SignedAngle(transform.forward, directionToPlayer, Vector3.up));
        isFacingPlayer = angleToPlayer <= facingThreshold;

        if (isFacingPlayer || distanceToPlayer > closeDistance)
        {
            if (isFacingPlayer)
            {
                agent.isStopped = false;
                agent.destination = playerTransform.position;
                HandleMovementSpeed(distanceToPlayer);
            }
        }
        else
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            animator.SetFloat("Forward", 0f);
        }

        HandleRotation(angleToPlayer, directionToPlayer);
    }


    // Optional: If  want a small delay before chasing
    /*
    private void StartChasing()
    {
        if (playerInSightRange)
        {
            agent.isStopped = false;
        }
    }
    */

    void HandleRotation(float angleToPlayer, Vector3 directionToPlayer)
    {
        if (Mathf.Abs(angleToPlayer) > 1f)
        {
            float rotationSpeed = isCloseToPlayer ? closeRotationSpeed : normalRotationSpeed;
            rotationSpeed *= Time.deltaTime;

            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed);

            float turnAmount = Mathf.Clamp(angleToPlayer / 180f, -1f, 1f);
            animator.SetFloat("Turn", turnAmount, 0.15f, Time.deltaTime);
        }
        else
        {
            animator.SetFloat("Turn", 0f, 0.15f, Time.deltaTime);
        }
    }

    void HandleMovementSpeed(float distanceToPlayer)
    {
        if (isCloseToPlayer)
        {
            agent.velocity = Vector3.zero;
            animator.SetFloat("Forward", 0f);
            return;
        }

        float targetSpeed;
        float forwardBlendValue;

        if (distanceToPlayer <= walkDistance)
        {
            targetSpeed = walkSpeed;
            forwardBlendValue = WALK_BLEND_VALUE;
        }
        else if (distanceToPlayer > runDistance)
        {
            targetSpeed = runSpeed;
            forwardBlendValue = RUN_BLEND_VALUE;
        }
        else
        {
            float speedBlend = Mathf.InverseLerp(walkDistance, runDistance, distanceToPlayer);
            targetSpeed = Mathf.Lerp(walkSpeed, runSpeed, speedBlend);
            forwardBlendValue = Mathf.Lerp(WALK_BLEND_VALUE, RUN_BLEND_VALUE, speedBlend);
        }

        agent.speed = targetSpeed;
        characterScript.maxWalkSpeed = targetSpeed;
        animator.SetFloat("Forward", forwardBlendValue, 0.1f, Time.deltaTime);
    }

    void OnDrawGizmosSelected()
    {
        // Draw sight range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);

        // Draw other ranges
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, closeDistance);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, walkDistance);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, runDistance);

        // Draw patrol radius
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, patrolRadius);

        // Draw current patrol point if set
        if (patrolPointSet)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(patrolPoint, 0.5f);
            Gizmos.DrawLine(transform.position, patrolPoint);
        }
    }
}