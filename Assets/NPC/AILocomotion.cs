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
    public float walkDistance = 1f;    // Distance to start walking
    public float runDistance = 5f;    // Distance to start running

    [Header("Speed Settings")]
    public float walkSpeed = 3.5f;
    public float runSpeed = 5f;

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

        // Update destination
        agent.destination = playerTransform.position;

        // Adjust speed based on distance
        if (distanceToPlayer <= walkDistance)
        {
            agent.speed = walkSpeed;
            characterScript.maxWalkSpeed = walkSpeed;
        }
        else if (distanceToPlayer > runDistance)
        {
            agent.speed = runSpeed;
            characterScript.maxWalkSpeed = runSpeed;
        }
        else
        {
            // Lerp speed between walk and run for smooth transition
            float speedBlend = Mathf.InverseLerp(walkDistance, runDistance, distanceToPlayer);
            float currentSpeed = Mathf.Lerp(walkSpeed, runSpeed, speedBlend);
            agent.speed = currentSpeed;
            characterScript.maxWalkSpeed = currentSpeed;
        }

        // Calculate movement parameters for animation
        Vector3 velocityRelative = transform.InverseTransformDirection(agent.velocity);
        float forwardAmount = velocityRelative.z / runSpeed; // Normalize to max speed

        // Calculate turn amount
        Vector3 directionToTarget = (playerTransform.position - transform.position).normalized;
        float angleToTarget = Vector3.SignedAngle(transform.forward, directionToTarget, Vector3.up);
        float turnAmount = Mathf.Clamp(angleToTarget / 180f, -1f, 1f);

        // Update animator parameters
        animator.SetFloat("Forward", Mathf.Abs(forwardAmount));
        animator.SetFloat("Turn", turnAmount);
    }

    // Optional: Add visualization in the editor
    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, walkDistance);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, runDistance);
    }
}