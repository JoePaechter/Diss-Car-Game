using UnityEngine;

public class SoundTracker : MonoBehaviour
{
    public AudioSource soundEffects;

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
}
