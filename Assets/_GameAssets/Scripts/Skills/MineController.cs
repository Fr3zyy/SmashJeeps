using Unity.Netcode;
using UnityEngine;

public class MineController : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Collider _mineCollider;


    [Header("Settings")]
    [SerializeField] private float _fallSpeed;
    [SerializeField] private float _raycastDistance;
    [SerializeField] private LayerMask _groundLayer;

    private bool _hasLanded;
    private Vector3 _lastSendPosition;
    private void Update()
    {
        if (!IsServer || _hasLanded) return;

        if (Physics.Raycast(transform.position, Vector3.down, out var hit, _raycastDistance, _groundLayer))
        {
            _hasLanded = true;
            transform.position = hit.point;
            // reset rotation to 0.0.0
            transform.rotation = Quaternion.identity;
            if (_lastSendPosition != transform.position)
            {
                SyncPositionRpc(transform.position, transform.rotation);
                _lastSendPosition = transform.position;
            }
        }
        else
        {
            transform.position += _fallSpeed * Vector3.down * Time.deltaTime;
            transform.rotation = Quaternion.identity;
            if (_lastSendPosition != transform.position)
            {
                SyncPositionRpc(transform.position, transform.rotation);
                _lastSendPosition = transform.position;
            }
        }
    }
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            SetOwnerVisualsRpc();
        }
    }
    [Rpc(SendTo.ClientsAndHost)]
    private void SyncPositionRpc(Vector3 newPosition, Quaternion newRotation)
    {
        if (IsServer) return;

        transform.position = newPosition;
        transform.rotation = newRotation;
    }
    [Rpc(SendTo.Owner)]
    private void SetOwnerVisualsRpc()
    {
        _mineCollider.enabled = false;
    }
}
