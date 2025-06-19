using Unity.Netcode;
using UnityEngine;

public class MysteryBoxCollectible : NetworkBehaviour, ICollectible
{

    [Header("References")]
    [SerializeField] private MysteryBoxSkillsSO[] _mysteryBoxSkills;
    [SerializeField] private Collider _collider;
    [SerializeField] private Animator _animator;

    [Header("Settings")]
    [SerializeField] private float _respawnTimer;

    public void Collect(PlayerSkillController skillController)
    {
        if (skillController.HasSkillAlready) return;
        
        var skill = GetRandomSkill();
        SkillsUI.Instance.SetSkill(skill);
        skillController.SetSkill(skill);

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
    private MysteryBoxSkillsSO GetRandomSkill()
    {
        var randomIndex = Random.Range(0, _mysteryBoxSkills.Length);
        return _mysteryBoxSkills[randomIndex];
    }
}
