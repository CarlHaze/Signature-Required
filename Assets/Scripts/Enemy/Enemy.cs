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
    [SerializeField] private float knockdownDuration = 10f;
    private readonly int knockdownTriggerHash = Animator.StringToHash("KnockDown");
    private readonly int knockdownIndexHash = Animator.StringToHash("KnockdownIndex");
    private readonly int getUpTriggerHash = Animator.StringToHash("GetUp");
    private bool isKnockedDown = false;

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

        // Verify animator parameters
        bool hasAllParameters = true;
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            Debug.Log($"Found parameter: {param.name} of type {param.type}");
        }

        // Check for required collider
        if (GetComponent<Collider>() == null)
        {
            Debug.LogError("No Collider found on NPC parent object! Please add a Collider component.");
            return;
        }

        Debug.Log($"Enemy initialized with {currentHealth} health. Using animator on {characterModel.name}");
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
            HandleKnockdown();
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

    private void HandleKnockdown()
    {
        Debug.Log($"HandleKnockdown called. Current health: {currentHealth}, IsKnockedDown: {isKnockedDown}");
        if (animator != null && !isKnockedDown)
        {
            isKnockedDown = true;

            // Set random knockdown index (0 or 1)
            float randomIndex = Random.Range(0, 2);
            animator.SetFloat(knockdownIndexHash, randomIndex);
            Debug.Log($"Knockdown trigger set. RandomIndex: {randomIndex}");

            // Trigger the knockdown
            animator.SetTrigger(knockdownTriggerHash);

            StartCoroutine(KnockdownSequence());
        }
    }

    private IEnumerator KnockdownSequence()
    {
        // Wait until near the end of the animation
        AnimatorStateInfo stateInfo;
        do
        {
            stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            yield return null;
        } while (stateInfo.normalizedTime < 0.9f);

        // Set the animator speed to 0 to freeze at the last frame
        animator.speed = 0;

        // Wait for knockdown duration
        yield return new WaitForSeconds(knockdownDuration);

        // Resume animation speed and play get up
        animator.speed = 1;
        animator.SetTrigger(getUpTriggerHash);

        // Wait for get up animation to complete
        do
        {
            stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            yield return null;
        } while (stateInfo.normalizedTime < 1.0f);

        // Reset state
        isKnockedDown = false;
        currentHealth = maxHealth;
        Debug.Log($"Animation state: {stateInfo.normalizedTime:F2}");
    }

    private void OnValidate()
    {
        if (characterModel == null)
        {
            Debug.LogWarning("Character Model not assigned on " + gameObject.name);
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