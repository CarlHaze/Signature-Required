using UnityEngine;
using UnityEngine.InputSystem;

public class PackageThrow : MonoBehaviour
{
    [SerializeField] private GameObject packagePrefab;
    [SerializeField] private float throwForce = 10f;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ThrowPackages();
        }
    }

    private void ThrowPackages()
    {
        int packagesToThrow = DeliveryManager.Instance.TotalDelivered;

        for (int i = 0; i < packagesToThrow; i++)
        {
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = Camera.main.nearClipPlane;
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);

            GameObject package = Instantiate(packagePrefab, transform.position, Quaternion.identity);
            Rigidbody rb = package.GetComponent<Rigidbody>();

            if (rb != null)
            {
                Vector3 direction = (worldPosition - transform.position).normalized;
                rb.AddForce(direction * throwForce, ForceMode.Impulse);
                Debug.Log($"Package thrown towards: {worldPosition} with force: {direction * throwForce}");
            }
            else
            {
                Debug.LogError("Package prefab does not have a Rigidbody component.");
            }
        }
    }
}
