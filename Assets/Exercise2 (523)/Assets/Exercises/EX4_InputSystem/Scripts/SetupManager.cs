using Ex3;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Ex4
{
    public class SetupManager : MonoBehaviour
    {
        public static SetupManager instance;
        public GameObject[] playerStartPositions;
        public int playerCount = 0;
        public int playersReady = 0;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            DontDestroyOnLoad(this.gameObject);
            //make it a singleton
            instance = this;
        }

        void OnPlayerJoined(PlayerInput pi)
        {
            //Add the player to do not destroy on load;
            DontDestroyOnLoad(pi.gameObject);

            //Put the player at a start position
            pi.gameObject.transform.position = playerStartPositions[playerCount].transform.position;

            //Save the player number
            pi.gameObject.GetComponent<PlayerSetup>().playerNum = playerCount;

            //Save the playe in the GameManager
            GameManager.instance.playerList.Add(pi.gameObject);
            playerCount++;
        }

        public void PlayerReady(int playerNum)
        {
            UIManager.instance.SetReadyText(playerNum);
            playersReady++;

            if (playersReady == playerCount)
            {
                //don't allow players to join after the game has started!
                GetComponent<PlayerInputManager>().DisableJoining();

                //switch the player's action maps so they can move
                GameManager.instance.SwitchToGameActions();
                //switch to the game scene
                SceneManager.LoadScene(1);
            }
        }
    }
}
