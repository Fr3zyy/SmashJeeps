using Unity.Netcode;

public class FakeBoxDamageable : NetworkBehaviour, IDamageable
{
    public void Damage(PlayerVehicleController vehicleController)
    {
        vehicleController.CrashVehicle();
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
}
