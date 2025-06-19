using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillsUI : MonoBehaviour
{
    public static SkillsUI Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Image _skillImage;
    [SerializeField] private TMP_Text _skillNameText;
    [SerializeField] private TMP_Text _timerCounterText;
    [SerializeField] private Transform _timerCounterParentTransform;

    [Header("Settings")]
    [SerializeField] private float _scaleDuration = 0.5f;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        SetSkillToNone();
    }
    public void SetSkill(MysteryBoxSkillsSO skill)
    {
        _skillImage.gameObject.SetActive(true);
        _skillImage.sprite = skill.SkillIcon;
        _skillNameText.text = skill.SkillName;

        if (skill.SkillUsageType == SkillUsageType.Timer || skill.SkillUsageType == SkillUsageType.Amount)
        {
            SetTimerCounterAnimation(skill.SkillData.SpawnAmountOrTimer);
        }
    }
    public void SetTimerCounterAnimation(int timerCounter)
    {
        if (_timerCounterParentTransform.gameObject.activeInHierarchy) return;

        _timerCounterParentTransform.gameObject.SetActive(true);
        _timerCounterParentTransform.DOScale(1.3f, _scaleDuration).SetEase(Ease.OutBack);
        _timerCounterText.text = timerCounter.ToString();
    }
    public void SetSkillToNone()
    {
        _skillImage.gameObject.SetActive(false);
        _skillNameText.text = string.Empty;
        _timerCounterParentTransform.gameObject.SetActive(false);

        if (_timerCounterParentTransform.gameObject.activeInHierarchy)
        {
            _timerCounterParentTransform.gameObject.SetActive(false);
        }
    }
    public void SetTimerCounterText(int timerCounter)
    {
        _timerCounterText.text = timerCounter.ToString();
    }
}