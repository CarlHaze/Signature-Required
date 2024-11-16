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
    public int Damage = 10;


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
}
