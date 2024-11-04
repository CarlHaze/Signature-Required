using UnityEngine;

public class DropOffArea : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Check if the collider belongs to a vehicle
        if (other.CompareTag("Car")) // Assuming the vehicle has the "Car" tag
        {
            DeliveryManager deliveryManager = DeliveryManager.Instance;

            // Get the address of this house
            string houseAddress = transform.parent.GetComponent<House>().address;

            // Deliver packages at this address
            foreach (var package in deliveryManager.packages)
            {
                if (!package.isDelivered && package.address == houseAddress)
                {
                    package.isDelivered = true; // Mark as delivered
                    Debug.Log($"Delivered package to {houseAddress}");
                    // Implement payment logic or any further action here
                }
            }

            // Check if all packages have been delivered after this delivery
            deliveryManager.CheckAllPackagesDelivered();
        }
    }

    public void OnPackageDelivered(Package package)
    {
        // Handle the delivery logic here
        package.isDelivered = true;
        Debug.Log($"Package delivered to {transform.parent.GetComponent<House>().address}");
    }
}
