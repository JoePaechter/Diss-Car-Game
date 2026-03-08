using UnityEngine;
using System.Collections;

public class SpeedController : MonoBehaviour
{
    public float SpeedDuration = 10f;
    public float SpeedCooldown = 20f;
    public float SpeedTimer = 20f;
    

    public bool SpeedOn { get; private set; }

    public void Update()
    {
 
        SpeedTimer += Time.deltaTime;

    }

    public void TurnOnSpeed()
    {
        if (SpeedTimer >= SpeedDuration)
        {
            SpeedTimer = 0;
            StopAllCoroutines();
            StartCoroutine(SpeedRoutine());
            SpeedTimer = 0;

        }

    }

    IEnumerator SpeedRoutine()
    {
        SpeedOn = true;
        SoundTracker.Instance.PlaySpeed();
        yield return new WaitForSecondsRealtime(SpeedDuration);
        SpeedOn = false;
        
    }
}
