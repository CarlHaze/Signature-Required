using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    // Player properties
    [SerializeField] private int money = 0; // Player's current money
    [SerializeField] private int packagesDelivered = 0; // Total packages delivered

    private void Awake()
    {
        // Ensure there's only one instance of Player
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist player across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Method to add money
    public void AddMoney(int amount)
    {
        if (amount > 0)
        {
            money += amount;
            Debug.Log($"Money added: {amount}. Total money: {money}");
        }
    }

    // Method to deliver a package
    public void DeliverPackage()
    {
        packagesDelivered++;
        Debug.Log($"Package delivered! Total packages delivered: {packagesDelivered}");
    }

    // Method to get the current money
    public int GetMoney()
    {
        return money;
    }

    // Method to get the total packages delivered
    public int GetPackagesDelivered()
    {
        return packagesDelivered;
    }

    // Optional: Reset player's progress (for testing or restarting)
    public void ResetProgress()
    {
        money = 0;
        packagesDelivered = 0;
        Debug.Log("Player progress reset.");
    }

    // Method to update player stats based on DeliveryManager data
    public void UpdateStats(int totalCollected, int totalDelivered)
    {
        packagesDelivered = totalDelivered;
        money = totalDelivered * 5;
        Debug.Log($"Player stats updated. Total packages delivered: {packagesDelivered}, Total money: {money}");
    }
}
