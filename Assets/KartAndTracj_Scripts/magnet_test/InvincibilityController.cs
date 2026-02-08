using UnityEngine;
using System.Collections;

public class InvincibilityController : MonoBehaviour
{
    
    public float InvDuration = 4f;
    public float InvCooldown = 20f;
    public float InvTimer = 20f;

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
        yield return new WaitForSecondsRealtime(InvDuration);
        InvOn = false;
    }
}
