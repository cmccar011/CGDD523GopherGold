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
    public CoinScoreUI coinScoreUI;

    //public List<TMP_Text> playerTexts = new List<TMP_Text>();

    void Start()
    {
        int i = 0;

        foreach (GameObject g in GGameManager.instance.playerList)
        {
            GameObject prefab = g.GetComponent<GopherSetup>().ourGopher.gopherGame;

            g.transform.position = startPos[i].position;

            GameObject spawnedGopher = Instantiate(prefab, g.transform);

            // Find the coin counter on the newly spawned gopher.
            PlayerCoinCounter counter = spawnedGopher.GetComponentInChildren<PlayerCoinCounter>();

            // Connect the spawned gopher's counter to the UI.
            if (counter != null)
            {
                coinScoreUI.AssignPlayerCounter(i, counter);
            }

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
