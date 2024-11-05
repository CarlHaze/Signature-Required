using System.Collections.Generic;
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
            List<Package> packagesToDeliver = new List<Package>();
            foreach (var package in deliveryManager.packages)
            {
                if (!package.isDelivered && package.address == houseAddress)
                {
                    packagesToDeliver.Add(package);
                }
            }

            if (packagesToDeliver.Count == 0)
            {
                Debug.Log($"No packages to deliver at address: {houseAddress}");
            }
            else
            {
                foreach (var package in packagesToDeliver)
                {
                    deliveryManager.DeliverPackage(package);
                }

                // Check if all packages have been delivered after this delivery
                deliveryManager.CheckAllPackagesDelivered();
            }
        }
    }

    public void OnPackageDelivered(Package package)
    {
        // Handle the delivery logic here
        package.isDelivered = true;
        Debug.Log($"Package delivered to {transform.parent.GetComponent<House>().address}");
    }
}
