using UnityEngine;
using YOLOTools.YOLO.ObjectDetection;
using System.Collections.Generic;


namespace YOLOTools.YOLO
{

    public class CollisionManager : MonoBehaviour
    {
        public Camera referenceCamera;
        [SerializeField] private MonoBehaviour responderBehaviour;
        private CollisionResponderInterface responder;
        [SerializeField] private BoundingBoxDisplayManager boxDisplay;



        public float closeYThreshold = 0.45f;
        private bool hasCrashed;

        Rect debugKartRect;
        Rect debugCarRect;
        bool hasDebugCar;

        public void Awake()
        {
            responder = responderBehaviour as CollisionResponderInterface;
        }

        public void CheckCollisions(IReadOnlyList<DetectedObject> detections)
        {
            if (responder == null)
            {
                Debug.LogWarning("CollisionManager: responder is NULL");
                return;
            }

            if (referenceCamera == null)
            {
                Debug.LogWarning("CollisionManager: referenceCamera is NULL");
                return;
            }

            Debug.Log("CollisionManager running");

            Rect kartRect = responder.GetScreenRect(referenceCamera);

             

            foreach (var detection in detections)
            {
                if (!IsCar(detection))
                    continue;

                if (isLaneHit(detection, kartRect))
                {
                    TriggerCrash();
                    Debug.Log("CRASH!");
                    return;
                }
            }

            if (hasCrashed)
            {
                hasCrashed = false;
                responder.OnCollisionCleared();
            }


        }

        bool isLaneHit(DetectedObject detection, Rect kartRect)
        {
            Rect carRect = boxDisplay.GetCarRect(detection);

            float x_overlap = Mathf.Max(0f, Mathf.Min(kartRect.xMax, carRect.xMax) -
                Mathf.Max(kartRect.xMin, carRect.xMin));

            float y_overlap = Mathf.Max(0f, Mathf.Min(kartRect.yMax, carRect.yMax) -
                Mathf.Max(kartRect.yMin, carRect.yMin));

            float overlap_area = x_overlap * y_overlap;
            float kartArea = kartRect.width * kartRect.height;


            bool overlaps = overlap_area > kartArea * 0.25f;
            //float kartArea = kartRect.width * kartRect.height;

            Debug.Log($"Kart:{kartRect} | car:{carRect}, overlaps:{overlaps}");

            
            if (overlaps)
            {
                return true;
            }
            else
            {
                return false;
            }
            //return overlaps;
        }

        bool IsCar(DetectedObject obj)
        {
            int id = obj.CocoClass;
            return id == 2 || id == 3 || id == 5 || id == 7 || id == 63; //car, motorcylce, bus, truck, laptop for testing
        }

        void TriggerCrash()
        {
            if (hasCrashed) return;

            hasCrashed = true;
            Debug.Log($"CRASH (lane-based");

            responder.OnCollisionDetected();
        }
        void DebugPositions(
            float kartX,
            float carX,
            float carY,
            bool xMatch,
            bool isClose,
            DetectedObject det
        )
        {
            Debug.Log(
                $"KartX: {kartX:F2} | " +
                $"CarX: {carX:F2} | " +
                $"CarY: {carY:F2} | " +
                $"XMatch: {xMatch} | " +
                $"Close: {isClose} | " +
                $"Class: {det.CocoName}"
            );
        }
        Rect ConvertYOLOToUnityRect(Rect r)
        {

            float xMin = r.xMin / referenceCamera.pixelWidth;
            float xMax = r.xMax / referenceCamera.pixelWidth;
            float yMin = 1f - (r.yMax / referenceCamera.pixelHeight);
            float yMax = 1f - (r.yMin / referenceCamera.pixelHeight);

            return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
        }

        float GetOverlapArea(Rect a, Rect b)
        {
            float xOverlap = Mathf.Max(0, Mathf.Min(a.xMax, b.xMax) - Mathf.Max(a.xMin, b.xMin));
            float yOverlap = Mathf.Max(0, Mathf.Min(a.yMax, b.yMax) - Mathf.Max(a.yMin, b.yMin));
            return xOverlap * yOverlap;
        }

        void OnGUI()
        {
            if (responder == null || referenceCamera == null) return;

            Rect kartRect = responder.GetScreenRect(referenceCamera);
            
        }

        
        void DebugOverlap(Rect kartRect, Rect carRect, DetectedObject det)
        {
            Debug.Log(
                $" Kart:{kartRect} | Car:{carRect} | Class:{det.CocoName}"
            );
        }

        

    }








}