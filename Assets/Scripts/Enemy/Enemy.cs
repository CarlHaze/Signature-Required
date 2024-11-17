using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour, IDamageable
{
    public int maxHealth = 100;
    [SerializeField] private int currentHealth;

    [Header("Components")]
    private Animator animator;
    [SerializeField] private GameObject characterModel; // Reference to the X Bot model
    private Rigidbody rb;
    private CapsuleCollider capsuleCollider;

    [Header("Animation Parameters")]
    private readonly int hitTriggerHash = Animator.StringToHash("Hit");
    private readonly int hitDirectionXHash = Animator.StringToHash("HitDirectionX");
    private readonly int hitDirectionZHash = Animator.StringToHash("HitDirectionZ");
    private readonly int hitIntensityHash = Animator.StringToHash("HitIntensity");

    [Header("Hit Reaction Settings")]
    [SerializeField] private float hitRecoveryTime = 0.5f;
    [SerializeField] private bool canBeHitWhileReacting = false;
    private bool isReacting = false;
    private float hitReactionTimer = 0f;

    [Header("Knockdown Settings")]
    [SerializeField] private float knockdownDuration = 2f;
    [SerializeField] private float recoverySpeed = 1.5f;
    private readonly int knockdownTriggerHash = Animator.StringToHash("KnockDown");
    private readonly int getUpTriggerHash = Animator.StringToHash("GetUp");
    private readonly int isKnockedDownHash = Animator.StringToHash("IsKnockedDown");
    private readonly int knockdownIndexHash = Animator.StringToHash("KnockdownIndex");
    private bool isKnockedDown = false;
    private Coroutine knockdownCoroutine;

    [Header("Physics Settings")]
    [SerializeField] private float groundCheckDistance = 2f;
    [SerializeField] private LayerMask groundLayer = -1; // Default to all layers

    private void Start()
    {
        currentHealth = maxHealth;

        // Get reference to existing Rigidbody
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("No Rigidbody found on NPC parent object! Please add a Rigidbody component.");
            return;
        }

        // Find the character model if not assigned
        if (characterModel == null)
        {
            characterModel = GetComponentInChildren<Animator>()?.gameObject;
            if (characterModel == null)
            {
                Debug.LogError("No character model with Animator found in children of " + gameObject.name);
                return;
            }
        }

        // Get the animator from the character model
        animator = characterModel.GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("No Animator component found on character model: " + characterModel.name);
            return;
        }

        // Get the capsule collider
        capsuleCollider = GetComponent<CapsuleCollider>();
        if (capsuleCollider == null)
        {
            Debug.LogError("No CapsuleCollider found on NPC parent object! Please add a CapsuleCollider component.");
            return;
        }

        ValidateAnimatorParameters();
        Debug.Log($"Enemy initialized with {currentHealth} health. Using animator on {characterModel.name}");
    }

    private void ValidateAnimatorParameters()
    {
        if (animator != null)
        {
            // Validate all animator parameters exist
            foreach (var parameter in animator.parameters)
            {
                if (parameter.name == "Hit" || parameter.name == "HitDirectionX" ||
                    parameter.name == "HitDirectionZ" || parameter.name == "HitIntensity" ||
                    parameter.name == "Knockdown" || parameter.name == "GetUp" ||
                    parameter.name == "IsKnockedDown")
                {
                    continue;
                }
                Debug.LogWarning($"Missing animator parameter: {parameter.name} on {gameObject.name}");
            }
        }
    }

    private void Update()
    {
        if (isReacting)
        {
            hitReactionTimer += Time.deltaTime;
            if (hitReactionTimer >= hitRecoveryTime)
            {
                ResetHitReaction();
            }
        }
    }

    public void TakeDamage(int damage, Vector3 hitPosition, bool isJab)
    {
        if (isKnockedDown || (!canBeHitWhileReacting && isReacting))
            return;

        currentHealth -= damage;
        Debug.Log($"Enemy took {damage} damage! Current health: {currentHealth}");

        if (currentHealth <= 0)
        {
            TriggerKnockdown(hitPosition, 10f); // Example knockback force
        }
        else
        {
            PlayHitReaction(hitPosition, isJab);
        }
    }

    private void PlayHitReaction(Vector3 hitPosition, bool isJab)
    {
        if (animator == null)
        {
            Debug.LogError("No animator found for hit reaction!");
            return;
        }

        isReacting = true;
        hitReactionTimer = 0f;

        // Convert world hit position to local direction
        Vector3 localHitDir = transform.InverseTransformPoint(hitPosition).normalized;

        // Set blend tree parameters
        animator.SetFloat(hitDirectionXHash, localHitDir.x);
        animator.SetFloat(hitDirectionZHash, localHitDir.z);
        animator.SetFloat(hitIntensityHash, isJab ? 0f : 1f); // 0 for light (jab), 1 for heavy (straight)
        animator.SetTrigger(hitTriggerHash);

        Debug.Log($"Hit reaction played - Direction: ({localHitDir.x:F2}, {localHitDir.z:F2}), Type: {(isJab ? "Jab" : "Straight")}");
    }

    private void ResetHitReaction()
    {
        isReacting = false;
    }

    public void TriggerKnockdown(Vector3 knockbackDirection, float knockbackForce)
    {
        if (isKnockedDown) return;

        // Stop any existing knockdown sequence
        if (knockdownCoroutine != null)
            StopCoroutine(knockdownCoroutine);

        knockdownCoroutine = StartCoroutine(ImprovedKnockdownSequence(knockbackDirection, knockbackForce));
    }

    private IEnumerator ImprovedKnockdownSequence(Vector3 knockbackDirection, float knockbackForce)
    {
        isKnockedDown = true;
        animator.SetBool(isKnockedDownHash, true);
        animator.SetTrigger(knockdownTriggerHash);

        // Set blend tree parameter
        animator.SetFloat(knockdownIndexHash, Random.Range(0, 2));

        // Instantly restore health at start of knockdown
        currentHealth = maxHealth;

        // Apply knockback force with upward component
        Vector3 adjustedKnockback = knockbackDirection.normalized + Vector3.up * 0.5f;
        rb.AddForce(adjustedKnockback * knockbackForce, ForceMode.Impulse);

        // Start adjusting capsule as soon as knockdown begins
        float originalHeight = capsuleCollider.height;
        Vector3 originalCenter = capsuleCollider.center;
        float transitionDuration = 0.3f;
        float elapsed = 0f;

        // Smoothly transition capsule to lying position during knockdown
        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionDuration;

            // Only change the capsule direction, maintain original height
            capsuleCollider.direction = t > 0.5f ? 2 : 1; // Switch to Z-axis halfway through

            // Adjust center position for the rotation
            if (capsuleCollider.direction == 2) // If horizontal
            {
                capsuleCollider.center = new Vector3(0, 0.45f, 0); // Adjust this Y value to match your character
            }
            else // If vertical
            {
                capsuleCollider.center = originalCenter;
            }

            yield return null;
        }

        // Ground check and position adjustment
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out hit, groundCheckDistance, groundLayer))
        {
            float startY = transform.position.y;
            float targetY = hit.point.y;
            float lerpTime = 0.1f;
            elapsed = 0f;

            while (elapsed < lerpTime)
            {
                elapsed += Time.deltaTime;
                Vector3 newPos = transform.position;
                newPos.y = Mathf.Lerp(startY, targetY, elapsed / lerpTime);
                transform.position = newPos;
                yield return null;
            }
        }

        yield return new WaitForSeconds(knockdownDuration);

        // Begin recovery
        animator.SetTrigger(getUpTriggerHash);
        animator.speed = recoverySpeed;

        // Smoothly transition capsule back to standing position
        elapsed = 0f;
        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionDuration;

            // Switch back to vertical orientation
            capsuleCollider.direction = t > 0.5f ? 1 : 2;

            // Return to original center position
            if (capsuleCollider.direction == 1) // If vertical
            {
                capsuleCollider.center = originalCenter;
            }
            else // If horizontal
            {
                capsuleCollider.center = new Vector3(0, 0.45f, 0);
            }

            yield return null;
        }

        // Wait for recovery animation to start
        yield return new WaitUntil(() =>
            animator.GetCurrentAnimatorStateInfo(0).IsName("GetUp"));

        // Gradually restore control during recovery
        float recoveryProgress = 0f;
        while (recoveryProgress < 1f)
        {
            recoveryProgress += Time.deltaTime * recoverySpeed;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            yield return null;
        }

        // Complete recovery
        animator.speed = 1f;
        animator.SetBool(isKnockedDownHash, false);
        isKnockedDown = false;
    }


    private void OnValidate()
    {
        if (characterModel == null)
        {
            characterModel = GetComponentInChildren<Animator>()?.gameObject;
            if (characterModel != null)
            {
                Debug.Log($"Auto-assigned character model to {characterModel.name}");
            }
            else
            {
                Debug.LogWarning("Character Model not assigned on " + gameObject.name);
            }
        }

        // Validate ground check distance
        if (groundCheckDistance <= 0)
        {
            groundCheckDistance = 2f;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward * 2);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.right * 2);
    }
}