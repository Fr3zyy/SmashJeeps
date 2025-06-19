using Unity.Netcode;
using UnityEngine;

public class RocketController : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Collider _rocketCollider;

    [Header("Settings")]
    [SerializeField] private float _rocketSpeed;
    [SerializeField] private float _rocketRotationSpeed;

    private bool _isMoving;

    private void Update()
    {
        if (IsServer && _isMoving)
        {
            MoveRocket();
        }
    }
    private void MoveRocket()
    {
        transform.position += transform.forward * _rocketSpeed * Time.deltaTime;
        transform.Rotate(Vector3.forward, _rocketRotationSpeed * Time.deltaTime, Space.Self);
    }
    [Rpc(SendTo.Server)]
    private void RequestStartMovementFromServerRpc()
    {
        _isMoving = true;
    }
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            SetOwnerVisualsRpc();
            RequestStartMovementFromServerRpc();
        }
    }

    [Rpc(SendTo.Owner)]
    private void SetOwnerVisualsRpc()
    {
        _rocketCollider.enabled = false;
    }
}
