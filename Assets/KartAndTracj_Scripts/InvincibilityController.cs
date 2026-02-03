using UnityEngine;
using System.Collections;

public class InvincibilityController : MonoBehaviour
{
    
    public float InvDuration = 4f;

    public bool InvOn { get; private set; }

    public void TurnOnInv()
    {
        StopAllCoroutines();
        StartCoroutine(InvRoutine());
    }

    IEnumerator InvRoutine()
    {
        InvOn = true;
        yield return new WaitForSecondsRealtime(InvDuration);
        InvOn = false;
    }
}
