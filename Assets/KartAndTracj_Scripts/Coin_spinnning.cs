using UnityEngine;

public class Coin_spinnning : MonoBehaviour
{
    
    
    float rotate_speed = 180f;
    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.up, rotate_speed * Time.deltaTime, Space.Self);
    }
}
