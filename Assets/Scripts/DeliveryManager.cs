using System.Collections.Generic;
using UnityEngine;

public class DeliveryManager : MonoBehaviour
{
    public static DeliveryManager Instance { get; private set; }

    public List<Package> packages = new List<Package>();
    private List<string> loadedAddresses = new List<string>();

    [SerializeField] private int TotalCollected = 0;
    public int TotalDelivered { get; private set; }

    [SerializeField] private bool arePackagesDelivered = false;

    [SerializeField] private string currentDay;

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
        UpdateCurrentDay();
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
        int basePackageCount = 0;
        string day = DayManager.Instance.currentDay.ToString();

        if (day != "Saturday" && day != "Sunday")
        {
            basePackageCount = Random.Range(1, 3); // Adjust the range as needed
        }

        int bonusPackages = DayManager.Instance.GetPackageBonus();
        int totalPackages = basePackageCount + bonusPackages;

        packages.Clear();
        TotalCollected = 0;
        TotalDelivered = 0;
        arePackagesDelivered = false;

        for (int i = 0; i < totalPackages; i++)
        {
            string address = GetRandomAddress();
            Package package = new Package(address);
            packages.Add(package);
        }

        Debug.Log($"Generated {packages.Count} packages for {day}");
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
                TotalCollected++;
                Debug.Log($"Collected package for: {package.address}");
            }
        }
        CheckAllPackagesDelivered();
    }

    public void DeliverPackage(Package package)
    {
        if (!package.isCollected)
        {
            Debug.LogWarning($"Package at address {package.address} has not been collected yet.");
            return;
        }

        House targetHouse = FindHouseByAddress(package.address);

        if (targetHouse != null && targetHouse.dropOffArea != null)
        {
            package.isDelivered = true;
            TotalDelivered++;
            Player.Instance.IncrementTotalPackagesDelivered();
            Debug.Log($"Delivered package to {package.address}");
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
        return null;
    }

    public void CheckAllPackagesDelivered()
    {
        arePackagesDelivered = packages.TrueForAll(package => package.isDelivered);

        if (arePackagesDelivered)
        {
            Debug.Log("All packages have been delivered! Day's work is complete. Return to the Depot.");
        }
    }

    public bool ArePackagesDelivered()
    {
        return arePackagesDelivered;
    }

    public void ResetForNewDay()
    {
        UpdateCurrentDay();
        GeneratePackages();
    }

    private void UpdateCurrentDay()
    {
        currentDay = DayManager.Instance.currentDay.ToString();
    }
}

[System.Serializable]
public class AddressList
{
    public List<string> addresses;
}
