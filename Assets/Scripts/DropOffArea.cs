using System.Collections.Generic;
using UnityEngine;

public class DropOffArea : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Car"))
        {
            DeliveryManager deliveryManager = DeliveryManager.Instance;
            string houseAddress = transform.parent.GetComponent<House>().address;

            List<Package> packagesToDeliver = new List<Package>();
            foreach (var package in deliveryManager.packages)
            {
                if (!package.isDelivered && package.address == houseAddress)
                {
                    if (package.isCollected)
                    {
                        packagesToDeliver.Add(package);
                    }
                    else
                    {
                        Debug.LogWarning($"Package for address {package.address} has not been collected yet.");
                    }
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
}
