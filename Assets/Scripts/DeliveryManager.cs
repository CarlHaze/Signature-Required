using System.Collections.Generic;
using UnityEngine;

public class DeliveryManager : MonoBehaviour
{
    public static DeliveryManager Instance { get; private set; }
    public List<Package> packages = new List<Package>();
    public List<AddressPackages> addressPackages = new List<AddressPackages>();

    // New list to hold loaded addresses
    private List<string> loadedAddresses = new List<string>();

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
                    InitializeAddressPackages(); // Initialize address packages
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

    private void InitializeAddressPackages()
    {
        foreach (var address in loadedAddresses)
        {
            addressPackages.Add(new AddressPackages(address));
        }
    }

    private void GeneratePackages()
    {
        int basePackageCount = Random.Range(15, 21);
        int bonusPackages = DayManager.Instance.GetPackageBonus();
        int totalPackages = basePackageCount + bonusPackages;

        packages.Clear();

        for (int i = 0; i < totalPackages; i++)
        {
            string address = GetRandomAddress();
            Package package = new Package(address); // Create a new package instance
            packages.Add(package);
            UpdateAddressPackageCount(address); // Update package count for the address
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

    private void UpdateAddressPackageCount(string address)
    {
        var addressPackage = addressPackages.Find(ap => ap.address == address);
        if (addressPackage != null)
        {
            addressPackage.numberOfPackages++;
        }
    }

    public void CollectPackages()
    {
        foreach (var package in packages)
        {
            if (!package.isCollected)
            {
                package.isCollected = true;
                package.isDelivered = false;
                Debug.Log($"Collected package at address: {package.address}");
            }
        }
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
            Debug.Log($"Delivered package to {package.address}");
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
            Debug.Log("All packages have been delivered!");
            // You can also trigger other actions here, such as updating the UI or advancing to the next day.
        }
    }

}

[System.Serializable]
public class AddressList
{
    public List<string> addresses; // List of addresses
}

[System.Serializable]
public class AddressPackages
{
    public string address;
    public int numberOfPackages = 0;

    public AddressPackages(string address)
    {
        this.address = address;
    }
}
