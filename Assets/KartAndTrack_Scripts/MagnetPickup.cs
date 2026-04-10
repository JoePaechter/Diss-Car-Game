using UnityEngine;

public class MagnetPickup : MonoBehaviour
{
    public CoinSpawner coinSpawner;
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<kart>())
        {
            MagnetController magnet = other.GetComponent<MagnetController>();
            if (magnet != null)
            {
                magnet.TurnOnMagnet();
            }

            Destroy(gameObject);
            coinSpawner.RemovepowerUpFromList(gameObject);
        }
    }


}
