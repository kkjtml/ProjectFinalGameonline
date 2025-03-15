using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.SocialPlatforms.Impl;

public class HPPlayerScript : NetworkBehaviour
{
    TMP_Text p1Text;
    TMP_Text p2Text;
    MainPlayerMovement mainPlayer;
    public NetworkVariable<int> hpP1 = new NetworkVariable<int>(5,
    NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public NetworkVariable<int> hpP2 = new NetworkVariable<int>(5,
    NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private OwnerNetworkAnimationScript ownerNetworkAnimationScript;

    private Vector3 spawnPosition; //เก็บตำแหน่ง spawn

    // Start is called before the first frame update
    void Start()
    {
        p1Text = GameObject.Find("P1HPText (TMP)").GetComponent<TMP_Text>();
        p2Text = GameObject.Find("P2HPText (TMP)").GetComponent<TMP_Text>();
        mainPlayer = GetComponent<MainPlayerMovement>();
        ownerNetworkAnimationScript = GetComponent<OwnerNetworkAnimationScript>();

        spawnPosition = transform.position; // เก็บตำแหน่ง spawn เริ่มต้น
    }

    private void UpdatePlayerNameAndScore()
    {
        if (IsOwnedByServer)
        {
            p1Text.text = $"{mainPlayer.playerNameA.Value} : {hpP1.Value}";
        }
        else
        {
            p2Text.text = $"{mainPlayer.playerNameB.Value} : {hpP2.Value}";
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdatePlayerNameAndScore();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsLocalPlayer) return;

        if (collision.gameObject.tag == "DeathZone")
        {
            if (IsOwnedByServer)
            {
                hpP1.Value--;
            }
            else
            {
                hpP2.Value--;
            }
            gameObject.GetComponent<PlayerSpawnerScript>().Respawn();
        }
        else if (collision.gameObject.CompareTag("Bomb"))
        {
            if (IsOwnedByServer)
            {
                // ลด HP ของ P1
                if (hpP1.Value > 0)
                {
                    DecreaseHPServerRpc(1, true); // ลด HP P1
                    ownerNetworkAnimationScript.SetTrigger("Hurt");
                }
            }
            else
            {
                // ลด HP ของ P2
                if (hpP2.Value > 0)
                {
                    DecreaseHPServerRpc(1, false); // ลด HP P2
                    ownerNetworkAnimationScript.SetTrigger("Hurt");
                }
            }
        }
        else if (collision.gameObject.CompareTag("Heart"))
        {
            if (IsOwnedByServer)
            {
                hpP1.Value++;
                ownerNetworkAnimationScript.SetTrigger("Glad");
            }
            else
            {
                hpP2.Value++;
                ownerNetworkAnimationScript.SetTrigger("Glad");
            }
        }
    }

    [ServerRpc]
    void DecreaseHPServerRpc(int damage, bool isPlayer1)
    {
        if (isPlayer1)
        {
            hpP1.Value -= damage;
            if (hpP1.Value <= 0)
            {
                ownerNetworkAnimationScript.SetTrigger("Dead");
                RespawnPlayer();
            }
        }
        else
        {
            hpP2.Value -= damage;
            if (hpP2.Value <= 0)
            {
                ownerNetworkAnimationScript.SetTrigger("Dead");
                RespawnPlayer();
            }
        }
    }

    public void RespawnPlayer() // ฟังก์ชันให้ผู้เล่น respawn ที่ตำแหน่งที่เคยเกิด
    {
        Vector3 spawnPos = LoginManagerScipt.Instance.lastSpawnPosition;  // Retrieve spawn position from LoginManager
        transform.position = spawnPos; // รีเซ็ตตำแหน่งผู้เล่น

        // รีเซ็ตค่า HP ให้กลับไปเป็น 5
        if (IsOwnedByServer)
        {
            hpP1.Value = 5;  // ตั้งค่า HP ของผู้เล่นคนที่ 1
        }
        else
        {
            hpP2.Value = 5;  // ตั้งค่า HP ของผู้เล่นคนที่ 2
        }

        // หยุดแอนิเมชัน "Dead" ถ้า HP มากกว่า 0
        ownerNetworkAnimationScript.ResetTrigger("Dead");
    }
}