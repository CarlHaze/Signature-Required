using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(NavMeshAgent))]
public class EnhancedNPCController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the model GameObject (e.g., X Bot) that has the Animator component")]
    public GameObject characterModel;

    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public float rotationSpeed = 10f;
    public float groundCheckDistance = 0.1f;
    public LayerMask groundLayer;

    [Header("Animation Settings")]
    public float animationBlendSpeed = 0.2f;
    public float locomotionAnimationSpeed = 1f;

    private Rigidbody rb;
    private Animator animator;
    private NavMeshAgent agent;
    private CapsuleCollider capsuleCollider;

    // Animation hashes for 2D blend tree
    private readonly int forwardHash = Animator.StringToHash("Forward");
    private readonly int turnHash = Animator.StringToHash("Turn");
    private readonly int groundedHash = Animator.StringToHash("Grounded");

    private void Start()
    {
        if (characterModel == null)
        {
            Debug.LogError("Character model reference is missing! Please assign the model GameObject (e.g., X Bot) in the inspector.");
            enabled = false;
            return;
        }

        InitializeComponents();
        SetupPhysics();
    }

    private void InitializeComponents()
    {
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        animator = characterModel.GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogError("Animator component not found on the character model!");
            enabled = false;
            return;
        }

        // Configure NavMeshAgent
        agent.speed = moveSpeed;
        agent.angularSpeed = rotationSpeed * 100f; // Convert to degrees
        agent.updatePosition = true;
        agent.updateRotation = true;

        // Configure Rigidbody for kinematic movement since we're using NavMeshAgent
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.None;
        rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
    }

    private void SetupPhysics()
    {
        // Adjust capsule collider to match character dimensions
        capsuleCollider.height = animator.humanScale * 2f;
        capsuleCollider.center = Vector3.up * animator.humanScale;
        capsuleCollider.radius = animator.humanScale * 0.25f;
    }

    private void Update()
    {
        UpdateAnimator();
        HandleGroundDetection();
    }

    private void UpdateAnimator()
    {
        if (agent.hasPath)
        {
            // Convert world velocity to local space relative to character
            Vector3 localVelocity = transform.InverseTransformDirection(agent.velocity);

            // Normalize values based on speed
            float forward = localVelocity.z / agent.speed;
            float turn = localVelocity.x / agent.speed;

            // Update animator parameters
            animator.SetFloat(forwardHash, forward, animationBlendSpeed, Time.deltaTime);
            animator.SetFloat(turnHash, turn, animationBlendSpeed, Time.deltaTime);

            // Debug log to check values
            Debug.Log($"Forward: {forward}, Turn: {turn}, Velocity: {agent.velocity.magnitude}");
        }
        else
        {
            // Return to idle
            animator.SetFloat(forwardHash, 0f, animationBlendSpeed, Time.deltaTime);
            animator.SetFloat(turnHash, 0f, animationBlendSpeed, Time.deltaTime);
        }
    }

    private void HandleGroundDetection()
    {
        bool isGrounded = Physics.Raycast(
            transform.position + Vector3.up * 0.1f,
            Vector3.down,
            groundCheckDistance + 0.1f,
            groundLayer
        );

        animator.SetBool(groundedHash, isGrounded);
    }

    public void SetDestination(Vector3 target)
    {
        if (agent && agent.isOnNavMesh)
        {
            Debug.Log($"Setting destination to: {target}"); // Debug log
            agent.SetDestination(target);
        }
        else
        {
            Debug.LogWarning("Agent not on NavMesh or agent is null!");
        }
    }

    // OnDrawGizmos to visualize the path
    private void OnDrawGizmos()
    {
        if (agent != null && agent.hasPath)
        {
            Gizmos.color = Color.yellow;
            var path = agent.path;
            Vector3 previousCorner = transform.position;

            foreach (var corner in path.corners)
            {
                Gizmos.DrawLine(previousCorner, corner);
                Gizmos.DrawWireSphere(corner, 0.2f);
                previousCorner = corner;
            }
        }
    }
}