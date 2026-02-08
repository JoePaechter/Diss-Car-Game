using UnityEngine;

public class CoinMovement : MonoBehaviour
{

    public float startSpeed = 3f;
    public float newSpeed;
    public float magnet_speed = 5f;
    public Transform kart;
    public Transform track;
    private MagnetController magnet;
    private ScoreTracker scoreTracker;
    

    private void Start()
    {
        magnet = kart.GetComponent<MagnetController>();
        scoreTracker = FindObjectOfType<ScoreTracker>();
    }
    // Update is called once per frame
    void Update()
    {
        if (magnet == null && kart != null)
        {
            magnet = kart.GetComponent<MagnetController>();
        }
        if (magnet != null && magnet.MagnetOn)
        {
            float distance = Vector3.Distance(transform.position, kart.position);

            if (distance < magnet.magnetArea)
            {
                GoToKart();
                return;
            }
        }
        newSpeed = startSpeed + (0.1f * scoreTracker.score);
        transform.position -= track.forward * newSpeed * Time.deltaTime;
        
    }

    void GoToKart()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            kart.position
            , magnet_speed * Time.deltaTime );
    }

}
