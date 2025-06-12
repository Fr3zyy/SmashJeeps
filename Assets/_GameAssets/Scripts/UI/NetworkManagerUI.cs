using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Button _startHostButton;
    [SerializeField] private Button _startClientButton;


    private void Awake()
    {
        _startHostButton.onClick.AddListener(OnStartHostButtonClicked);
        _startClientButton.onClick.AddListener(OnStartClientButtonClicked);
    }

    private void OnStartClientButtonClicked()
    {
        NetworkManager.Singleton.StartClient();
        Hide();
    }

    private void OnStartHostButtonClicked()
    {
        NetworkManager.Singleton.StartHost();
        Hide();
    }
    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
