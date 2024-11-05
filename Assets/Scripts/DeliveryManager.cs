using System.Collections.Generic;
using UnityEngine;

public class DeliveryManager : MonoBehaviour
{
    public static DeliveryManager Instance { get; private set; }
    public List<Package> packages = new List<Package>();

    // New list to hold loaded addresses
    private List<string> loadedAddresses = new List<string>();

    // Variables to track total collected and delivered packages
    [SerializeField] private int TotalCollected = 0;
    [SerializeField] private int TotalDelivered = 0;

    // Boolean flag to track if all packages are delivered
    private bool arePackagesDelivered = false;

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

    void Start()
    {
        LoadAddresses();
        GeneratePackages();
    }

    private void LoadAddresses()
    {
        TextAsset addressData = Resources.Load<TextAsset>("addresses");
        if (addressData != null)
        {
            try
            {
                AddressList addressList = JsonUtility.FromJson<AddressList>(addressData.text);

                if (addressList != null && addressList.addresses != null && addressList.addresses.Count > 0)
                {
                    loadedAddresses = addressList.addresses; // Store loaded addresses
                    Debug.Log($"{loadedAddresses.Count} addresses loaded successfully.");
                }
                else
                {
                    Debug.LogError("Address list is null or empty after deserialization.");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error deserializing address data: {ex.Message}");
            }
        }
        else
        {
            Debug.LogError("Could not find addresses.json in Resources folder.");
        }
    }

    private void GeneratePackages()
    {
        int basePackageCount = Random.Range(1, 1);
        int bonusPackages = DayManager.Instance.GetPackageBonus();
        int totalPackages = basePackageCount + bonusPackages;

        packages.Clear();
        TotalCollected = 0; // Reset collected count
        TotalDelivered = 0; // Reset delivered count
        arePackagesDelivered = false; // Reset the flag

        for (int i = 0; i < totalPackages; i++)
        {
            string address = GetRandomAddress();
            Package package = new Package(address); // Create a new package instance
            packages.Add(package);
        }

        Debug.Log($"Generated {packages.Count} packages for {DayManager.Instance.currentDay}");
    }

    private string GetRandomAddress()
    {
        if (loadedAddresses.Count == 0)
        {
            Debug.LogWarning("No addresses available. Generating fallback address.");
            return $"Random Address {packages.Count + 1}"; // Fallback address if no addresses available
        }

        int randomIndex = Random.Range(0, loadedAddresses.Count);
        return loadedAddresses[randomIndex];
    }

    public void CollectPackages()
    {
        foreach (var package in packages)
        {
            if (!package.isCollected) // Only collect if the package hasn't been collected
            {
                package.isCollected = true;
                package.isDelivered = false; // Ensure package is not marked as delivered when collected
                TotalCollected++; // Increment collected count
                Debug.Log($"Collected package at address: {package.address}");
            }
        }

        // Optionally, check if all packages have been delivered after collecting packages
        // You can remove this part if you don't want to check here
        CheckAllPackagesDelivered();
    }


    public void AdvanceDay()
    {
        DayManager.Instance.NextDay();
        GeneratePackages();
    }

    public void DeliverPackage(Package package)
    {
        House targetHouse = FindHouseByAddress(package.address);

        if (targetHouse != null && targetHouse.dropOffArea != null) // Ensure dropOffArea is not null
        {
            // Use the dropOffArea reference to deliver the package
            targetHouse.dropOffArea.OnPackageDelivered(package);
            package.isDelivered = true;
            TotalDelivered++; // Increment delivered count
            Debug.Log($"Delivered package to {package.address}");
            packages.Remove(package); // Remove the delivered package from the list

            CheckAllPackagesDelivered();
        }
        else
        {
            Debug.LogWarning($"No house found for address: {package.address}");
        }
    }

    private House FindHouseByAddress(string address)
    {
        House[] allHouses = FindObjectsByType<House>(FindObjectsSortMode.None);
        foreach (House house in allHouses)
        {
            if (house.address == address)
            {
                return house;
            }
        }
        return null; // No house found
    }

    public void CheckAllPackagesDelivered()
    {
        bool allDelivered = true; // Assume all are delivered unless proven otherwise

        foreach (var package in packages)
        {
            if (!package.isDelivered)
            {
                allDelivered = false; // Found a package that is not delivered
                break; // No need to check further
            }
        }

        if (allDelivered)
        {
            arePackagesDelivered = true; // Set the flag to true
            Debug.Log("All packages have been delivered! Day's work is complete. Return to the Depot.");
        }
    }

    public void UpdatePlayerStats()
    {
        if (arePackagesDelivered)
        {
            Player.Instance.UpdateStats(TotalCollected, TotalDelivered);
        }
        else
        {
            Debug.LogWarning("Not all packages have been delivered yet!");
        }
    }

    public bool ArePackagesDelivered()
    {
        return arePackagesDelivered;
    }
}

[System.Serializable]
public class AddressList
{
    public List<string> addresses; // List of addresses
}
