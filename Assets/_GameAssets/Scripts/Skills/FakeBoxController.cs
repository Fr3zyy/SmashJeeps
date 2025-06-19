using DG.Tweening;
using Unity.Netcode;
using UnityEngine;

public class FakeBoxController : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Canvas _fakeBoxCanvas;
    [SerializeField] private Collider _fakeBoxCollider;
    [SerializeField] private RectTransform _arrowTransform;

    [Header("Settings")]
    [SerializeField] private float _animationDuration;


    private Tween _arrowTween;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            SetOwnerVisualsRpc();
        }
    }

    [Rpc(SendTo.Owner)]
    private void SetOwnerVisualsRpc()
    {
        _fakeBoxCanvas.gameObject.SetActive(true);
        _fakeBoxCanvas.worldCamera = Camera.main;
        _fakeBoxCollider.enabled = false;


        _arrowTween = _arrowTransform.DOAnchorPosY(-1, _animationDuration).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
    }

    public override void OnNetworkDespawn()
    {
        _arrowTween.Kill();
    }
}
