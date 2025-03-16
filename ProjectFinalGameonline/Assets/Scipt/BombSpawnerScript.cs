using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

public class BombSpawnerScript : NetworkBehaviour
{
    public GameObject bombPrefab;
    private List<GameObject> spawnedBomb = new List<GameObject>();
    private OwnerNetworkAnimationScript ownerNetworkAnimationScript;

    void Start()
    {
        ownerNetworkAnimationScript = GetComponent<OwnerNetworkAnimationScript>();
        if (IsServer) // ให้เซิร์ฟเวอร์เริ่มสุ่มเกิดระเบิด
        {
            StartCoroutine(SpawnBombRoutine());
        }
    }

    IEnumerator SpawnBombRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(2f); // Spawn ทุก 1 วินาที

            float randomX = Random.Range(-1f, 1.0f);
            float randomZ = Random.Range(25.0f, 26.0f);
            Vector3 spawnPos = transform.position + new Vector3(randomX, 10f, randomZ);

            SpawnBombServerRpc(spawnPos);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void SpawnBombServerRpc(Vector3 spawnPos)
    {
        Quaternion spawnRot = Quaternion.identity;
        GameObject bomb = Instantiate(bombPrefab, spawnPos, spawnRot);

        // ตรวจสอบว่ามี Rigidbody หรือไม่
        Rigidbody rb = bomb.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = bomb.AddComponent<Rigidbody>(); // ถ้าไม่มี ให้เพิ่ม Rigidbody เข้าไป
        }

        rb.useGravity = true;  // เปิดใช้งานแรงโน้มถ่วง
        rb.isKinematic = false; // ปิด Kinematic เพื่อให้ Physics ทำงาน
        rb.mass = 1.5f;          // ตั้งค่ามวลของระเบิด
        rb.drag = 0f;          // ไม่มีแรงต้านอากาศ
        rb.angularDrag = 0.05f;// ค่าแรงต้านการหมุน

        NetworkObject networkObject = bomb.GetComponent<NetworkObject>();
        networkObject.Spawn(); // Spawn ให้กับ client

        spawnedBomb.Add(bomb);
        bomb.GetComponent<BombScript>().bombSpawner = this;
    }

    [ServerRpc(RequireOwnership = false)]
    public void DestroyServerRpc(ulong networkObjectId)
    {
        GameObject toDestory = findBombFromNetworkId(networkObjectId);
        if (toDestory == null) return;

        toDestory.GetComponent<NetworkObject>().Despawn();
        spawnedBomb.Remove(toDestory);
        Destroy(toDestory);
    }

    private GameObject findBombFromNetworkId(ulong networkObjectId)
    {
        foreach (GameObject bomb in spawnedBomb)
        {
            ulong bombId = bomb.GetComponent<NetworkObject>().NetworkObjectId;
            Debug.Log("bombId  " + bombId);
            if (bombId == networkObjectId)
            {
                return bomb;
            }
        }
        return null;
    }
}
