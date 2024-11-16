using UnityEngine;
using ECM2;

public class CharacterMovementController : MonoBehaviour
{
    private Character _character;

    private void Awake()
    {
        _character = GetComponent<Character>();
    }


    private void Update()
    {
        // Poll movement input

        Vector2 inputMove = new Vector2()
        {
            x = Input.GetAxisRaw("Horizontal"),
            y = Input.GetAxisRaw("Vertical")
        };

        // Compose a movement direction vector in world space

        Vector3 movementDirection = Vector3.zero;

        movementDirection += Vector3.right * inputMove.x;
        movementDirection += Vector3.forward * inputMove.y;

        // If character has a camera assigned,
        // make movement direction relative to this camera view direction

        if (_character.camera)
        {
            movementDirection
                = movementDirection.relativeTo(_character.cameraTransform);
        }

        // Set character's movement direction vector

        _character.SetMovementDirection(movementDirection);

        // Crouch input

        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.C))
            _character.Crouch();
        else if (Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.C))
            _character.UnCrouch();

        // Jump input

        if (Input.GetButtonDown("Jump"))
            _character.Jump();
        else if (Input.GetButtonUp("Jump"))
            _character.StopJumping();
    }
}

