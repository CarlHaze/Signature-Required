using UnityEngine;

public class PaymentManager : MonoBehaviour
{
    public static PaymentManager Instance { get; private set; }

    [SerializeField] private int paymentPerPackage = 5;

    [SerializeField] private int paymentEarned;
    public int PaymentEarned
    {
        get => paymentEarned;
        private set => paymentEarned = value;
    }

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
        if (DeliveryManager.Instance != null)
        {
            int totalDelivered = DeliveryManager.Instance.TotalDelivered;
            PaymentEarned = totalDelivered * paymentPerPackage;
        }
    }

    public void UpdatePlayerMoney()
    {
        if (Player.Instance != null)
        {
            Player.Instance.Money += PaymentEarned;
            Debug.Log($"Player money updated. New balance: {Player.Instance.Money}");
        }
    }
}
