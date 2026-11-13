using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class SetupManager : MonoBehaviour
{
    public List<GameObject> playerList;
    public int playerTotal = 0;
    public Transform[] startPos;

    //public List<TMP_Text> playerTexts = new List<TMP_Text>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        int i = 0;
        foreach (GameObject g in GGameManager.instance.playerList)
        {
            GameObject prefab = g.GetComponent<GopherSetup>().ourGopher.gopherGame;
            g.transform.position = startPos[i].position;
            Instantiate(prefab, g.transform);
            i++;
        }
    }

    void OnPlayerJoined(PlayerInput pi)
    {
        print("Player Joined: " + pi.gameObject.name);
        //playerList.Add(pi.gameObject);
        pi.SwitchCurrentActionMap(pi.gameObject.name);
    }
}
