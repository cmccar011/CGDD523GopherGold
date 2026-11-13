using UnityEngine;

public class PlayerCoinCounter : MonoBehaviour
{
    public int coinCount = 0;

    [Header("Coin Audio")]
    [SerializeField]
    private AudioSource coinAudioSource;

    [SerializeField]
    private AudioClip coinCollectClip;

    public void AddCoin()
    {
        // Increase this player's coin count.
        coinCount++;

        Debug.Log(gameObject.name + " Coins: " + coinCount);

        // Play the coin sound without interrupting other audio.
        if (coinAudioSource != null && coinCollectClip != null)
        {
            coinAudioSource.PlayOneShot(coinCollectClip);
        }
    }
}
