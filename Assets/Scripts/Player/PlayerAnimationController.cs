using UnityEngine;
using ECM2;

public class PlayerAnimationController : MonoBehaviour
{
    // Cache Animator parameters

    private static readonly int Forward = Animator.StringToHash("Forward");
    private static readonly int Turn = Animator.StringToHash("Turn");
    private static readonly int Ground = Animator.StringToHash("OnGround");
    private static readonly int Crouch = Animator.StringToHash("Crouch");
    private static readonly int Jump = Animator.StringToHash("Jump");
    private static readonly int JumpLeg = Animator.StringToHash("JumpLeg");

    //
    private static readonly int IsPunching = Animator.StringToHash("IsPunching"); // New parameterfor punching animation

    // Cached Character

    private Character _character;

    // punch
    private bool isPunching = false;

    [SerializeField]
    private Transform modelTransform; // Reference to the actual mesh/model transform


    private void Awake()
    {
        // Cache our Character

        _character = GetComponentInParent<Character>();
        if (modelTransform == null)
            modelTransform = transform.GetChild(0); // Assuming the model is the first child
    
    }

    // Add this method to be called as an Animation Event at the end of the punch animation
    public void OnPunchAnimationComplete()
    {
        isPunching = false;
        _character.GetAnimator().SetBool(IsPunching, false);
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;

        // Get Character animator

        Animator animator = _character.GetAnimator();

        // Compute input move vector in local space

        Vector3 move = transform.InverseTransformDirection(_character.GetMovementDirection());


        // Get the direction the character is facing
        Vector3 characterForward = transform.forward;

        // Update the animator parameters

        float forwardAmount = Mathf.InverseLerp(0.0f, _character.GetMaxSpeed(), _character.GetSpeed());


        animator.SetFloat(Forward, forwardAmount, 0.1f, deltaTime);
        animator.SetFloat(Turn, Mathf.Atan2(move.x, move.z), 0.1f, deltaTime);

        animator.SetBool(Ground, _character.IsGrounded());
        animator.SetBool(Crouch, _character.IsCrouched());

        if (_character.IsFalling())
            animator.SetFloat(Jump, _character.GetVelocity().y, 0.1f, deltaTime);

        // Calculate which leg is behind, so as to leave that leg trailing in the jump animation
        // (This code is reliant on the specific run cycle offset in our animations,
        // and assumes one leg passes the other at the normalized clip times of 0.0 and 0.5)

        float runCycle = Mathf.Repeat(animator.GetCurrentAnimatorStateInfo(0).normalizedTime + 0.2f, 1.0f);
        float jumpLeg = (runCycle < 0.5f ? 1.0f : -1.0f) * forwardAmount;

        if (_character.IsGrounded())
            animator.SetFloat(JumpLeg, jumpLeg);

        // Handle punch input - add check for not crouched
        if (Input.GetMouseButtonDown(0) &&
            !isPunching &&
            _character.IsGrounded() &&
            !_character.IsCrouched())  // Add this check to prevent punch while crouched
        {
            isPunching = true;
            modelTransform.forward = characterForward;
            animator.SetBool(IsPunching, true);
        }

        // Handle rotation when not punching
        if (!isPunching)
        {
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
}