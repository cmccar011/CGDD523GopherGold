using TMPro;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.InputSystem;
using System.Collections.Generic;

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
        for(int i =0; i <= playerTotal; i++)
        {

        }
    }
    void Update()
    {
       // player1Text.text = TobyCC.coinCount.ToString();
       // player2Text.text = player2.coinCount.ToString();
       // player3Text.text = player3.coinCount.ToString();
       // player4Text.text = player4.coinCount.ToString();
    }
}
