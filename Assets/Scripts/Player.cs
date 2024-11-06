using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    // Player statistics
    public int Money = 0; // Total money aquired 
    public int TotalPackagesDelivered = 0; // lifetime Packages delivered.
    public int PackagesDelivered = 0; // Packages delivered in a day.

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

    }


}
