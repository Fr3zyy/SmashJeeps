using Unity.Netcode;
using UnityEngine;

public struct SkillTransformDataSerializable : INetworkSerializeByMemcpy
{
    public Vector3 Position { get; set; }
    public Quaternion Rotation { get; set; }
    public SkillType SkillType { get; set; }
    public NetworkObject NetworkObject { get; set; }

    public SkillTransformDataSerializable(Vector3 position, Quaternion rotation, SkillType skillType, NetworkObject networkObject)
    {
        Position = position;
        Rotation = rotation;
        SkillType = skillType;
        NetworkObject = networkObject;
    }
}
