using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CameraManager : MonoBehaviour
{
    public Camera hostCamera; // กล้องของ Host
    public Camera clientCamera; // กล้องของ Client

    public Transform hostPlayer; // ตัวแปรที่เก็บผู้เล่นที่เป็น Host
    public Transform clientPlayer; // ตัวแปรที่เก็บผู้เล่นที่เป็น Client

    private void Start()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            // หากเป็น Host, เปิดกล้องของ Host และปิดกล้องของ Client
            hostCamera.gameObject.SetActive(true);
            clientCamera.gameObject.SetActive(false);

            // ตั้งค่าผู้เล่นที่เป็น Host
            hostPlayer = GameObject.FindWithTag("HostPlayer").transform;
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            // หากเป็น Client, เปิดกล้องของ Client และปิดกล้องของ Host
            hostCamera.gameObject.SetActive(false);
            clientCamera.gameObject.SetActive(true);

            // ตั้งค่าผู้เล่นที่เป็น Client
            clientPlayer = GameObject.FindWithTag("ClientPlayer").transform;
        }
    }

    private void Update()
    {
        if (NetworkManager.Singleton.IsHost && hostPlayer != null)
        {
            // Host camera ติดตาม Host player
            Vector3 targetPosition = hostPlayer.position;
            targetPosition.y += 5f; // เพิ่มความสูงให้กล้อง
            targetPosition.z -= 10f; // กำหนดระยะห่างจากผู้เล่น

            hostCamera.transform.position = targetPosition;
            hostCamera.transform.LookAt(hostPlayer); // ให้กล้องมองผู้เล่น Host
        }
        else if (NetworkManager.Singleton.IsClient && clientPlayer != null)
        {
            // Client camera ติดตาม Client player
            Vector3 targetPosition = clientPlayer.position;
            targetPosition.y += 5f; // เพิ่มความสูงให้กล้อง
            targetPosition.z -= 10f; // กำหนดระยะห่างจากผู้เล่น

            clientCamera.transform.position = targetPosition;
            clientCamera.transform.LookAt(clientPlayer); // ให้กล้องมองผู้เล่น Client
        }
    }
}