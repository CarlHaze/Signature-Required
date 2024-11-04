using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Package
{
    public string address; 
    public bool isCollected = false; 
    public bool isDelivered = false; 


    public Package(string address)
    {
        this.address = address;
    }

    // Method to set the address from a list of pre-written addresses
    public void SetAddressFromList(List<string> addresses, int index)
    {
        if (index >= 0 && index < addresses.Count)
        {
            this.address = addresses[index];
        }
        else
        {
            Debug.LogError("Index out of range");
        }
    }
}
