using UnityEngine;
using System.Collections.Generic;

public class PunchHitbox : MonoBehaviour
{
    [Header("Punch Settings")]
    [SerializeField] private bool isJabPunch = true; // True for jab, False for straight

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

        // Determine punch type based on object name or parent
        if (gameObject.name.Contains("Left") || transform.parent?.name.Contains("Left") == true)
        {
            isJabPunch = true;
        }
        else if (gameObject.name.Contains("Right") || transform.parent?.name.Contains("Right") == true)
        {
            isJabPunch = false;
        }

        Debug.Log($"PunchHitbox initialized on {gameObject.name}, Parent: {transform.parent?.name}, Type: {(isJabPunch ? "Jab" : "Straight")}");
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

            // Calculate hit position - the point of contact between hitbox and target
            Vector3 hitPosition = other.ClosestPoint(transform.position);

            // Call the enhanced TakeDamage method
            damageable.TakeDamage(damage, hitPosition, isJabPunch);

            Debug.Log($"Hit landed on {other.gameObject.name} for {damage} damage! Punch type: {(isJabPunch ? "Jab" : "Straight")}");
        }
    }

    private void OnDrawGizmos()
    {
        if (hitboxCollider != null)
        {
            // Use different colors for jab and straight punches
            Gizmos.color = isActive ?
                (isJabPunch ? Color.red : Color.blue) :
                (isJabPunch ? new Color(1, 0.5f, 0.5f) : new Color(0.5f, 0.5f, 1));

            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(hitboxCollider.center, hitboxCollider.size);
        }
    }

    // Optional: Public method to manually set punch type if needed
    public void SetPunchType(bool isJab)
    {
        isJabPunch = isJab;
    }
}