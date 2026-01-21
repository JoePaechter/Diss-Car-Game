using UnityEngine;

public class CarProxy : MonoBehaviour
{
    public BoxCollider box;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        box = GetComponent<BoxCollider>();
    }

    // Update is called once per frame
    public void UpdateProxy(
        Vector3 position,
        Vector3 size,
        Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;
        box.size = size;
    }

   
}
