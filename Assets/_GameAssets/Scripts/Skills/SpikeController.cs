using Unity.Netcode;
using UnityEngine;

public class SpikeController : NetworkBehaviour
{

    [SerializeField] private Collider _spikeCollider;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        PlayerSkillController.OnTimerFinished += PlayerSkillController_OnTimerFinished;
        SetOwnerVisualsRpc();
    }

    private void PlayerSkillController_OnTimerFinished()
    {
        DestroyRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void DestroyRpc()
    {
        if (IsServer)
        {
            Destroy(gameObject);
        }
    }
    [Rpc(SendTo.Owner)]
    private void SetOwnerVisualsRpc()
    {
        _spikeCollider.enabled = false;
    }
    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;
        PlayerSkillController.OnTimerFinished -= PlayerSkillController_OnTimerFinished;
    }
}
