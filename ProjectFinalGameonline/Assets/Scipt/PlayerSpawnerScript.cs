using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.Multiplayer.Samples.Utilities.ClientAuthority; // ต้องใช้เพื่อเข้าถึง ClientNetworkTransform

public class PlayerSpawnerScript : NetworkBehaviour
{
    //MainPlayerScript mainPlayer;
    public Behaviour[] scripts;
    private Renderer[] renderers;
    private ClientNetworkTransform clientNetworkTransform;

    void Start()
    {
        //mainPlayer = gameObject.GetComponent<MainPlayerScript>();
        renderers = GetComponentsInChildren<Renderer>();
        clientNetworkTransform = GetComponent<ClientNetworkTransform>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // ให้เฉพาะ Owner เท่านั้นที่ตั้งค่ากล้องของตัวเอง
        if (IsOwner)
        {
            CameraManager.Instance.SetPlayer(gameObject);
        }
    }
    
    void SetPlayerState(bool state)
    {
        foreach (var script in scripts) { script.enabled = state; }
        foreach (var renderer in renderers) { renderer.enabled = state; }
    }

    private Vector3 GetRandomPos()
    {
        // If you want to ensure the host and client have fixed spawn positions:
        if (IsHost())
        {
            // Host always spawns at spawn point 1 (index 0)
            return LoginManagerScipt.Instance.spawnPoint[0].transform.position;
        }
        else
        {
            // Client always spawns at spawn point 2 (index 1)
            return LoginManagerScipt.Instance.spawnPoint[1].transform.position;
        }
    }

    private bool IsHost()
    {
        return NetworkManager.Singleton.IsHost;
    }

    public void Respawn()
    {
        RespawnServerRpc();
    }

    [ServerRpc]
    void RespawnServerRpc()
    {
        Vector3 pos = GetRandomPos();
        RespawnClientRpc(pos);
    }

    [ClientRpc]
    void RespawnClientRpc(Vector3 spawnPos)
    {
        StartCoroutine(RespawnCoroutine(spawnPos));
    }

    IEnumerator RespawnCoroutine(Vector3 spawnPos)
    {
        SetPlayerState(false);

        // ปิด Interpolation ก่อนเปลี่ยนตำแหน่ง
        if (clientNetworkTransform != null)
        {
            clientNetworkTransform.Interpolate = false;
        }

        transform.position = spawnPos;  // เปลี่ยนตำแหน่งทันที
        yield return new WaitForSeconds(3f);

        // เปิด Interpolation กลับมาให้เคลื่อนไหวลื่นไหล
        if (clientNetworkTransform != null)
        {
            clientNetworkTransform.Interpolate = true;
        }

        SetPlayerState(true);
    }
}
