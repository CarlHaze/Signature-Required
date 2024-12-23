using UnityEngine;

public class DayManager : MonoBehaviour
{
    public static DayManager Instance { get; private set; }

    public enum DayOfWeek { Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday }
    public DayOfWeek currentDay = DayOfWeek.Monday;

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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            AdvanceDay();
        }
    }

    public int GetPackageBonus()
    {
        switch (currentDay)
        {
            case DayOfWeek.Monday:
                return 5;  // Busy day
            case DayOfWeek.Tuesday:
                return 0;  // Less busy
            case DayOfWeek.Wednesday:
                return 4;  // Similar to Monday
            case DayOfWeek.Thursday:
                return 2;  // Less busy
            case DayOfWeek.Friday:
                return 7;  // More busy
            default:
                return 0;  // No deliveries on weekends (placeholder for now)
        }
    }

    public void NextDay()
    {
        currentDay = (DayOfWeek)(((int)currentDay + 1) % 7);
        Debug.Log("New day: " + currentDay);
    }

    public void AdvanceDay()
    {
        if (DeliveryManager.Instance.ArePackagesDelivered() || DeliveryManager.Instance.packages.Count == 0)
        {
            NextDay();
            DeliveryManager.Instance.ResetForNewDay();
            PaymentManager.Instance.UpdatePlayerMoney();
        }
        else
        {
            Debug.Log("Cannot advance to the next day. There are still packages to be delivered.");
        }
    }
}
