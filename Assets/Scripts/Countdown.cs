using System.Collections;
using TMPro;
using UnityEngine;

public class Countdown : MonoBehaviour
{
    [Header("COUNTDOWN AUDIO")]
    public AudioSource countdownAudioSource;
    public AudioClip countdownBeep;
    public AudioClip goSound;
    public TMP_Text countdownText;

    private MinecartController[] minecartControllers;
    private Rigidbody[] racerRigidbodies;

    public float countdownDuration = 1f;

    void Start()
    {
        StartCoroutine(StartCountdown());
    }

    IEnumerator StartCountdown()
    {
        // Wait one frame for racers to be instantiated
        yield return null;

        // Find the racers' movement scripts
        minecartControllers = FindObjectsByType<MinecartController>();

        // Disable racer movement
        foreach (MinecartController script in minecartControllers)
        {
            if (script != null)
                script.enabled = false;
        }

        // Find and freeze the racers' rigidbodies
        racerRigidbodies = new Rigidbody[minecartControllers.Length];

        for (int i = 0; i < minecartControllers.Length; i++)
        {
            racerRigidbodies[i] = minecartControllers[i].GetComponent<Rigidbody>();

            if (racerRigidbodies[i] != null)
            {
                racerRigidbodies[i].linearVelocity = Vector3.zero;
                racerRigidbodies[i].angularVelocity = Vector3.zero;
            }
        }

        // Display countdown
        for (int i = 3; i > 0; i--)
        {
            countdownText.text = i.ToString();
            countdownAudioSource.PlayOneShot(countdownBeep);
            yield return new WaitForSeconds(countdownDuration);
        }

        countdownText.text = "GO!";
        countdownAudioSource.PlayOneShot(goSound);
        yield return new WaitForSeconds(0.75f);

        // Re-enable movement scripts
        foreach (MinecartController script in minecartControllers)
        {
            if (script != null)
                script.enabled = true;
        }

        countdownText.gameObject.SetActive(false);
    }
}
