using UnityEngine;

public interface ICollectible
{
    void Collect(PlayerSkillController skillController);
    void CollectRpc();
}
