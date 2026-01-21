
using UnityEngine;

public class TrackRoot : MonoBehaviour
{
    public Transform xrCamera;
    public float forwardOffSet = 2.5f;
    public float heightOffSet = 1f;
    public Transform trackScroller;



    private Vector3 lockedForward;
  
    private float fixedY;
    private bool isInitialized;
    private bool isTurning;
    private Vector3 lockedPosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lockedForward = FlattenForward(xrCamera.forward);

        fixedY = xrCamera.position.y - heightOffSet;
        lockedPosition = xrCamera.position + lockedForward * forwardOffSet;
        lockedPosition.y = fixedY;

        isInitialized = true;


    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (!isInitialized || trackScroller == null) return;
       
        
        transform.position = lockedPosition;

    }
    

    public void SetTurning(bool turning) => isTurning = turning;

    public void CommitTurn()
    {
        lockedForward = FlattenForward(trackScroller.forward);

        lockedPosition = lockedPosition + lockedForward * forwardOffSet;
        lockedPosition.y = fixedY;
        transform.position = lockedPosition;
    }

    public void ReLockForwardFromScroller()
    {
        lockedForward = FlattenForward(trackScroller.forward);
       
    }

    private Vector3 FlattenForward(Vector3 f)
    {
        f.y = 0f;
        if (f.sqrMagnitude < 1e-6f) return Vector3.forward;
        return f.normalized;
    }
   
}
