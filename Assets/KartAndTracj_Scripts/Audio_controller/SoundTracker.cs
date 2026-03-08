using UnityEngine;

public class SoundTracker : MonoBehaviour
{
    public AudioSource soundEffects;
    public AudioSource carCrash;
    public AudioSource magnet;
    public AudioSource inv;
    public AudioSource speed;
    public AudioSource busDetected;

    public static SoundTracker Instance;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    public void PlayCoinPickup()
    {
        soundEffects.Play();
    }

    public void PlayCarCrash()
    {
        carCrash.Play();
    }
    public void PlayMagnet()
    {
        magnet.Play();
    }
    public void PlaySpeed()
    {
        speed.Play();
    }
    public void PlayInv()
    {
        inv.Play();
    }

    public void PlayBusDetected()
    {
        busDetected.Play();
    }
}
