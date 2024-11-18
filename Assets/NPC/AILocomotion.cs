using UnityEngine;
using UnityEngine.AI;

public class AILocomotion : MonoBehaviour
{
    public Transform playerTransform;
    NavMeshAgent agent;
    Animator animator;

    // Thresholds for different movement states
    private const float runThreshold = 0.9f;
    private const float walkThreshold = 0.4f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        agent.destination = playerTransform.position;

        // Calculate forward movement and turn values
        Vector3 velocityRelative = transform.InverseTransformDirection(agent.velocity);
        float forwardAmount = velocityRelative.z / agent.speed; // Normalized forward speed

        // Calculate turn amount based on the difference between current rotation and desired rotation
        Vector3 directionToTarget = (playerTransform.position - transform.position).normalized;
        float angleToTarget = Vector3.SignedAngle(transform.forward, directionToTarget, Vector3.up);
        float turnAmount = Mathf.Clamp(angleToTarget / 180f, -1f, 1f);

        // Set animator parameters
        animator.SetFloat("Forward", Mathf.Abs(forwardAmount));
        animator.SetFloat("Turn", turnAmount);
    }
}