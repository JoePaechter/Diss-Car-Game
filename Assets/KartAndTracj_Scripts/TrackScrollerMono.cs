using UnityEngine;

public class TrackScrollerMono : MonoBehaviour
{
  

 

    //head rotation

    public Transform xrCamera;

    public float rotationSmooth = 6f;

    public float forwardOffset = 10f;
    public float heightBelowHead = 3.0f;

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

        
       


    }

    private void LateUpdate()
    {
        ApplyHeadTracking();
        Vector3 forward = transform.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 pos = xrCamera.position + forward * forwardOffset;
        pos.y = xrCamera.position.y - heightBelowHead;

     

        transform.position = pos;


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

