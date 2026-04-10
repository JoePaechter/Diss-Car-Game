using UnityEngine;

public class SpeedPickup : MonoBehaviour
{
    public CoinSpawner coinSpawner;
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<kart>())
        {
            SpeedController speed = other.GetComponent<SpeedController>();
            if (speed != null)
            {
                //speed.TurnOnSpeed();
            }

            Destroy(gameObject);
            coinSpawner.RemovepowerUpFromList(gameObject);
        }
    }
}
