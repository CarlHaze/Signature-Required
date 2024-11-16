using UnityEngine;
using System.Collections.Generic;

public class PunchHitbox : MonoBehaviour
{
    private BoxCollider hitboxCollider;
    private bool isActive = false;
    private HashSet<Collider> hitColliders = new HashSet<Collider>();
    private float lastHitTime;
    private const float MIN_HIT_INTERVAL = 0.1f; // Minimum time between hits

    private void Awake()
    {
        // Add BoxCollider component if it doesn't exist
        hitboxCollider = gameObject.AddComponent<BoxCollider>();
        hitboxCollider.isTrigger = true;
        hitboxCollider.enabled = false;
        hitboxCollider.size = new Vector3(0.2f, 0.2f, 0.2f);
        Debug.Log($"PunchHitbox initialized on {gameObject.name}, Parent: {transform.parent?.name}");
    }

    public void EnableHitbox()
    {
        isActive = true;
        hitboxCollider.enabled = true;
        hitColliders.Clear(); // Clear the hit list when starting a new punch
        Debug.Log($"Enabled hitbox on {gameObject.name}");
    }

    public void DisableHitbox()
    {
        isActive = false;
        hitboxCollider.enabled = false;
        hitColliders.Clear();
        Debug.Log($"Disabled hitbox on {gameObject.name}");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;
        if (Time.time - lastHitTime < MIN_HIT_INTERVAL) return;
        if (hitColliders.Contains(other)) return;

        Debug.Log($"Trigger entered by {other.gameObject.name} on {gameObject.name}");

        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null && Player.Instance != null)
        {
            hitColliders.Add(other); // Add to hit list
            lastHitTime = Time.time;

            int damage = Player.Instance.Damage;
            damageable.TakeDamage(damage);
            Debug.Log($"Hit landed on {other.gameObject.name} for {damage} damage!");
        }
    }

    private void OnDrawGizmos()
    {
        if (hitboxCollider != null)
        {
            Gizmos.color = isActive ? Color.red : Color.yellow;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(hitboxCollider.center, hitboxCollider.size);
        }
    }
}