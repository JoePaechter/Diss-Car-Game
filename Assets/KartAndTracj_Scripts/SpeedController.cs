using UnityEngine;
using System.Collections;

public class SpeedController : MonoBehaviour
{
    public float SpeedDuration = 10f;

    public bool SpeedOn { get; private set; }

    public void TurnOnSpeed()
    {
        StopAllCoroutines();
        StartCoroutine(SpeedRoutine());
    }

    IEnumerator SpeedRoutine()
    {
        SpeedOn = true;
        yield return new WaitForSecondsRealtime(SpeedDuration);
        SpeedOn = false;
    }
}
