using UnityEngine;

namespace YOLOTools.YOLO
{
    public interface CollisionResponderInterface
    {
        void OnCollisionDetected();
        void OnCollisionCleared();


        Rect  GetScreenRect(Camera cam);
    }
}
