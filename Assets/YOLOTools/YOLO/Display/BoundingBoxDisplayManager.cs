
using AYellowpaper.SerializedCollections;
using Meta.XR;
using Meta.XR.MRUtilityKit;
using MyBox;
using NUnit.Framework.Constraints;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;
using YOLOTools.ObjectDetection;
using YOLOTools.Utilities;
using YOLOTools.YOLO.ObjectDetection;


public class BoundingBoxDisplayManager : MonoBehaviour
{
    [Header("interpolation variables")]
    [SerializeField] private float interpolationRate = 20f;
    [SerializeField] private float predictionStrength = 0.25f;
    [SerializeField] private float newObjectThreshold = 300f;
    [SerializeField] private float ObjectCoolDown = 0.5f;
    [SerializeField] private float velocirty_smoothing = 0.4f;
    [SerializeField] private int minFrames = 1;
    [SerializeField] private float screenPadding = 50f;
    

    


    private float _depthFromCamera = 2.0f;

    #region External Data Management

    [Tooltip("The VideoFeedManager used to capture input frames.")]
    [MustBeAssigned][SerializeField] private VideoFeedManager _videoFeedManager;

    private Camera _camera;

    #endregion

    private class BoundingBoxVisual
    {
        public GameObject root;
        public LineRenderer line;
        public TextMesh label;
    }

    private class PredictedVisual
    {

        public BoundingBoxVisual Visual;
        public Vector2 CurrentMin; //predicted min and max
        public Vector2 CurrentMax;

        public Vector2 RawMin; //raw yolo min and max
        public Vector2 RawMax;
        public Vector2 VelocityMin; // how fast the box is moving (lag)
        public Vector2 VelocityMax;

        public float LastSeenCaptureTime;
        public float LastSeenRenderTime;

        public DetectedObject lastObject;
        public int FrameCount;
    }

    //list of bounding boxes created, dont wanna throw them away, saves memory
    //private List<BoundingBoxVisual> box_list;
    private List<PredictedVisual> predicted = new List<PredictedVisual>();

    private void Awake()
    {
        //box_list = new List<BoundingBoxVisual>();
    }

    public void DisplayModels(List<DetectedObject> objects, Camera referenceCamera, float captureTime)
    {
        Profiler.BeginSample("ObjectDisplayManager.DisplayModels");

        _camera = referenceCamera;
        float now = Time.time;

        List<DetectedObject> unmatchedDetections = new List<DetectedObject>(objects);
        HashSet<PredictedVisual> matchedObjects = new HashSet<PredictedVisual>();

        //match new detections
        foreach (var predict in predicted)
        {
            float closest = float.MaxValue;
            DetectedObject bestMatch = null;

            foreach (DetectedObject obj in unmatchedDetections)
            {
                float distance = Vector2.Distance(ImageToScreenCoordinates(obj.BoundingBox.center),
                    (predict.CurrentMin + predict.CurrentMax) / 2f);

                float sizeRatio = (obj.BoundingBox.width * obj.BoundingBox.height) /
                    (Mathf.Abs(predict.RawMax.x - predict.RawMin.x) * Mathf.Abs(predict.RawMax.y - predict.RawMin.y));

                if (distance < closest && distance < newObjectThreshold && sizeRatio > 0.5f && sizeRatio < 2.0f)
                {
                    closest = distance;
                    bestMatch = obj;
                }
            }

            if (bestMatch != null)
            {
                UpdatePrediction(predict, bestMatch, captureTime);
                matchedObjects.Add(predict);
                unmatchedDetections.Remove(bestMatch);
            }
        }

            //spawn boxes
            foreach (var newDetection in unmatchedDetections)
            {
                predicted.Add(CreateNewPrediction(newDetection, captureTime));

            }

            //destroy boxes
            for (int i = predicted.Count - 1; i >= 0; i--)
            {
                if (!matchedObjects.Contains(predicted[i]) && (now - predicted[i].LastSeenRenderTime > ObjectCoolDown))
                {
                    Destroy(predicted[i].Visual.root);
                    predicted.RemoveAt(i);
                }
            }
            Profiler.EndSample();
    }


    /*EnsureListSize(objects.Count);

    //draw all boxes in list
    for (int i = 0; i < box_list.Count; i++)
    {
        if (i < objects.Count)
        {
            DrawBox(objects[i],  box_list[i]);
            box_list[i].line.gameObject.SetActive(true);
            box_list[i].label.gameObject.SetActive(true);
        }
        else
        {
            box_list[i].line.gameObject.SetActive(false);
            box_list[i].label.gameObject.SetActive(false);

        }


    }
    Profiler.EndSample();
}
*/


    private void Update()
    {
        if (_camera == null)
        {
            return;
        }
        float now = Time.time;

        foreach (var predict in predicted) 
        {
            if (IsOffScreen(predict.CurrentMin, predict.CurrentMax))
            {
                predict.Visual.root.SetActive(false);
            }
            float latency = now - predict.LastSeenCaptureTime;

            Vector2 predMin = predict.RawMin + (predict.VelocityMin * latency * predictionStrength);
            Vector2 predMax = predict.RawMax + (predict.VelocityMax *latency * predictionStrength);

            // interpolate

            predict.CurrentMin = Vector2.Lerp(predict.CurrentMin, predMin, Time.deltaTime * interpolationRate);
            predict.CurrentMax = Vector2.Lerp(predict.CurrentMax, predMax, Time.deltaTime * interpolationRate);

            if (predict.FrameCount >= minFrames)
            {
               //DrawPredictedBox(predict);
            }
        }
    }

    private bool IsOffScreen(Vector2 min, Vector2 max)
    {
        return max.x < -screenPadding || min.x > _camera.scaledPixelWidth + screenPadding || max.y < -screenPadding || min.y > _camera.scaledPixelHeight + screenPadding;

    }

    private void UpdatePrediction(PredictedVisual predict, DetectedObject newObj, float captureTime)
    {
        Vector2 newMin = ImageToScreenCoordinates(newObj.BoundingBox.min);
        Vector2 newMax = ImageToScreenCoordinates(newObj.BoundingBox.max);
        float dt = captureTime - predict.LastSeenCaptureTime;

        if (dt > 0.001f)
        {
            Vector2 rawVelMin = (newMin - predict.RawMin) / dt;
            Vector2 rawVelMax = (newMax - predict.RawMax) / dt;

            predict.VelocityMin = Vector2.Lerp(predict.VelocityMin, rawVelMin, velocirty_smoothing);
            predict.VelocityMax = Vector2.Lerp(predict.VelocityMax, rawVelMax, velocirty_smoothing);
        }

        predict.RawMin = newMin;
        predict.RawMax = newMax;
        predict.lastObject = newObj;
        predict.LastSeenCaptureTime = captureTime;
        predict.LastSeenRenderTime = Time.time;

        predict.FrameCount++;
        if (predict.FrameCount >= minFrames)
        {
            predict.Visual.root.SetActive(true);
        }
    }

    private PredictedVisual CreateNewPrediction(DetectedObject obj, float captureTime)
    {
        var visual = CreateVisual();
        

        Vector2 screenMin = ImageToScreenCoordinates(obj.BoundingBox.min);
        Vector2 screenMax = ImageToScreenCoordinates(obj.BoundingBox.max);

        int initialFrames = 1;

        if (initialFrames >= minFrames)
        {
            visual.root.SetActive(true);
        }
        else
        {
            visual.root.SetActive(false);
        }

        return new PredictedVisual
        {
            Visual = visual,
            CurrentMin = screenMin,
            CurrentMax = screenMax,
            RawMin = screenMin,
            RawMax = screenMax,
            LastSeenCaptureTime = captureTime,
            LastSeenRenderTime = Time.time,
            lastObject = obj,
            FrameCount = initialFrames
        };
    }

    private void DrawPredictedBox(PredictedVisual predict)
    {
        Vector3[] corners = new Vector3[5];

        corners[0] = ScreenToWorld(predict.CurrentMin.x, predict.CurrentMin.y);
        corners[1] = ScreenToWorld(predict.CurrentMax.x, predict.CurrentMin.y);
        corners[2] = ScreenToWorld(predict.CurrentMax.x, predict.CurrentMax.y);
        corners[3] = ScreenToWorld(predict.CurrentMin.x, predict.CurrentMax.y);
        corners[4] = corners[0];

        predict.Visual.line.positionCount = corners.Length;
        predict.Visual.line.SetPositions(corners);

        predict.Visual.label.text = $"{predict.lastObject.CocoName} {(predict.lastObject.Confidence * 100f): 0}%";
        predict.Visual.label.transform.position = ScreenToWorld(predict.CurrentMin.x, predict.CurrentMax.y + 10f);
        
        predict.Visual.label.transform.LookAt(predict.Visual.label.transform.position + _camera.transform.rotation * Vector3.forward,
                                _camera.transform.rotation * Vector3.up);
    }

    #region Model Methods
    /*
    private void  EnsureListSize(int size)
    {
        if (box_list == null)
            box_list= new List<BoundingBoxVisual>();

        while (box_list.Count < size)
        {
            box_list.Add(CreateVisual());
        }
    }*/

    private BoundingBoxVisual CreateVisual()
    {
        GameObject root = new GameObject("BoundingBoxVisual");
        root.transform.parent = transform;

        //create line
        LineRenderer line = root.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.loop = false;
        line.material = new Material(Shader.Find("Unlit/Color"));
        line.material.color = Color.green;
        line.startWidth = 0.002f;
        line.endWidth = 0.002f;
        line.positionCount = 5;
        line.startColor = Color.green;
        line.endColor = Color.green;

        //create label
        GameObject labelGO = new GameObject("Label");
        labelGO.transform.SetParent(root.transform);

        TextMesh label = labelGO.AddComponent<TextMesh>();
        label.fontSize = 64;
        label.characterSize = 0.01f;
        label.anchor = TextAnchor.LowerLeft;
        label.color = Color.white;

        //hide lines remove to see bounding boxes
        line.enabled = false;              
        label.gameObject.SetActive(false);


        root.SetActive(false);

        return new BoundingBoxVisual
        {
            root = root,
            line = line,
            label = label
        };
    }

    //draw bounding box from struct

    private void DrawBox(DetectedObject obj, BoundingBoxVisual visual)
    {
        Debug.Log(
        $"BBox raw: min={obj.BoundingBox.min}, max={obj.BoundingBox.max}"
        );

        Vector2 min = ImageToScreenCoordinates(obj.BoundingBox.min);
        Vector2 max = ImageToScreenCoordinates(obj.BoundingBox.max);

        /*float width = Math.Abs(max.x - min.x);
        float height = Math.Abs(max.y - min.y);
        float area = width * height;

        Debug.Log($"AREA: {area}");

        if(area > 800000)
        {
            return;
        }*/

        Vector3[] corners = new Vector3[5];

        corners[0] = ScreenToWorld(min.x, min.y);
        corners[1] = ScreenToWorld(max.x, min.y);
        corners[2] = ScreenToWorld(max.x, max.y);
        corners[3] = ScreenToWorld(min.x, max.y);
        corners[4] = corners[0];

        visual.line.positionCount = corners.Length;
        visual.line.SetPositions(corners);

        visual.label.text = $"{obj.CocoName} {(obj.Confidence * 100f):0}%";
        visual.label.transform.position = ScreenToWorld(min.x, max.y + 10f);
        visual.label.transform.LookAt(_camera.transform);
        visual.label.transform.Rotate(0f, 180f, 0f);


    }

    private Vector3 ScreenToWorld(float x, float y)
    {
        return _camera.ScreenToWorldPoint(
        new Vector3(x, y, _depthFromCamera)
);
    }

    public void ClearModels()
    {
        foreach (var p in predicted)
        {
            if(p.Visual.root != null)
            {
                Destroy(p.Visual.root);
            }
            
        }
        predicted.Clear();

    }


    #endregion

    #region Helper Methods

  


    public Vector2 ImageToScreenCoordinates(Vector2 coordinates)
    {
        FeedDimensions feedDimensions = _videoFeedManager.GetFeedDimensions();

        var xOffset = (_camera.scaledPixelWidth - feedDimensions.Width) / 2f;
        var yOffset = (_camera.scaledPixelHeight - feedDimensions.Height) / 2f;

        var newX = coordinates.x + xOffset;
        var newY = (feedDimensions.Height - coordinates.y) + yOffset;

        
        
        return new Vector2(newX, newY - 200f);

    }

    private Vector2 ImageToScreenForCollision(Vector2 coordinates)
    {
        FeedDimensions feedDimensions = _videoFeedManager.GetFeedDimensions();

        var xOffset = (_camera.scaledPixelWidth - feedDimensions.Width) / 2f;
        var yOffset = (_camera.scaledPixelHeight - feedDimensions.Height) / 2f;

        float x = coordinates.x + xOffset;

        float y = (feedDimensions.Height - coordinates.y) + yOffset;



        return new Vector2(x, y - 200f);

    }

    public Rect GetCarRect(DetectedObject obj)
    {
        var predict = predicted.FirstOrDefault(p => p.lastObject == obj);

        Vector2 min, max;
        if(predict != null)
        {
            min = predict.CurrentMin;
            max = predict.CurrentMax;
        }
        else
        {
            min = ImageToScreenForCollision(obj.BoundingBox.min);
            max = ImageToScreenForCollision(obj.BoundingBox.max);
        }
        return Rect.MinMaxRect(Mathf.Min(min.x, max.x), Mathf.Min(min.y, max.y),
            Mathf.Max(min.x, max.x), Mathf.Max(min.y,max.y));   
    }

    #endregion
}

