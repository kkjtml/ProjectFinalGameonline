using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class RaceFinishManager : MonoBehaviour
{
    public TMP_Text winText;
    public TMP_Text loseText;
    public GameObject winPanel;
    public GameObject losePanel;

    private bool raceFinished = false;

    public void Start()
    {
        winPanel.SetActive(false);
        losePanel.SetActive(false);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (raceFinished) return; // Don't check once race is finished

        // ตรวจสอบว่า Object ที่ชนมี tag เป็น "Win"
        if (other.gameObject.CompareTag("Win"))
        {
            Debug.Log("Player hit the Win zone with Collision");

            // ถ้าเป็น Host หรือ Client
            if (NetworkManager.Singleton.IsHost)
            {
                Debug.Log("Host has won the race");
                EndRace(true); // Host ชนะ
            }
            else if (NetworkManager.Singleton.IsClient)
            {
                Debug.Log("Client has won the race");
                EndRace(false); // Client ชนะ
            }
        }
    }

    private void EndRace(bool isWinner)
    {
        raceFinished = true;

        // แสดง UI "You Win" สำหรับผู้ชนะ
        if (isWinner)
        {
            winPanel.SetActive(true);
            winText.text = "You Win!";
            losePanel.SetActive(false);
        }
        else
        {
            losePanel.SetActive(true);
            loseText.text = "You Lost!";
            winPanel.SetActive(false);
        }

        // Notify the other player of the result
        if (NetworkManager.Singleton.IsHost)
        {
            NotifyClientResult(true); // Host wins
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            NotifyHostResult(false); // Client loses
        }
    }

    private void NotifyClientResult(bool isWinner)
    {
        if (isWinner)
        {
            winPanel.SetActive(true);
            winText.text = "You Win!";
            losePanel.SetActive(false);
        }
        else
        {
            losePanel.SetActive(true);
            loseText.text = "You Lost!";
            winPanel.SetActive(false);
        }
    }

    private void NotifyHostResult(bool isWinner)
    {
        if (isWinner)
        {
            winPanel.SetActive(true);
            winText.text = "You Win!";
            losePanel.SetActive(false);
        }
        else
        {
            losePanel.SetActive(true);
            loseText.text = "You Lost!";
            winPanel.SetActive(false);
        }
    }
}