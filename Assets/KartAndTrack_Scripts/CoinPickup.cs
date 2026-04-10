using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;


public class CoinPickup : MonoBehaviour
    
    
{
    public AudioSource coin_pickup_sound;
    public float haptic_amplitude;
    public float haptic_duration;
    public int scoreAmount = 1;
    public CoinSpawner coinSpawner;


    private void Awake()
    {
 
        coin_pickup_sound = GetComponent<AudioSource>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<kart>())
        {
            Debug.Log("coin picked up!");
            //coin_pickup_sound.Play();
            ScoreTracker.Instance.addScore(scoreAmount);
            SoundTracker.Instance.PlayCoinPickup();
            playLeftHaptic(haptic_amplitude, haptic_duration);
            Destroy(gameObject, 0.1f);
            coinSpawner.RemoveCoinFromList(gameObject);
            
            
            


            

        }
    }

    public void playLeftHaptic(float amplitude, float duration)
    {
        var devices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(
            InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller,devices);

        foreach (InputDevice device in devices)
        {
            if (device.TryGetHapticCapabilities(out HapticCapabilities controller) && controller.supportsImpulse) 
                {
                device.SendHapticImpulse(0, haptic_amplitude, haptic_duration);
                }
        }
    }
}
