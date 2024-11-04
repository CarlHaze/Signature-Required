[System.Serializable]
public class Package
{
    public string address; // The address for delivery
    public bool isDelivered; // Indicates if the package has been delivered
    public bool isCollected; // Indicates if the package has been collected

    public Package(string address)
    {
        this.address = address;
        this.isDelivered = false; // Set initial delivery status
        this.isCollected = false; // Set initial collection status
    }
}
