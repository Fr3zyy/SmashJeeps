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
            if (_lastSendPosition != transform.position)
            {
                SyncPositionRpc(transform.position);
                _lastSendPosition = transform.position;
            }
        }
        else
        {
            transform.position += _fallSpeed * Vector3.down * Time.deltaTime;
            if (_lastSendPosition != transform.position)
            {
                SyncPositionRpc(transform.position);
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
    private void SyncPositionRpc(Vector3 newPosition)
    {
        if (IsServer) return;

        transform.position = newPosition;
    }
    [Rpc(SendTo.Owner)]
    private void SetOwnerVisualsRpc()
    {
        _mineCollider.enabled = false;
    }
}
