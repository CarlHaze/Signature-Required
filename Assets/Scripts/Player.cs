using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    // Player statistics
    [SerializeField] private int Money = 0;
    [SerializeField] private int PackagesDelivered = 0;
    [SerializeField] public int TotalPackagesDelivered = 0;

    // Daily stats
    private bool statsUpdatedForDay = false;

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

        // Initialize player stats
        Money = 0;
        PackagesDelivered = 0;
        TotalPackagesDelivered = 0;
    }

    // Method to reset daily stats at the beginning of a new day
    public void ResetDailyStats()
    {
        PackagesDelivered = 0; // Reset the number of packages delivered for today
        statsUpdatedForDay = false; // Allow stats to be updated again
    }

    // Method to update the player's money based on payments received
    public void UpdateMoney(int amount)
    {
        Money += amount;
        Debug.Log($"Player Money Updated: {Money}");
    }

    // Method to handle the delivery of a package
    public void DeliverPackage()
    {
        PackagesDelivered++;
        TotalPackagesDelivered++;
        Debug.Log($"Packages Delivered Today: {PackagesDelivered}, Total Delivered: {TotalPackagesDelivered}");
    }

    // Method to check if stats have been updated for the day
    public bool HasStatsBeenUpdatedForDay()
    {
        return statsUpdatedForDay;
    }

    // Method to set the stats updated flag
    public void SetStatsUpdatedForDay(bool updated)
    {
        statsUpdatedForDay = updated;
    }

    // Additional methods can be added here to manage player interactions and stats
}
