using UnityEngine;

[CreateAssetMenu(fileName = "Mystery Box Skills", menuName = "Scriptable Objects/Mystery Box Skills")]
public class MysteryBoxSkillsSO : ScriptableObject
{
    [SerializeField] private string _skillName;
    [SerializeField] private Sprite _skillIcon;
    [SerializeField] private SkillType _skillType;
    [SerializeField] private SkillUsageType _skillUsageType;
    [SerializeField] private SkillDataSO _skillData;

    public string SkillName => _skillName;
    public Sprite SkillIcon => _skillIcon;
    public SkillDataSO SkillData => _skillData;
    public SkillType SkillType => _skillType;
    public SkillUsageType SkillUsageType => _skillUsageType;
}
