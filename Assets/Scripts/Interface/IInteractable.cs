using UnityEngine;

public interface IInteractable
{
    void OnPickup(Transform holder);
    void OnRelease(Vector3 throwForce);
    bool RequiresTwoHands();
}