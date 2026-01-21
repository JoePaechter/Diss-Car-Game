using UnityEngine;

public class CoinMovement : MonoBehaviour
{

    public float speed = 3f;
    public float magnet_speed = 5f;
    public Transform kart;
    public Transform track;
    private MagnetController magnet;
    

    private void Start()
    {
        magnet = kart.GetComponent<MagnetController>();
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
        transform.position -= track.forward * speed * Time.deltaTime;
        
    }

    void GoToKart()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            kart.position
            , magnet_speed * Time.deltaTime );
    }

}
