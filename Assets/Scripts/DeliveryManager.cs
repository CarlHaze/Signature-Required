using System.Collections.Generic;
using UnityEngine;

public class DeliveryManager : MonoBehaviour
{
    public static DeliveryManager Instance { get; private set; }
    public List<Package> packages = new List<Package>();

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
                // Deserialize the JSON into AddressList object
                AddressList addressList = JsonUtility.FromJson<AddressList>(addressData.text);

                // Check if addresses were successfully loaded
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
        // Get base package count range and apply day modifier
        int basePackageCount = Random.Range(15, 21);
        int bonusPackages = DayManager.Instance.GetPackageBonus();
        int totalPackages = basePackageCount + bonusPackages;

        // Ensure the packages list is cleared before adding new packages
        packages.Clear();

        // Generate the packages
        for (int i = 0; i < totalPackages; i++)
        {
            // Randomly select an address from the loaded addresses
            string address = GetRandomAddress();
            packages.Add(new Package(address));
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

        // Get a random index from the loaded addresses
        int randomIndex = Random.Range(0, loadedAddresses.Count);
        return loadedAddresses[randomIndex]; // Return the address of the randomly selected package
    }

    public void CollectPackages()
    {
        foreach (var package in packages)
        {
            if (!package.isCollected)
            {
                package.isCollected = true;
                package.isDelivered = true; // Flag as delivered
                Debug.Log($"Collected package at address: {package.address}");
            }
        }
    }

    public void AdvanceDay()
    {
        DayManager.Instance.NextDay(); // Call NextDay to advance the day
        GeneratePackages(); // Generate new packages after advancing the day
    }
}

[System.Serializable]
public class AddressList
{
    public List<string> addresses; // List of addresses
}
