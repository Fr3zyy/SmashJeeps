using Unity.Netcode;
using UnityEngine;

public class MysteryBoxCollectible : NetworkBehaviour, ICollectible
{

    [Header("References")]
    [SerializeField] private Collider _collider;
    [SerializeField] private Animator _animator;

    [Header("Settings")]
    [SerializeField] private float _respawnTimer;

    public void Collect()
    {
        CollectRpc();
    }
    [Rpc(SendTo.ClientsAndHost)]
    public void CollectRpc()
    {
        AnimateCollection();
        Invoke(nameof(Respawn), _respawnTimer);
    }
    private void AnimateCollection()
    {
        _collider.enabled = false;
        _animator.SetTrigger(Consts.BoxAnimations.IS_COLLECTED);
    }
    private void Respawn()
    {
        _animator.SetTrigger(Consts.BoxAnimations.IS_RESPAWNED);
        _collider.enabled = true;
    }
}
