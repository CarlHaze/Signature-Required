using UnityEngine;

// Add this component to objects that can be picked up
public class PickupProperties : MonoBehaviour
{
    public bool requiresTwoHands = false;
    public Vector3 holdOffset = Vector3.zero;
    public Vector3 holdRotation = Vector3.zero;
}