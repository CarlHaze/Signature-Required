using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace NPC
{   
    [TaskCategory("Custom")]
    public class Patrol : Action
    {
        public SharedGameObject[] waypoints;
        public SharedFloat speed = 10;
        public SharedFloat angularSpeed = 120;
        public SharedFloat arriveDistance = 0.2f;

        private int currentWaypointIndex;
        private UnityEngine.AI.NavMeshAgent navMeshAgent;

        public override void OnAwake()
        {
            navMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        }

        public override void OnStart()
        {
            navMeshAgent.speed = speed.Value;
            navMeshAgent.angularSpeed = angularSpeed.Value;
            navMeshAgent.isStopped = false;

            currentWaypointIndex = 0;
            SetDestination(waypoints[currentWaypointIndex].Value.transform.position);
        }

        public override TaskStatus OnUpdate()
        {
            if (HasArrived())
            {
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
                SetDestination(waypoints[currentWaypointIndex].Value.transform.position);
            }

            return TaskStatus.Running;
        }

        private bool SetDestination(Vector3 destination)
        {
            navMeshAgent.isStopped = false;
            return navMeshAgent.SetDestination(destination);
        }

        private bool HasArrived()
        {
            float remainingDistance;
            if (navMeshAgent.pathPending)
            {
                remainingDistance = float.PositiveInfinity;
            }
            else
            {
                remainingDistance = navMeshAgent.remainingDistance;
            }

            return remainingDistance <= arriveDistance.Value;
        }

        public override void OnEnd()
        {
            navMeshAgent.isStopped = true;
        }
    }
}