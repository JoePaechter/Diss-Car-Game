
using UnityEngine;


public class TrackScoller : MonoBehaviour
{

    public Transform[] track_segments;
    public float segment_length = 5f;

    private float current_speed = 6f;

    //head rotation

    public Transform xrCamera;
 
    public float rotationSmooth = 6f;

    public float forwardOffset = 2.5f;
    public float heightBelowHead = 1.0f;

    public float turnThreshold = 45f;
  
    public float turnSpeed = 360f;
      

    private Vector3 smoothedHeadDir;
    public float maxTurnAngle = 20f;

    private bool isTurning = false;
    Quaternion targetRotation;

 
    



    private void Start()
    {
        Vector3 dir = xrCamera.forward;
        dir.y = 0f;
        smoothedHeadDir = dir.normalized;

        for (int i = 0; i < track_segments.Length; i++)
        {
            track_segments[i].localPosition =
                new Vector3(0f, 0f, (i * segment_length));
        }


    }

    private void LateUpdate()
    {
        Vector3 forward = transform.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 newPos = xrCamera.position + forward * forwardOffset;
        newPos.y = xrCamera.position.y - heightBelowHead;

        Vector3 parentDelta = newPos - transform.position;

        transform.position = newPos;

        foreach(Transform segment in track_segments)
        {
            segment.position -= parentDelta;
        }

    }


    private void Update()
    {
        
        ApplyHeadTracking();
        ScrollSegments();

        
    }

    

    void ScrollSegments()

    {
        float wrap = -segment_length;
       
        float moveAmount = current_speed * Time.deltaTime;
        foreach (Transform segment in track_segments)
        {
            segment.localPosition -= Vector3.forward  * moveAmount;

            if (segment.localPosition.z <= wrap)
            {
                segment.localPosition += Vector3.forward * segment_length * track_segments.Length;
            }
        
         }

        
    }

    void ApplyHeadTracking()
    {
        
        Vector3 headDir = xrCamera.forward;
        headDir.y = 0f;
        headDir.Normalize();

        if (!isTurning)
        {

            smoothedHeadDir = Vector3.Slerp(
                smoothedHeadDir,
                headDir,
                Time.deltaTime * rotationSmooth
            );

        }
        else
        {
            smoothedHeadDir = headDir;
        }

        Vector3 trackForward = transform.forward;
        trackForward.y = 0f;
        trackForward.Normalize();



            float angle = Vector3.SignedAngle(trackForward, smoothedHeadDir, Vector3.up);

       



        if (Mathf.Abs(angle) > turnThreshold && !isTurning)
        {
            isTurning = true;
            targetRotation =
                Quaternion.LookRotation(smoothedHeadDir, Vector3.up);
        }
        
        if (isTurning || Mathf.Abs(angle) > turnThreshold)
        {
            targetRotation = Quaternion.LookRotation(smoothedHeadDir, Vector3.up);

            transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            turnSpeed * Time.deltaTime);
        }

        
        if (Mathf.Abs(angle) < 5f)
        {
            isTurning = false;
        }
    }


}
