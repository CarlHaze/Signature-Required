// IDamageable.cs
using UnityEngine;

public interface IDamageable
{
    void TakeDamage(int damage, Vector3 hitPosition, bool isJab);
}