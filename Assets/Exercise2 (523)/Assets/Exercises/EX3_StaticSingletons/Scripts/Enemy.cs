using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

namespace Ex3
{
    public class Enemy : MonoBehaviour
    {
        public static float moveSpeed = 1;
        public static bool canMove = true;
        private Rigidbody2D rgb2D;
        private int xDir,
            yDir;
        public GameObject player;

        // Start is called once before the first execution of Update after the MonoBehaviour is created

        void Start()
        {
            rgb2D = GetComponent<Rigidbody2D>();
            player = GameObject.Find("Player");
        }

        // Update is called once per frame
        void Update()
        {
            TrackPlayer();
            MoveObject();
        }

        private void TrackPlayer()
        {
            if (transform.position.x < player.transform.position.x)
            {
                xDir = 1;
            }
            else
            {
                xDir = -1;
            }

            if (transform.position.y < player.transform.position.y)
            {
                yDir = 1;
            }
            else
            {
                yDir = -1;
            }
        }

        private void MoveObject()
        {
            if (canMove)
            {
                rgb2D.linearVelocityX = xDir * moveSpeed;
                rgb2D.linearVelocityY = yDir * moveSpeed;
            }
            else
            {
                rgb2D.linearVelocity = Vector2.zero;
            }
        }
    }
}
