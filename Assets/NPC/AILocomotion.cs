using UnityEngine;
using UnityEngine.AI;
using ECM2;

public class AILocomotion : MonoBehaviour
{
    [Header("References")]
    public Transform playerTransform;
    private NavMeshAgent agent;
    private Animator animator;
    private Character characterScript;

    [Header("Distance Thresholds")]
    public float walkDistance = 1f;
    public float runDistance = 5f;
    public float sightRange = 15f;     // Distance to detect player
    public LayerMask playerLayer;      // Layer for player detection

    [Header("Patrol Settings")]
    public float patrolWalkSpeed = 2f;
    public float patrolRadius = 10f;   // Range for random patrol points
    public float minPatrolWaitTime = 2f;
    public float maxPatrolWaitTime = 5f;
    private Vector3 patrolPoint;
    private bool patrolPointSet;
    private float patrolWaitTimer;

    [Header("Speed Settings")]
    public float walkSpeed = 3.5f;
    public float runSpeed = 5f;

    [Header("Rotation Settings")]
    public float closeRotationSpeed = 720f;
    public float normalRotationSpeed = 120f;
    public float closeDistance = 2f;
    public float facingThreshold = 30f;

    private bool isCloseToPlayer;
    private bool isFacingPlayer;
    private bool playerInSightRange;

    private float originalStoppingDistance; // Store the original stopping distance

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        characterScript = GetComponent<Character>();
        originalStoppingDistance = agent.stoppingDistance; // Store original distance
    }

    void Update()
    {
        // Simple distance check for sight range
        playerInSightRange = false;
        if (playerTransform != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            playerInSightRange = distanceToPlayer <= sightRange;
        }

        // Only chase if player is in sight range, otherwise patrol
        if (playerInSightRange)
        {
            ChasePlayer();
        }
        else
        {
            // Reset facing when losing sight of player
            if (agent.velocity.magnitude < 0.1f)
            {
                Vector3 patrolDirection = (patrolPointSet ? patrolPoint - transform.position : transform.forward);
                transform.rotation = Quaternion.LookRotation(patrolDirection);
            }
            Patrol();
        }

        // Update animator
        UpdateAnimator(agent.velocity.magnitude);
    }

    void Patrol()
    {
        // Set stopping distance to smaller value for patrol
        agent.stoppingDistance = 0.1f;

        if (!patrolPointSet)
        {
            if (patrolWaitTimer <= 0)
            {
                SearchPatrolPoint();
            }
            else
            {
                // When waiting at patrol point, just go to idle
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
                animator.SetFloat("Forward", 0);
                animator.SetFloat("Turn", 0);
                patrolWaitTimer -= Time.deltaTime;
            }
        }
        else
        {
            // Continue to patrol point
            agent.isStopped = false;
            agent.speed = patrolWalkSpeed;
            agent.destination = patrolPoint;

            float distanceToPoint = Vector3.Distance(transform.position, patrolPoint);
            if (distanceToPoint < 1f)
            {
                // Set idle and wait
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
                animator.SetFloat("Forward", 0);
                animator.SetFloat("Turn", 0);
                patrolPointSet = false;
                patrolWaitTimer = Random.Range(minPatrolWaitTime, maxPatrolWaitTime);
                Debug.Log("Waiting for " + patrolWaitTimer + " seconds");
            }
        }
    }

    void SearchPatrolPoint()
    {
        // Calculate random point in range
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
        // Restore original stopping distance for player chase
        agent.stoppingDistance = originalStoppingDistance;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        isCloseToPlayer = distanceToPlayer <= closeDistance;

        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
        float angleToPlayer = Mathf.Abs(Vector3.SignedAngle(transform.forward, directionToPlayer, Vector3.up));
        isFacingPlayer = angleToPlayer <= facingThreshold;

        // Handle rotation first
        HandleRotation(angleToPlayer, directionToPlayer);

        // Only move if facing player or far enough away
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
        }
    }

    void HandleRotation(float angleToPlayer, Vector3 directionToPlayer)
    {
        if (Mathf.Abs(angleToPlayer) > 1f)
        {
            float rotationSpeed = closeRotationSpeed * Time.deltaTime;
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed);

            float normalizedTurn = Mathf.Clamp(angleToPlayer / 180f, -1f, 1f);

            if (Mathf.Abs(angleToPlayer) > 90f)
            {
                normalizedTurn *= 0.75f;
            }
            else if (Mathf.Abs(angleToPlayer) > 45f)
            {
                normalizedTurn *= 0.5f;
            }

            animator.SetFloat("Turn", normalizedTurn, 0.15f, Time.deltaTime);
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
            // When at stopping distance, don't move toward player
            agent.velocity = Vector3.zero;
            return;
        }

        float targetSpeed;
        if (distanceToPlayer <= walkDistance)
        {
            targetSpeed = walkSpeed;
        }
        else if (distanceToPlayer > runDistance)
        {
            targetSpeed = runSpeed;
        }
        else
        {
            float speedBlend = Mathf.InverseLerp(walkDistance, runDistance, distanceToPlayer);
            targetSpeed = Mathf.Lerp(walkSpeed, runSpeed, speedBlend);
        }

        agent.speed = targetSpeed;
        characterScript.maxWalkSpeed = targetSpeed;
    }

    void UpdateAnimator(float distanceToPlayer)
    {
        Vector3 velocityRelative = transform.InverseTransformDirection(agent.velocity);
        float forwardAmount = velocityRelative.z / runSpeed;

        // Calculate turn amount
        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
        float angleToPlayer = Vector3.SignedAngle(transform.forward, directionToPlayer, Vector3.up);
        float turnAmount = Mathf.Clamp(angleToPlayer / 180f, -1f, 1f);

        if (isCloseToPlayer)
        {
            turnAmount *= 1.5f;
            if (Mathf.Abs(angleToPlayer) > 90f)
            {
                turnAmount *= 2f;
            }
            else if (Mathf.Abs(angleToPlayer) > 45f)
            {
                turnAmount *= 1.5f;
            }
        }

        animator.SetFloat("Forward", Mathf.Abs(forwardAmount));
        animator.SetFloat("Turn", turnAmount, 0.1f, Time.deltaTime);

        // If completely stopped, ensure we're in idle
        if (agent.velocity.magnitude < 0.1f)
        {
            animator.SetFloat("Forward", 0);
            animator.SetFloat("Turn", 0);
        }
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
