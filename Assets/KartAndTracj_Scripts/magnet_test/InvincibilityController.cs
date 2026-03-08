using UnityEngine;
using System.Collections;

public class InvincibilityController : MonoBehaviour
{
    
    public float InvDuration = 10f;
    public float InvCooldown = 10f;
    public float InvTimer = 10f;

    public bool InvOn { get; private set; }

    public void Update()
    {
        InvTimer += Time.deltaTime;

    }

    public void TurnOnInv()
    {
        if (InvTimer >= InvCooldown)
        {
            InvTimer = 0;
            StopAllCoroutines();
            StartCoroutine(InvRoutine());
            InvTimer = 0;
        }
    }

    IEnumerator InvRoutine()
    {
        InvOn = true;
        SoundTracker.Instance.PlayInv();
        yield return new WaitForSecondsRealtime(InvDuration);
        InvOn = false;
    }

    public void playBusSound()
    {
        SoundTracker.Instance.PlayBusDetected();
    }
}
