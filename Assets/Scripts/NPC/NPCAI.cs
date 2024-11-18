using UnityEngine;
using UnityEngine.AI;
using ECM2;

public class NPCAI : MonoBehaviour
{
    private NavMeshAgent _navMeshAgent;
    private Character _character;
    public Transform target;

    // Movement speed modifier
    public float movementSpeed = 5f;

    private void Start()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _character = GetComponent<Character>();

        // Disable NavMeshAgent's default movement
        _navMeshAgent.updatePosition = false;
        _navMeshAgent.updateRotation = false;
    }

    private void Update()
    {
        if (target == null)
            return;

        // Set the destination for NavMesh pathfinding
        _navMeshAgent.SetDestination(target.position);

        // Get movement direction from NavMesh Agent
        Vector3 movementDirection = _navMeshAgent.desiredVelocity.normalized;

        // Set character's movement direction
        _character.SetMovementDirection(movementDirection);

        // Handle rotation
        if (_navMeshAgent.remainingDistance > _navMeshAgent.stoppingDistance)
        {
            transform.rotation = Quaternion.LookRotation(movementDirection);
        }
    }
}