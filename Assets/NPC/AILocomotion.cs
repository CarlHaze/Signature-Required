using UnityEngine;
using UnityEngine.AI;
using ECM2;

public class AILocomotion : MonoBehaviour
{
    public Transform playerTransform;
    private NavMeshAgent agent;
    private Animator animator;
    private Character characterScript;

    [Header("Distance Thresholds")]
    public float walkDistance = 1f;
    public float runDistance = 5f;

    [Header("Speed Settings")]
    public float walkSpeed = 3.5f;
    public float runSpeed = 5f;

    [Header("Rotation Settings")]
    public float closeRotationSpeed = 720f;
    public float normalRotationSpeed = 120f;
    public float closeDistance = 2f;
    public float facingThreshold = 30f; // Angle within which NPC is considered "facing" the player

    private bool isCloseToPlayer;
    private bool isFacingPlayer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        characterScript = GetComponent<Character>();
    }

    void Update()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        isCloseToPlayer = distanceToPlayer <= closeDistance;

        // Check if facing player
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

        // Update animator parameters
        UpdateAnimator(distanceToPlayer);
    }

    void HandleRotation(float angleToPlayer, Vector3 directionToPlayer)
    {
        // Rotate only if the angle exceeds a threshold
        if (Mathf.Abs(angleToPlayer) > 1f)
        {
            float rotationSpeed = closeRotationSpeed * Time.deltaTime;
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed);

            // Normalize and smooth the turn parameter
            float normalizedTurn = Mathf.Clamp(angleToPlayer / 180f, -1f, 1f);

            // Apply different turn multipliers based on angle severity
            if (Mathf.Abs(angleToPlayer) > 90f)
            {
                normalizedTurn *= 0.75f; // Reduce sharp turns
            }
            else if (Mathf.Abs(angleToPlayer) > 45f)
            {
                normalizedTurn *= 0.5f;
            }

            // Smooth the turn animation
            animator.SetFloat("Turn", normalizedTurn, 0.15f, Time.deltaTime);
        }
        else
        {
            // Smoothly return to zero
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
            // When close, emphasize turning animations
            turnAmount *= 1.5f;

            if (Mathf.Abs(angleToPlayer) > 90f)
            {
                turnAmount *= 2f;
            }
            else if (Mathf.Abs(angleToPlayer) > 45f)
            {
                turnAmount *= 1.5f;
            }

            // Reduce forward movement when turning sharply at close range
            if (Mathf.Abs(angleToPlayer) > 45f)
            {
                forwardAmount *= 0.5f;
            }
        }

        animator.SetFloat("Forward", Mathf.Abs(forwardAmount));
        animator.SetFloat("Turn", turnAmount, 0.1f, Time.deltaTime);
    }

    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, closeDistance);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, walkDistance);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, runDistance);

        // Visualize facing direction
        if (isFacingPlayer)
        {
            Gizmos.color = Color.green;
        }
        else
        {
            Gizmos.color = Color.red;
        }
        Gizmos.DrawRay(transform.position, transform.forward * 2f);
    }
}