using UnityEngine;

public class MagnetMovement : MonoBehaviour
{
    public Transform track;
    public float speed = 3f;


    // Update is called once per frame
    void Update()
    {
        transform.position -= track.forward * speed * Time.deltaTime;
    }
}
