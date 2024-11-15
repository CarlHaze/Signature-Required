using ECM2;
using UnityEngine;

public class FightingAbility : MonoBehaviour
{
    // Private variables to track punching state
    private bool _isPunching;

    // You can adjust these variables to suit your combat system (e.g., for cooldowns or punch damage)
    public float punchCooldown = 0.5f;
    private float _timeSinceLastPunch;

    // Public property to check if the character is currently punching
    public bool IsPunching
    {
        get { return _isPunching; }
    }

    // Event for triggering punch animation (you can modify this for custom event handling)
    public void StartPunching()
    {
        if (_timeSinceLastPunch >= punchCooldown) // Prevent punching if cooldown is active
        {
            _isPunching = true;
            _timeSinceLastPunch = 0f;  // Reset cooldown
            // Trigger any necessary animation or attack logic here
            Debug.Log("Punch Started");
        }
    }

    // Stop the punching state
    public void StopPunching()
    {
        _isPunching = false;
        // Trigger logic to stop punch animation or reset any related state
        Debug.Log("Punch Stopped");
    }

    // Method to update punch cooldown timer
    private void Update()
    {
        // Update cooldown
        if (_timeSinceLastPunch < punchCooldown)
        {
            _timeSinceLastPunch += Time.deltaTime;
        }
    }

    // Optionally, you can add a method for dealing damage or triggering effects
    public void DealDamage()
    {
        // Logic for dealing damage when punching (if necessary for your game)
        if (_isPunching)
        {
            Debug.Log("Dealing punch damage");
            // Add your damage logic here (e.g., hit detection, applying damage, etc.)
        }
    }
}

