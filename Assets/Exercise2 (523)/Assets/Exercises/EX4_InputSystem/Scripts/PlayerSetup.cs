using UnityEngine;

namespace Ex4
{
    public class PlayerSetup : MonoBehaviour
    {
        public Sprite[] playerCharacters;
        public int playerNum;
        private int currentChar = 0;
        public SpriteRenderer spriteRenderer;
        private bool playerReady = false;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start() { }

        // Update is called once per frame
        void Update() { }

        void OnSelectLeft()
        {
            UpdateChar(-1);
        }

        void OnSelectRight()
        {
            UpdateChar(1);
        }

        void UpdateChar(int dir)
        {
            if (!playerReady)
            {
                currentChar += dir;
                //bound the colors to the array length
                if (currentChar > playerCharacters.Length - 1)
                {
                    currentChar = 0;
                }
                else if (currentChar < 0)
                {
                    currentChar = playerCharacters.Length - 1;
                }
                //change the sprites color
                spriteRenderer.sprite = playerCharacters[currentChar];
            }
        }

        void OnStart()
        {
            if (!playerReady)
            {
                playerReady = true;
                SetupManager.instance.PlayerReady(playerNum);
            }
        }
    }
}
