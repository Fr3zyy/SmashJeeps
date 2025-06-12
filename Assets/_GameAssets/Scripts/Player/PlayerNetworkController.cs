using System;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetworkController : NetworkBehaviour
{

    [SerializeField] private CinemachineCamera _playerCamera;
    private void Start()
    {
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        _playerCamera.gameObject.SetActive(IsOwner);
    }
}
