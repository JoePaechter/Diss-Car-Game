using System.Collections;
using UnityEngine;

public class MagnetController : MonoBehaviour
{

    public float magnetArea = 5f;
    public float magnetAttraction = 10f;
    public float magnetDuration = 5f;

    public bool MagnetOn { get; private set; }
    
    public void TurnOnMagnet()
    {
        StopAllCoroutines();
        StartCoroutine(MagnetRoutine());
    }

    IEnumerator MagnetRoutine()
    {
        MagnetOn = true;
        yield return new WaitForSeconds(magnetDuration);
        MagnetOn = false;
    }
}
