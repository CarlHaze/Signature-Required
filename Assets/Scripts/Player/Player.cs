using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    // Player statistics
    public int Money = 0; // Total money acquired 
    public int TotalPackagesDelivered = 0; // lifetime Packages delivered.

    // Player Health and Stamina
    public int maxHealth = 100;
    public int Health;
    public int Stamina = 100;

    // Player Strength and Damage
    public int Strength = 10;
    public int Damage => Strength * 2; // Damage is based on Strength

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        Health = maxHealth;
    }

    public void IncrementTotalPackagesDelivered()
    {
        TotalPackagesDelivered++;
        Debug.Log($"Total packages delivered incremented to: {TotalPackagesDelivered}");
    }

    // Method to modify health
    public void ModifyHealth(int amount)
    {
        Health = Mathf.Clamp(Health + amount, 0, maxHealth);
        Debug.Log($"Health modified by {amount}. Current health: {Health}");
    }

    // Method to modify stamina
    public void ModifyStamina(int amount)
    {
        Stamina = Mathf.Clamp(Stamina + amount, 0, 100);
        Debug.Log($"Stamina modified by {amount}. Current stamina: {Stamina}");
    }

    // Method to modify strength
    public void ModifyStrength(int amount)
    {
        Strength += amount;
        Debug.Log($"Strength modified by {amount}. Current strength: {Strength}");
    }
}
