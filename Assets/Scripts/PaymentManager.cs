using UnityEngine;

public class PaymentManager : MonoBehaviour
{
    public static PaymentManager Instance { get; private set; }

    [SerializeField] private int paymentPerPackage = 6; // Adjust this value as needed
    public int PaymentEarned { get; private set; }

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
    }

    // Method to handle payment calculation and update
    public void HandlePayment(int packagesDelivered)
    {
        // Calculate payment based on packages delivered
        PaymentEarned = packagesDelivered * paymentPerPackage;

        // Log the payment amount
        Debug.Log($"Payment Earned for {packagesDelivered} packages: {PaymentEarned}");

        // Update player's money
        Player.Instance.UpdateMoney(PaymentEarned);
    }

    // Method to update the delivered packages count
    public void UpdatePackageDeliveredStats(int packagesDelivered)
    {
        // Update the total packages delivered in Player stats
        for (int i = 0; i < packagesDelivered; i++)
        {
            Player.Instance.DeliverPackage();
        }

        // Optionally, log the updated total packages delivered
        Debug.Log($"Updated Player's total packages delivered: {Player.Instance.TotalPackagesDelivered}");
    }

    // Additional methods can be added here as needed
}
