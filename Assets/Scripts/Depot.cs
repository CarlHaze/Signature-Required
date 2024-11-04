using UnityEngine;

public class Depot : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Car"))
        {
            DeliveryManager.Instance.CollectPackages();
        }
    }
}
