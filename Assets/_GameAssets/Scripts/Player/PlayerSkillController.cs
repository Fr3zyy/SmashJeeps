using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerSkillController : NetworkBehaviour
{
    public static event Action OnTimerFinished;

    private MysteryBoxSkillsSO _currentSkill;

    private bool _isSkillUsed = false;
    public MysteryBoxSkillsSO CurrentSkill => _currentSkill;
    public bool HasSkillAlready => _hasSkillAlready;
    private bool _hasTimerStarted;
    private float _timer;
    private float _timerMax;
    private int _mineAmountCounter;

    [Header("References")]
    [SerializeField] private Transform _rocketLauncherTransform;
    [SerializeField] private Transform _rocketLaunchPoint;

    [Header("Settings")]
    [SerializeField] private float _resetDelay;
    [SerializeField] private bool _hasSkillAlready;

    private void Start()
    {
        if (!IsOwner)
            this.enabled = false;
    }
    private void Update()
    {
        if (!IsOwner) return;
        if (Input.GetKeyDown(KeyCode.Space) && !_isSkillUsed)
        {
            UseSkill();
            _isSkillUsed = true;
        }

        if (_hasTimerStarted)
        {
            _timer -= Time.deltaTime;
            SkillsUI.Instance.SetTimerCounterText((int)_timer);
            if (_timer <= 0)
            {
                OnTimerFinished?.Invoke();
                SkillsUI.Instance.SetSkillToNone();
                _hasTimerStarted = false;
                _hasSkillAlready = false;
            }
        }
    }

    public void UseSkill()
    {
        if (!IsOwner || !_hasSkillAlready) return;

        SkillManager.Instance.ActivateSkill(_currentSkill.SkillType, transform, OwnerClientId);

        SetSkillToNone();

        if (_currentSkill.SkillType == SkillType.Rocket)
        {
            StartCoroutine(ResetRocketLauncher());
        }

    }

    private void SetSkillToNone()
    {
        if (_currentSkill.SkillUsageType == SkillUsageType.None)
        {
            _hasSkillAlready = false;
            SkillsUI.Instance.SetSkillToNone();
        }

        if (_currentSkill.SkillUsageType == SkillUsageType.Timer)
        {
            _hasTimerStarted = true;
            _timerMax = _currentSkill.SkillData.SpawnAmountOrTimer;
            _timer = _timerMax;
        }

        if (_currentSkill.SkillUsageType == SkillUsageType.Amount)
        {
            _mineAmountCounter = _currentSkill.SkillData.SpawnAmountOrTimer;

            SkillManager.Instance.OnMineCountReduced += OnMineCountReduced;
        }
    }

    private void OnMineCountReduced()
    {
        _mineAmountCounter--;
        SkillsUI.Instance.SetTimerCounterText(_mineAmountCounter);
        if (_mineAmountCounter <= 0)
        {
            _hasSkillAlready = false;
            SkillsUI.Instance.SetSkillToNone();
            SkillManager.Instance.OnMineCountReduced -= OnMineCountReduced;
        }
    }

    public void SetSkill(MysteryBoxSkillsSO skill)
    {
        _currentSkill = skill;

        if (_currentSkill.SkillType == SkillType.Rocket)
        {
            SetRocketLauncherActiveRpc(true);
        }

        _hasSkillAlready = true;
        _isSkillUsed = false;
    }
    [Rpc(SendTo.ClientsAndHost)]
    private void SetRocketLauncherActiveRpc(bool active)
    {
        _rocketLauncherTransform.gameObject.SetActive(active);
    }
    private IEnumerator ResetRocketLauncher()
    {
        yield return new WaitForSeconds(_resetDelay);
        SetRocketLauncherActiveRpc(false);
    }
    public Vector3 GetRocketPointPosition()
    {
        return _rocketLaunchPoint.position;
    }
}
