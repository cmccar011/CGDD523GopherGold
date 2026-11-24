using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class CoinScoreUI : MonoBehaviour
{
    public int playerTotal = 0;

    [Header("Player Coin Counters")]
    public PlayerCoinCounter TobyCC;
    public PlayerCoinCounter player2;
    public PlayerCoinCounter player3;
    public PlayerCoinCounter player4;

    [Header("Score Text")]
    public TMP_Text player1Text;
    public TMP_Text player2Text;
    public TMP_Text player3Text;
    public TMP_Text player4Text;

    public void StartGame()
    {
        playerTotal = PlayerPrefs.GetInt("TotalPlayers");
        for (int i = 0; i <= playerTotal; i++) { }
    }

    void Update()
    {
        player1Text.text = TobyCC.coinCount.ToString();
        player2Text.text = player2.coinCount.ToString();
        player3Text.text = player3.coinCount.ToString();
        player4Text.text = player4.coinCount.ToString();
    }

    public void AssignPlayerCounter(int index, PlayerCoinCounter counter)
    {
        switch (index)
        {
            case 0:
                TobyCC = counter;
                break;

            case 1:
                player2 = counter;
                break;

            case 2:
                player3 = counter;
                break;

            case 3:
                player4 = counter;
                break;
        }
    }
}
