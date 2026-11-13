using System.Collections;
using TMPro;
using UnityEngine;

public class Countdown : MonoBehaviour
{
    public TMP_Text countdownText;

    //scripts that control the racers' movement
    public MinecartController[] racerMovementScripts;

    // how long each countdown number stays on screen
    public float countdownDuration = 1f;

    void Awake()
    {
        // prevent the racers from moving before the race begins
        foreach (Behaviour movementScript in racerMovementScripts)
        {
            if (movementScript != null)
            {
                movementScript.enabled = false;
            }
        }
    }

    void Start()
    {
        // start the countdown when the scene begins
        StartCoroutine(StartCountdown());
    }

    IEnumerator StartCountdown()
    {
        // Display 3, 2, then 1
        for (int i = 3; i > 0; i--)
        {
            countdownText.text = i.ToString();

            // wait one second before showing the next number
            yield return new WaitForSeconds(countdownDuration);
        }

        // tell the players the race is starting
        countdownText.text = "GO!";

        // keep GO on screen briefly
        yield return new WaitForSeconds(0.75f);

        // allow the racers to move
        foreach (MinecartController movementScript in racerMovementScripts)
        {
            if (movementScript != null)
            {
                movementScript.enabled = true;
            }
        }

        // hide the countdown after the race begins
        countdownText.gameObject.SetActive(false);
    }
}
