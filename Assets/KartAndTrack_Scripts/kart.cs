using Oculus.Interaction;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using YOLOTools.YOLO;

public class kart : MonoBehaviour, CollisionResponderInterface
{
    public float steer_speed = 1.5f;
    public float maxOffset_x = 2.2f;
    public float deadZone = 0.08f;


 
    public Transform kartVisual;
    public float visual_tilt = 15f;
    public float laneWidth = 0.12f;

    public float currentX;

    InputAction steerActionX;
    private bool collidingWithCar;

    public Renderer kartRenderer;

    public Color crashColor = Color.red;
    public Color magnetColor = Color.green;
    public Color invColor = Color.gold;
    public Color speedColor = Color.purple;
    private Color originalColor;
    private Material kartMaterial;

    public Vector3 startPosition;
    public Quaternion startRotation;

    public GameManager gameManager;

    private InvincibilityController inv;

    private SpeedController speed;

    private MagnetController magnet;

    //crash haptics

    public float haptic_amplitude;
    public float haptic_duration;

    public AudioSource carCrashSound;

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

        inv = GetComponent<InvincibilityController>();

        speed = GetComponent<SpeedController>();

        magnet = GetComponent<MagnetController>();



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
        if (inv == null )
        {
            inv = GetComponent<InvincibilityController>();
        }
        if (speed == null) 
        { 
            speed= GetComponent<SpeedController>();
        }

        transform.localRotation = Quaternion.identity;
        IsSpeedpowerUp();
        IsMagnet();
        IsInv();
        IsNothing();
        HandleMovement();
    }

    void HandleMovement()
    {
        float steer_input_x = steerActionX.ReadValue<float>();
       

        if (Mathf.Abs(steer_input_x) < deadZone)
        {
            steer_input_x = 0f;
        }

        //raise to power 2 for dynamic steering
        steer_input_x = Mathf.Sign(steer_input_x) * Mathf.Pow(Mathf.Abs(steer_input_x), 2f);

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
        Bounds bounds = kartRenderer.bounds;

        Vector3 center = bounds.center;
        Vector3 extents = bounds.extents;

        Vector3[] corners =
        {
        center + new Vector3(-extents.x, -extents.y, -extents.z),
        center + new Vector3(-extents.x, -extents.y,  extents.z),
        center + new Vector3(-extents.x,  extents.y, -extents.z),
        center + new Vector3(-extents.x,  extents.y,  extents.z),
        center + new Vector3( extents.x, -extents.y, -extents.z),
        center + new Vector3( extents.x, -extents.y,  extents.z),
        center + new Vector3( extents.x,  extents.y, -extents.z),
        center + new Vector3( extents.x,  extents.y,  extents.z),
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
        if (!inv.InvOn)
        {
            OnCarHit();
            if (kartMaterial == null) return;
            kartMaterial.color = crashColor;
            SoundTracker.Instance.PlayCarCrash();
            playLeftHaptic(haptic_amplitude, haptic_duration);
            gameManager.endGame();
        }

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

    public void IsSpeedpowerUp()
    {
        if (speed.SpeedOn)
        {
            steer_speed = 3f;
            kartMaterial.color = speedColor;
        }
        else
        {
            steer_speed = 2f;
            
        }
    }

    public void IsInv()
    {
        if (inv.InvOn)
        {
            kartMaterial.color = invColor;
        }
        
    }

    public void IsMagnet()
    {
        if (magnet.MagnetOn)
        {
            kartMaterial.color = magnetColor;
        }
        
    }

    public void IsNothing()
    {
        if (!magnet.MagnetOn && !inv.InvOn && !speed.SpeedOn)
        {
            kartMaterial.color = originalColor;
        }

    }

    public void playLeftHaptic(float amplitude, float duration)
    {
        var devices = new List<UnityEngine.XR.InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(
            InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller, devices);

        foreach (UnityEngine.XR.InputDevice device in devices)
        {
            if (device.TryGetHapticCapabilities(out HapticCapabilities controller) && controller.supportsImpulse)
            {
                device.SendHapticImpulse(0, haptic_amplitude, haptic_duration);
            }
        }
    }
}
