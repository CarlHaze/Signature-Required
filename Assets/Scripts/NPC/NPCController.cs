using UnityEngine;
using System.Collections;

public class NPCController : MonoBehaviour, IDamageable
{
    public int maxHealth = 100;
    [SerializeField] private int currentHealth;

    [Header("Components")]
    private Animator animator;
    [SerializeField] private GameObject characterModel; // Reference to the X Bot model
    private Rigidbody rb;
    private CapsuleCollider capsuleCollider; // Original capsuleCollider declaration
    [SerializeField] private BoxCollider boxCollider; // Added box collider

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

        // Get the colliders
        capsuleCollider = GetComponent<CapsuleCollider>();
        if (boxCollider == null)
            boxCollider = GetComponent<BoxCollider>();

        // Ensure box collider starts disabled
        if (boxCollider != null)
            boxCollider.enabled = false;

        ValidateAnimatorParameters();
        ValidateColliders();

        Debug.Log($"Enemy initialized with {currentHealth} health. Using animator on {characterModel.name}");
    }

    private void ValidateAnimatorParameters()
    {
        if (animator != null)
        {
            ValidateParameter("Hit", AnimatorControllerParameterType.Trigger);
            ValidateParameter("HitDirectionX", AnimatorControllerParameterType.Float);
            ValidateParameter("HitDirectionZ", AnimatorControllerParameterType.Float);
            ValidateParameter("HitIntensity", AnimatorControllerParameterType.Float);
            ValidateParameter("KnockDown", AnimatorControllerParameterType.Trigger);
            ValidateParameter("GetUp", AnimatorControllerParameterType.Trigger);
            ValidateParameter("IsKnockedDown", AnimatorControllerParameterType.Bool);
            ValidateParameter("KnockdownIndex", AnimatorControllerParameterType.Float);
        }
    }

    private void ValidateParameter(string paramName, AnimatorControllerParameterType expectedType)
    {
        bool found = false;
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName)
            {
                found = true;
                if (param.type != expectedType)
                {
                    Debug.LogError($"Parameter '{paramName}' exists but is of type {param.type} instead of expected {expectedType}");
                }
                break;
            }
        }
        if (!found)
        {
            Debug.LogError($"Missing animator parameter: '{paramName}' of type {expectedType} on {gameObject.name}");
        }
    }

    private void ValidateColliders()
    {
        if (capsuleCollider == null)
            Debug.LogError("No CapsuleCollider found on " + gameObject.name);
        if (boxCollider == null)
            Debug.LogError("No BoxCollider found on " + gameObject.name);
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

        // Switch to box collider when knocked down
        if (capsuleCollider != null && boxCollider != null)
        {
            capsuleCollider.enabled = false;
            boxCollider.enabled = true;
            Debug.Log("Switched to box collider for knockdown");
        }

        // Set blend tree parameter
        animator.SetFloat(knockdownIndexHash, Random.Range(0, 2));

        // Instantly restore health at start of knockdown
        currentHealth = maxHealth;

        // Apply knockback force with upward component
        Vector3 adjustedKnockback = knockbackDirection.normalized + Vector3.up * 0.5f;
        rb.AddForce(adjustedKnockback * knockbackForce, ForceMode.Impulse);

        yield return new WaitForSeconds(knockdownDuration);

        // Begin recovery
        animator.SetTrigger(getUpTriggerHash);
        animator.speed = recoverySpeed;

        // Wait for get up animation to start
        yield return new WaitUntil(() =>
            animator.GetCurrentAnimatorStateInfo(0).IsName("GetUp"));

        // Switch back to capsule collider when getting up
        if (capsuleCollider != null && boxCollider != null)
        {
            boxCollider.enabled = false;
            capsuleCollider.enabled = true;
            Debug.Log("Switched back to capsule collider for recovery");
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

        // Auto-assign box collider if not set
        if (boxCollider == null)
            boxCollider = GetComponent<BoxCollider>();

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
