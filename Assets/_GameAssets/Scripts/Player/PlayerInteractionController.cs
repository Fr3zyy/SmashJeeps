using Unity.Netcode;
using UnityEngine;

public class PlayerInteractionController : NetworkBehaviour
{
    [SerializeField] private PlayerSkillController _skillController;

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;
        _skillController = GetComponent<PlayerSkillController>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) return;

        if (other.gameObject.TryGetComponent(out ICollectible collectible))
        {
            collectible.Collect(_skillController);
        }
    }
}