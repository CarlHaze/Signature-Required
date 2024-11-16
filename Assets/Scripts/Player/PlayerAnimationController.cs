using UnityEngine;
using ECM2;
public class PlayerAnimationController : MonoBehaviour
{
    // Basic movement parameters
    private static readonly int Forward = Animator.StringToHash("Forward");
    private static readonly int Turn = Animator.StringToHash("Turn");
    private static readonly int Ground = Animator.StringToHash("OnGround");
    private static readonly int Crouch = Animator.StringToHash("Crouch");
    private static readonly int Jump = Animator.StringToHash("Jump");
    private static readonly int JumpLeg = Animator.StringToHash("JumpLeg");
    private static readonly int FightMode = Animator.StringToHash("FightMode");

    // Attack triggers
    private static readonly int TriggerJab = Animator.StringToHash("TriggerJab");
    private static readonly int TriggerStraight = Animator.StringToHash("TriggerStraight");

    private Character _character;
    private bool isAttacking = false;
    private bool inFightMode = false;
    private float currentAttackTime = 0f;
    private const float ATTACK_COOLDOWN = 0.5f;

    [SerializeField]
    private Transform modelTransform;

    private void Awake()
    {
        _character = GetComponentInParent<Character>();
        if (modelTransform == null)
            modelTransform = transform.GetChild(0);
    }

    public void OnAttackAnimationComplete()
    {
        Debug.Log("Attack Complete");
        isAttacking = false;
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;
        Animator animator = _character.GetAnimator();
        Vector3 move = transform.InverseTransformDirection(_character.GetMovementDirection());
        Vector3 characterForward = transform.forward;

        // Handle fight mode toggle
        if (Input.GetKeyDown(KeyCode.F))
        {
            inFightMode = !inFightMode;
            animator.SetBool(FightMode, inFightMode);

            if (inFightMode)
            {
                modelTransform.forward = transform.forward;
            }

            Debug.Log($"Fight Mode: {inFightMode}");
        }

        // Handle attacks in fight mode
        if (inFightMode && !isAttacking && _character.IsGrounded() && !_character.IsCrouched())
        {
            if (Time.time >= currentAttackTime + ATTACK_COOLDOWN)
            {
                if (Input.GetMouseButtonDown(0)) // Left click - Jab
                {
                    Debug.Log("Performing Jab");
                    PerformAttack(animator, TriggerJab);
                }
                else if (Input.GetMouseButtonDown(1)) // Right click - Straight
                {
                    Debug.Log("Performing Straight");
                    PerformAttack(animator, TriggerStraight);
                }
            }
        }

        // Calculate movement values
        float forwardAmount = Mathf.InverseLerp(0.0f, _character.GetMaxSpeed(), _character.GetSpeed());
        float turnAmount = Mathf.Atan2(move.x, move.z);

        // Handle movement and rotation
        if (!isAttacking)
        {
            if (inFightMode)
            {
                modelTransform.forward = transform.forward;

                if (_character.GetMovementDirection().magnitude > 0.1f)
                {
                    animator.SetFloat(Forward, forwardAmount, 0.1f, deltaTime);
                    animator.SetFloat(Turn, turnAmount, 0.1f, deltaTime);
                }
                else
                {
                    animator.SetFloat(Forward, 0f, 0.1f, deltaTime);
                    animator.SetFloat(Turn, 0f, 0.1f, deltaTime);
                }
            }
            else
            {
                animator.SetFloat(Forward, forwardAmount, 0.1f, deltaTime);
                animator.SetFloat(Turn, turnAmount, 0.1f, deltaTime);

                Vector3 moveDirection = _character.GetMovementDirection();
                if (moveDirection.magnitude > 0.1f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                    modelTransform.rotation = Quaternion.Slerp(
                        modelTransform.rotation,
                        targetRotation,
                        10f * deltaTime
                    );
                }
            }
        }

        // Update other parameters
        animator.SetBool(Ground, _character.IsGrounded());
        animator.SetBool(Crouch, _character.IsCrouched());

        if (_character.IsFalling())
            animator.SetFloat(Jump, _character.GetVelocity().y, 0.1f, deltaTime);

        float runCycle = Mathf.Repeat(animator.GetCurrentAnimatorStateInfo(0).normalizedTime + 0.2f, 1.0f);
        float jumpLeg = (runCycle < 0.5f ? 1.0f : -1.0f) * forwardAmount;

        if (_character.IsGrounded())
            animator.SetFloat(JumpLeg, jumpLeg);
    }

    private void PerformAttack(Animator animator, int triggerParameter)
    {
        isAttacking = true;
        currentAttackTime = Time.time;
        modelTransform.forward = transform.forward;
        animator.SetTrigger(triggerParameter);
    }
}