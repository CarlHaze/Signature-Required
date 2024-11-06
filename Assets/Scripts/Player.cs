using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    // Player statistics
    public int Money = 0; // Total money acquired 
    public int TotalPackagesDelivered = 0; // lifetime Packages delivered.

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

    public void IncrementTotalPackagesDelivered()
    {
        TotalPackagesDelivered++;
        Debug.Log($"Total packages delivered incremented to: {TotalPackagesDelivered}");
    }
}
