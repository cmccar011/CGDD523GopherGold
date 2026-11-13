using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ex4
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager instance;
        public List<GameObject> playerList;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Destroy(this.gameObject);
            }
        }

        // Update is called once per frame
        void Update() { }

        public void SwitchToGameActions()
        {
            foreach (GameObject player in playerList)
            {
                player.GetComponent<PlayerInput>().SwitchCurrentActionMap("Game");
            }
        }
    }
}
