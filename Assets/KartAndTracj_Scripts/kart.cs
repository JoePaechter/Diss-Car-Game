using UnityEngine;
using UnityEngine.InputSystem;
using YOLOTools.YOLO;

public class kart : MonoBehaviour, CollisionResponderInterface
{
    public float steer_speed = 2.0f;
    public float maxOffset_x = 2.2f;


 
    public Transform kartVisual;
    public float visual_tilt = 15f;
    public float laneWidth = 0.12f;

    public float currentX;

    InputAction steerActionX;
    private bool collidingWithCar;

    public Renderer kartRenderer;

    public Color crashColor = Color.red;
    private Color originalColor;
    private Material kartMaterial;

    public Vector3 startPosition;
    public Quaternion startRotation;

    public GameManager gameManager;







    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        steerActionX = new InputAction(
            type: InputActionType.Value,
            binding: "<XRController>{LeftHand}/thumbstick/x");

      
        currentX = transform.localPosition.x;

        if (kartRenderer != null)
        {
            kartMaterial = kartRenderer.material;
            originalColor = kartMaterial.color;
        }

        

    }
    void OnEnable()
    {
        steerActionX.Enable();
        
    }
    void OnDisable()
    {
        steerActionX.Disable();
        
    }


    // Update is called once per frame
    void Update()
    {
        transform.localRotation = Quaternion.identity;
        HandleMovement();
    }

    void HandleMovement()
    {
        float steer_input_x = steerActionX.ReadValue<float>();
       


        currentX += steer_input_x * steer_speed * Time.deltaTime;

        currentX = Mathf.Clamp(currentX, -maxOffset_x, maxOffset_x);

        Vector3 localPos = transform.localPosition;
        localPos.x = currentX;
   
        transform.localPosition = localPos; 

        UpdateVisualTilt(steer_input_x);
    }

    void UpdateVisualTilt(float steer)
    {
        if (!kartVisual) return;

        float target_tilt = -steer * visual_tilt;
        Quaternion targetRot = Quaternion.Euler(0, 0, target_tilt);

        kartVisual.localRotation = Quaternion.Slerp(
            kartVisual.localRotation,
            targetRot,
            Time.deltaTime * 8f );
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<CarProxy>())
        {
            if (!collidingWithCar)
            {
                collidingWithCar = true;
                OnCarHit();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<CarProxy>())
        {
            collidingWithCar = false;
            OnCarExit();
        }
    }

    public void OnCarHit()
    {
  
        //steer_speed *= 0.1f;
        

    }

    public void OnCarExit()
    {
        //steer_speed *= 10f;
    }

    public float GetLaneWidth()
    {
        return laneWidth;
    }

    public Rect GetScreenRect(Camera cam)
    {
        Bounds b = kartRenderer.bounds;

        Vector3 center = b.center;
        Vector3 ext = b.extents;

        Vector3[] corners =
        {
        center + new Vector3(-ext.x, -ext.y, -ext.z),
        center + new Vector3(-ext.x, -ext.y,  ext.z),
        center + new Vector3(-ext.x,  ext.y, -ext.z),
        center + new Vector3(-ext.x,  ext.y,  ext.z),
        center + new Vector3( ext.x, -ext.y, -ext.z),
        center + new Vector3( ext.x, -ext.y,  ext.z),
        center + new Vector3( ext.x,  ext.y, -ext.z),
        center + new Vector3( ext.x,  ext.y,  ext.z),
    };

        float minX = 1f, minY = 1f;
        float maxX = 0f, maxY = 0f;
        bool anyVisible = false;

        foreach (var c in corners)
        {
            Vector3 v = cam.WorldToViewportPoint(c);
            if (v.z <= 0) continue;

            anyVisible = true;
            minX = Mathf.Min(minX, v.x);
            minY = Mathf.Min(minY, v.y);
            maxX = Mathf.Max(maxX, v.x);
            maxY = Mathf.Max(maxY, v.y);
        }

        if (!anyVisible)
            return new Rect(0, 0, 0, 0);

        // Clamp viewport
        minX = Mathf.Clamp01(minX);
        minY = Mathf.Clamp01(minY);
        maxX = Mathf.Clamp01(maxX);
        maxY = Mathf.Clamp01(maxY);

        // Convert to screen pixels
        return new Rect(
            minX * cam.pixelWidth,
            minY * cam.pixelHeight,
            (maxX - minX) * cam.pixelWidth,
            (maxY - minY) * cam.pixelHeight
        );
    }


    public float GetScreenX(Camera cam)
    {
        Vector3 sp = cam.WorldToScreenPoint(transform.position);
        float x = sp.x / cam.pixelWidth;

        Debug.Log($"KartX (normalized): {x:F2}");
        return x;
    }



    public void OnCollisionDetected()
    {
        OnCarHit();
        if (kartMaterial == null) return;
        kartMaterial.color = crashColor;
        gameManager.endGame();

    }

    public void OnCollisionCleared()
    {
        OnCarExit();
        if (kartMaterial == null) return;
        kartMaterial.color = originalColor;
    }

    public void ResetKart()
    {
        transform.SetPositionAndRotation(startPosition, startRotation);

    }
}
