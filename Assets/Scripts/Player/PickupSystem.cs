using UnityEngine;

public class PickupSystem : MonoBehaviour
{
    [Header("Pickup Settings")]
    public float pickupRange = 3f;
    public float throwForce = 10f;
    public float pickupSphereRadius = 0.5f;
    public LayerMask pickupLayer;
    public Transform rightHandTransform;
    public Transform leftHandTransform;
    public Transform twoHandedPosition;

    [Header("Hand Colliders")]
    public BoxCollider rightHandCollider; 
    public BoxCollider leftHandCollider;   

    [Header("Input Keys")]
    public KeyCode pickupKey = KeyCode.E;
    public KeyCode throwKey = KeyCode.F;
    public KeyCode switchHandKey = KeyCode.Q;

    private GameObject _heldObject;
    private Rigidbody _heldRigidbody;
    private Collider _heldObjectCollider;
    private bool _isHoldingObject;
    private bool _isTwoHanded;          
    private Transform _currentHand;
    
    // Automatically found components
    private Collider[] _playerColliders;
    
    void Start()
    {
        Debug.Log("PickupSystem Started");
        
        // Only get the colliders we actually use
        _playerColliders = GetComponentsInChildren<Collider>();

        if (_playerColliders.Length == 0)
            Debug.LogWarning("No colliders found on player or children!");

        if (!rightHandTransform || !leftHandTransform || !twoHandedPosition)
            Debug.LogError("Hand transforms not properly assigned!");

        _currentHand = rightHandTransform;
    }

    void Update()
    {
        if (!_isHoldingObject)
        {
            CheckForPickup();
        }
        else
        {
            HandleHeldObject();
        }
    }
    private readonly Collider[] _hitColliders = new Collider[10];
    private void CheckForPickup()
    {
        if (!Input.GetKeyDown(pickupKey)) return;
        Debug.Log("Pickup key pressed (E)");

        var pickupPosition = transform.position + (transform.forward * 0.5f);
        var numColliders = Physics.OverlapSphereNonAlloc(pickupPosition, pickupSphereRadius, _hitColliders, pickupLayer);

        if (numColliders <= 0) return;
        var closestDistance = float.MaxValue;
        GameObject closestObject = null;

        for (var i = 0; i < numColliders; i++)
        {
            var col = _hitColliders[i];
            if (col == null) continue;
            
            var distance = Vector3.Distance(pickupPosition, col.transform.position);
            if (!(distance < closestDistance) || !(distance <= pickupRange)) continue;
            closestDistance = distance;
            closestObject = col.gameObject;
        }

        if (!closestObject) return;
        Debug.Log($"Found pick up object: {closestObject.name}");
        PickupObject(closestObject);
    }

    void PickupObject(GameObject obj)
    {
        if (obj == null) return;
        Debug.Log($"Attempting to pick up: {obj.name}");
        
        _heldObject = obj;
        _heldRigidbody = obj.GetComponent<Rigidbody>();
        _heldObjectCollider = obj.GetComponent<Collider>();
        
        if (_heldRigidbody != null)
        {
            _heldRigidbody.isKinematic = true;
            _heldRigidbody.useGravity = false;
            _heldRigidbody.interpolation = RigidbodyInterpolation.None;

            if (_heldObjectCollider != null && _playerColliders != null)
            {
                foreach (var playerCol in _playerColliders)
                {
                    if (playerCol != null && playerCol.enabled)
                    {
                        Physics.IgnoreCollision(_heldObjectCollider, playerCol, true);
                        Debug.Log($"Ignoring collisions between {_heldObjectCollider.name} and {playerCol.name}");
                    }
                }
            }
        }

        var props = obj.GetComponent<PickupProperties>();
        if (props != null)
        {
            _isTwoHanded = props.requiresTwoHands;
            _currentHand = _isTwoHanded ? twoHandedPosition : rightHandTransform;
        }

        if (_currentHand == null)
        {
            Debug.LogError("Current hand transform is null!");
            return;
        }

        // Disable hand colliders based on grip type
        if (_isTwoHanded)
        {
            DisableHandColliders(true, true);
        }
        else
        {
            if (_currentHand == rightHandTransform)
                DisableHandColliders(true, false);
            else
                DisableHandColliders(false, true);
        }

        obj.transform.SetParent(_currentHand);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
        
        _isHoldingObject = true;
    }

    void HandleHeldObject()
    {
        if (Input.GetKeyDown(switchHandKey) && !_isTwoHanded)
        {
            Debug.Log("Switching hands");
            // Enable the previous hand's collider and disable the new hand's collider
            if (_currentHand == rightHandTransform)
            {
                DisableHandColliders(false, true);
                _currentHand = leftHandTransform;
            }
            else
            {
                DisableHandColliders(true, false);
                _currentHand = rightHandTransform;
            }

            _heldObject.transform.SetParent(_currentHand);
            _heldObject.transform.localPosition = Vector3.zero;
            _heldObject.transform.localRotation = Quaternion.identity;
        }

        if (Input.GetKeyDown(throwKey))
        {
            ThrowObject();
        }
    }

    void ThrowObject()
    {
        if (_heldObject != null)
        {
            Debug.Log($"Throwing object: {_heldObject.name}");
            
            // Re-enable collisions with ALL player colliders
            if (_heldObjectCollider != null)
            {
                foreach (var playerCol in _playerColliders)
                {
                    if (playerCol != null && playerCol.enabled)
                    {
                        Physics.IgnoreCollision(_heldObjectCollider, playerCol, false);
                    }
                }
            }

            _heldObject.transform.SetParent(null);
            
            if (_heldRigidbody != null)
            {
                _heldRigidbody.isKinematic = false;
                _heldRigidbody.useGravity = true;
                _heldRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
                
                Vector3 throwDirection = Camera.main.transform.forward;
                _heldRigidbody.linearVelocity = Vector3.zero;
                _heldRigidbody.angularVelocity = Vector3.zero;
                _heldRigidbody.AddForce(throwDirection * throwForce, ForceMode.Impulse);
            }

            EnableHandColliders();

            _isHoldingObject = false;
            _heldObject = null;
            _heldRigidbody = null;
            _heldObjectCollider = null;
        }
    }

    void DisableHandColliders(bool rightHand, bool leftHand)
    {
        if (rightHand && rightHandCollider != null)
            rightHandCollider.enabled = false;
        if (leftHand && leftHandCollider != null)
            leftHandCollider.enabled = false;
    }

    void EnableHandColliders()
    {
        if (rightHandCollider != null)
            rightHandCollider.enabled = true;
        if (leftHandCollider != null)
            leftHandCollider.enabled = true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 pickupPosition = transform.position + (transform.forward * 0.5f);
        Gizmos.DrawWireSphere(pickupPosition, pickupSphereRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, pickupPosition + (transform.forward * pickupRange));
    }
}