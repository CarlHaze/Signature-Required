using UnityEngine;

public class House : MonoBehaviour
{
    public string address; // The address of the house
    public DropOffArea dropOffArea; // Reference to the DropOffArea component

    private void Awake()
    {
        // Set the address to the name of the GameObject
        address = gameObject.name;

        // Find the DropOffArea child component
        dropOffArea = GetComponentInChildren<DropOffArea>();
    }
}
