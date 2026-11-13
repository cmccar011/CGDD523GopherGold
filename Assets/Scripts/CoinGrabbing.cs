using UnityEngine;

public class CoinGrabbing : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // find the coin counter on gopher or on its parent object
        PlayerCoinCounter player = other.GetComponentInParent<PlayerCoinCounter>();

        // only collect the coin if a Gopher touches it
        if (player != null)
        {
            player.AddCoin();
            Destroy(gameObject);
        }
    }
}
