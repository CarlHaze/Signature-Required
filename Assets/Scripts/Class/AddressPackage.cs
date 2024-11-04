using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class AddressPackage
{
    public string address;
    public int numberOfPackages = 0;

    public AddressPackage(string address, int numberOfPackages)
    {
        this.address = address;
        this.numberOfPackages = numberOfPackages;
    }
}
