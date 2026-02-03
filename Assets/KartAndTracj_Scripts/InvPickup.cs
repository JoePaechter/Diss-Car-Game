using UnityEngine;

public class InvPickup : MonoBehaviour
{
    public CoinSpawner coinSpawner;
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<kart>())
        {
            InvincibilityController inv = other.GetComponent<InvincibilityController>();
            if (inv != null)
            {
                inv.TurnOnInv();
            }

            Destroy(gameObject);
            coinSpawner.RemovepowerUpFromList(gameObject);
        }
    }
}
