using System.Collections.Generic;
using UnityEngine;

public class DeliveryManager : MonoBehaviour
{
    public static DeliveryManager Instance { get; private set; }
    public List<Package> packages = new List<Package>();

    [SerializeField] // Use this to expose in Inspector
    public List<AddressPackage> addressPackages = new List<AddressPackage>(); // Now visible in Inspector

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
        DisplayAddressPackages(); // Call to display address packages on start
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
                    loadedAddresses = addressList.addresses;
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
        int basePackageCount = Random.Range(15, 21);
        int bonusPackages = DayManager.Instance.GetPackageBonus();
        int totalPackages = basePackageCount + bonusPackages;

        addressPackages.Clear(); // Clear previous data

        // Generate address packages with a random number of packages for each address
        for (int i = 0; i < totalPackages; i++)
        {
            string address = GetRandomAddress();

            // Check if this address already exists
            AddressPackage existingAddressPackage = addressPackages.Find(ap => ap.address == address);
            if (existingAddressPackage != null)
            {
                existingAddressPackage.numberOfPackages++; // Increment count
            }
            else
            {
                // Create new address package with count of 1
                addressPackages.Add(new AddressPackage(address, 1));
            }
        }

        // Generate packages from addressPackages list
        packages.Clear();
        foreach (var addressPackage in addressPackages)
        {
            for (int j = 0; j < addressPackage.numberOfPackages; j++)
            {
                packages.Add(new Package(addressPackage.address));
            }
        }

        Debug.Log($"Generated {packages.Count} packages for {DayManager.Instance.currentDay}");
    }

    private string GetRandomAddress()
    {
        if (loadedAddresses.Count == 0)
        {
            Debug.LogWarning("No addresses available. Generating fallback address.");
            return $"Random Address {packages.Count + 1}";
        }

        int randomIndex = Random.Range(0, loadedAddresses.Count);
        return loadedAddresses[randomIndex];
    }

    public void CollectPackages()
    {
        foreach (var package in packages)
        {
            if (!package.isCollected)
            {
                package.isCollected = true;
                Debug.Log($"Collected package at address: {package.address}");
            }
        }
    }

    public void AdvanceDay()
    {
        DayManager.Instance.NextDay();
        GeneratePackages();
        DisplayAddressPackages(); // Display address packages after advancing the day
    }

    private void DisplayAddressPackages()
    {
        Debug.Log("Address Packages and Counts:");
        foreach (var addressPackage in addressPackages)
        {
            Debug.Log($"Address: {addressPackage.address}, Number of Packages: {addressPackage.numberOfPackages}");
        }
    }
}

[System.Serializable]
public class AddressList
{
    public List<string> addresses;
}
