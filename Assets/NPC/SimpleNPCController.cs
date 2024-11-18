using UnityEngine;

public class SimpleNPCController : MonoBehaviour
{
    public GameObject characterModel;
    public Transform[] waypoints;
    public float moveSpeed = 5f;
    public float rotationSpeed = 5f;
    public float waypointRadius = 0.5f;
    public bool loopWaypoints = true;

    private Rigidbody rb;
    private Animator animator;
    private int currentWaypoint = 0;

    // Animation hashes
    private readonly int forwardHash = Animator.StringToHash("Forward");
    private readonly int turnHash = Animator.StringToHash("Turn");

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = characterModel.GetComponent<Animator>();

        // Configure rigidbody
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        if (waypoints.Length == 0)
        {
            Debug.LogWarning("No waypoints assigned!");
            enabled = false;
        }
    }

    private void FixedUpdate()
    {
        if (waypoints.Length == 0) return;

        Vector3 targetPosition = waypoints[currentWaypoint].position;
        // Keep y position constant
        targetPosition.y = transform.position.y;

        // Get direction to target
        Vector3 directionToTarget = (targetPosition - transform.position).normalized;

        // Calculate distance to waypoint
        float distanceToWaypoint = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z),
                                                   new Vector3(targetPosition.x, 0, targetPosition.z));

        // If we've reached the waypoint
        if (distanceToWaypoint < waypointRadius)
        {
            // Move to next waypoint
            if (loopWaypoints)
            {
                currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
            }
            else if (currentWaypoint < waypoints.Length - 1)
            {
                currentWaypoint++;
            }
        }
        else
        {
            // Move towards waypoint
            Vector3 movement = directionToTarget * moveSpeed;
            // Keep y velocity to maintain proper ground contact
            movement.y = rb.linearVelocity.y;
            rb.linearVelocity = movement;

            // Rotate towards movement direction
            if (directionToTarget != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));
            }
        }

        // Update animator
        UpdateAnimator();
    }

    private void UpdateAnimator()
    {
        // Calculate forward movement based on velocity
        float forwardSpeed = Vector3.Dot(rb.linearVelocity.normalized, transform.forward);
        animator.SetFloat(forwardHash, forwardSpeed, 0.1f, Time.deltaTime);

        // Calculate turning based on angular velocity
        float turn = Vector3.Dot(rb.linearVelocity.normalized, transform.right);
        animator.SetFloat(turnHash, turn, 0.1f, Time.deltaTime);
    }

    private void OnDrawGizmos()
    {
        if (waypoints == null) return;

        // Draw waypoints
        foreach (var waypoint in waypoints)
        {
            if (waypoint == null) continue;

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(waypoint.position, waypointRadius);
        }

        // Draw lines between waypoints
        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            if (waypoints[i] == null || waypoints[i + 1] == null) continue;

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
        }

        // Draw line from last to first waypoint if looping
        if (loopWaypoints && waypoints.Length > 1 && waypoints[0] != null && waypoints[waypoints.Length - 1] != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(waypoints[waypoints.Length - 1].position, waypoints[0].position);
        }
    }
}