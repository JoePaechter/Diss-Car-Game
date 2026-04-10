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


            bool overlaps = overlap_area > kartArea * 0.4f;
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
        

        

    }








}