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
        else if (collision.gameObject.tag == "Bomb")
        {
            if (IsOwnedByServer)
            {
                // เช็คว่า hpP1 ลดลงได้จริง
                if (hpP1.Value > 0)
                {
                    hpP1.Value--;
                    ownerNetworkAnimationScript.SetTrigger("Hurt");
                    if (hpP1.Value <= 0)
                    {
                        ownerNetworkAnimationScript.SetTrigger("Dead");
                        RespawnPlayer();  // เรียก respawn เมื่อ HP = 0
                    }
                }
            }
            else
            {
                // เช็คว่า hpP2 ลดลงได้จริง
                if (hpP2.Value > 0)
                {
                    hpP2.Value--;
                    ownerNetworkAnimationScript.SetTrigger("Hurt");
                    if (hpP2.Value <= 0)
                    {
                        ownerNetworkAnimationScript.SetTrigger("Dead");
                        RespawnPlayer();  // เรียก respawn เมื่อ HP = 0
                    }
                }
            }
        }
        else if (collision.gameObject.tag == "Heart")
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

    public void RespawnPlayer()  //ฟังก์ชันให้ผู้เล่น respawn ที่ตำแหน่งที่เคยเกิด
    {
        Vector3 spawnPos = LoginManagerScipt.Instance.lastSpawnPosition;  // Retrieve spawn position from LoginManager
        transform.position = spawnPosition;

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
