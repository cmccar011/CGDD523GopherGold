using System.Collections;
using Ex3;
using UnityEngine;

namespace Ex3
{
    public class PowerUp_Pause : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created

        void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.tag == "Player")
            {
                print("here");
                // StartCoroutine(PauseEnemies());
            }
        }

        IEnumerator PauseEnemies()
        {
            Enemy.canMove = false;
            yield return new WaitForSeconds(4);
            Enemy.canMove = true;
            Enemy.moveSpeed++;
        }
    }
}
