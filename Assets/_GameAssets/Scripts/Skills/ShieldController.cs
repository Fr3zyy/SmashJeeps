using Unity.Netcode;

public class ShieldController : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        PlayerSkillController.OnTimerFinished += PlayerSkillController_OnTimerFinished;
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
    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;
        PlayerSkillController.OnTimerFinished -= PlayerSkillController_OnTimerFinished;
    }
}
