using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using Unity.Netcode;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }
    public CinemachineVirtualCamera cinemachineCamera;
    public GameObject hostPlayer;
    public GameObject clientPlayer;


    // ปรับค่ามุมกล้องแบบ Third-Person
    public Vector3 thirdPersonOffset = new Vector3(0, 5, -8);
    public Vector3 cameraRotation = new Vector3(30, 0, 0); // หมุนกล้องให้เงยขึ้น 30 องศา

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void LateUpdate()
    {
        if (!NetworkManager.Singleton.IsConnectedClient) return;

        if (hostPlayer != null && hostPlayer.TryGetComponent(out NetworkObject hostNetworkObject) && hostNetworkObject.IsOwner)
        {
            SetCameraFollow(hostPlayer);
        }
        else if (clientPlayer != null && clientPlayer.TryGetComponent(out NetworkObject clientNetworkObject) && clientNetworkObject.IsOwner)
        {
            SetCameraFollow(clientPlayer);
        }
    }

    public void SetPlayer(GameObject player)
    {
        if (player == null) return;

        if (NetworkManager.Singleton.IsHost)
        {
            hostPlayer = player;
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            clientPlayer = player;
        }

        SetCameraFollow(player);
    }

    private void SetCameraFollow(GameObject player)
    {
        if (player == null || cinemachineCamera == null) return;

        cinemachineCamera.Follow = player.transform;
        cinemachineCamera.LookAt = player.transform;

        // ใช้ Framing Transposer เพื่อควบคุมมุมกล้อง
        var transposer = cinemachineCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        if (transposer != null)
        {
            transposer.m_CameraDistance = 10f;
            transposer.m_TrackedObjectOffset = new Vector3(0, 3, 0);
            transposer.m_XDamping = 1.0f;
            transposer.m_YDamping = 1.0f;
            transposer.m_ZDamping = 1.0f;
        }

        // หมุนกล้องให้มีมุมเงยขึ้น 30 องศา
        cinemachineCamera.transform.rotation = Quaternion.Euler(cameraRotation);
    }
}