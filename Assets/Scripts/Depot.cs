using UnityEngine;

public class Depot : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Car"))
        {
            // Allow collecting packages regardless of delivery status
            DeliveryManager.Instance.CollectPackages();

            // Check if all packages are delivered for feedback
            if (DeliveryManager.Instance.ArePackagesDelivered())
            {
                Debug.Log("All packages have been delivered! You can now collect your payment.");
            }
            else
            {
                Debug.LogWarning("Not all packages have been delivered yet!");
            }
        }
    }
}
