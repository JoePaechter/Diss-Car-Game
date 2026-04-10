
//commented out libararies im not using anymore
//using AYellowpaper.SerializedCollections;
//using Meta.XR;
//using Meta.XR.MRUtilityKit;
using MyBox;
using NUnit.Framework.Constraints;
//using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;
//using YOLOTools.ObjectDetection;
using YOLOTools.Utilities;
using YOLOTools.YOLO.ObjectDetection;
using UnityEngine.UI;
using UnityEngine.XR;


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
    

    public Image XSprite;

   
    public InvincibilityController inv;
    public SpeedController speed;


    [SerializeField] private Camera uiCamera;
    public RectTransform canvasRect;
    public RectTransform shieldRect;
    public UnityEngine.UI.Image shieldImage;





    public bool trafficLightOnScreen;


    [SerializeField] private float BusPowerUpWindow = 10f;
    private float BusPowerUpTimer = 10f;
    private bool XbuttonPressed = false;
    private LineRenderer border;






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

        public Vector2 RawMin; //raw yolo min and max in screen coordinates
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
        speed = FindFirstObjectByType<SpeedController>();
        inv = FindFirstObjectByType<InvincibilityController>();


        //temp
        /*shieldRect.anchorMin = new Vector2(0.5f, 0.5f);
        shieldRect.anchorMax = new Vector2(0.5f, 0.5f);
        shieldRect.pivot = new Vector2(0.5f, 0.5f);
        shieldRect.localScale = Vector3.one;
        shieldRect.anchoredPosition = Vector2.zero;
        shieldRect.sizeDelta = new Vector2(300, 300);

        shieldRect.gameObject.SetActive(false);
        shieldImage.enabled = false;*/

        
        //create warning border
        GameObject borderObject = new GameObject("Border");
        borderObject.transform.SetParent(transform);
        border = borderObject.AddComponent<LineRenderer>();
        border.useWorldSpace = true;
        border.loop = true;
        border.material = new Material(Shader.Find("Unlit/Color"));
        border.material.color = Color.red;
        border.startWidth = 0.5f;
        border.endWidth = 0.5f;
        border.positionCount = 4;
        border.enabled = false;

        trafficLightOnScreen = false;

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
                // check to see if thing is bus
                if (obj.CocoName == "bicycle" )
                {
                    speed.TurnOnSpeed();
                    Debug.Log("Speed on");
                }


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

        bool currentTrafficLight = false;

            //destroy boxes
            for (int i = predicted.Count - 1; i >= 0; i--)
            {
            if (predicted[i].lastObject.CocoName == "traffic light" || predicted[i].lastObject.CocoName == "stop sign" || predicted[i].lastObject.CocoName == "bus" || predicted[i].lastObject.CocoName == "laptop")
            {
                currentTrafficLight = true;
            }
                if (!matchedObjects.Contains(predicted[i]) && (now - predicted[i].LastSeenRenderTime > ObjectCoolDown))
                {
                    Destroy(predicted[i].Visual.root);
                    predicted.RemoveAt(i);
                }
            }
        if (!currentTrafficLight) 
        {
            trafficLightOnScreen = false;
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

        trafficLightOnScreen = false;
        BusPowerUpTimer += Time.deltaTime;

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
                if(predict.lastObject.CocoName == "traffic light" || predict.lastObject.CocoName == "stop sign" || predict.lastObject.CocoName == "bus" || predict.lastObject.CocoName == "laptop")
                {
                    trafficLightOnScreen = true;
                    //PLAY SOUND HERE
                    //inv.playBusSound();
                    DrawPridictedCross(predict);
                    DrawBorderFrame();
                }
               //DrawPredictedBox(predict);
            }
        }
        CheckForButton();

        
    }

    private bool IsOffScreen(Vector2 min, Vector2 max)
    {
        return max.x < -screenPadding || min.x > _camera.scaledPixelWidth + screenPadding || max.y < -screenPadding || min.y > _camera.scaledPixelHeight + screenPadding;

    }

    private void UpdatePrediction(PredictedVisual predict, DetectedObject newObj, float captureTime)
    {
        Vector2 newMin = ImageToScreenCoordinates(newObj.BoundingBox.min);
        Vector2 newMax = ImageToScreenCoordinates(newObj.BoundingBox.max);
        float changeInTime = captureTime - predict.LastSeenCaptureTime;

        if (changeInTime > 0.001f)
        {
            Vector2 rawVelocityMin = (newMin - predict.RawMin) / changeInTime;
            Vector2 rawVelocityMax = (newMax - predict.RawMax) / changeInTime;

            predict.VelocityMin = Vector2.Lerp(predict.VelocityMin, rawVelocityMin, velocirty_smoothing);
            predict.VelocityMax = Vector2.Lerp(predict.VelocityMax, rawVelocityMax, velocirty_smoothing);
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

        //warn player of bus, laptop just for testing

        if (obj.CocoName == "bus"|| obj.CocoName == "laptop")
        {
            if (BusPowerUpTimer >= BusPowerUpWindow)
            {
                inv.playBusSound();
                BusPowerUpTimer = 0f;
            }
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

    //this is not called unless testing normal boudning boxes
    private void DrawPredictedBox(PredictedVisual predict)
    {
        Vector3[] corners = new Vector3[5];

        corners[0] = ScreenToWorld(predict.CurrentMin.x, predict.CurrentMin.y);
        corners[1] = ScreenToWorld(predict.CurrentMax.x, predict.CurrentMin.y);
        corners[2] = ScreenToWorld(predict.CurrentMax.x, predict.CurrentMax.y);
        corners[3] = ScreenToWorld(predict.CurrentMin.x, predict.CurrentMax.y);
        corners[4] = corners[0];

        predict.Visual.line.positionCount = corners.Length;
        if (trafficLightOnScreen)
        {
            predict.Visual.line.material.color = Color.red;
        }
        predict.Visual.line.SetPositions(corners);

        predict.Visual.label.text = $"{predict.lastObject.CocoName} {(predict.lastObject.Confidence * 100f): 0}%";
        predict.Visual.label.transform.position = ScreenToWorld(predict.CurrentMin.x, predict.CurrentMax.y + 10f);

        
        predict.Visual.label.transform.LookAt(predict.Visual.label.transform.position + _camera.transform.rotation * Vector3.forward,
                                _camera.transform.rotation * Vector3.up);
    }

    private void DrawPridictedCross(PredictedVisual predict)
    {
        Debug.Log($"Drawing cross for {predict.lastObject.CocoName}");
        //predict.Visual.line.enabled = true;

        Vector3[] corners = new Vector3[4];
        corners[0] = ScreenToWorld(predict.CurrentMin.x, predict.CurrentMin.y);
        corners[1] = ScreenToWorld(predict.CurrentMax.x, predict.CurrentMin.y);
        corners[2] = ScreenToWorld(predict.CurrentMax.x, predict.CurrentMax.y);
        corners[3] = ScreenToWorld(predict.CurrentMin.x, predict.CurrentMax.y);

        predict.Visual.line.positionCount = 11;
        predict.Visual.line.material.color = Color.red;

        predict.Visual.line.SetPosition(0, corners[0]);
        predict.Visual.line.SetPosition(1, corners[1]);
        predict.Visual.line.SetPosition(2, corners[2]);
        predict.Visual.line.SetPosition(3, corners[3]);
        predict.Visual.line.SetPosition(4, corners[0]);

        predict.Visual.line.SetPosition(5, corners[0]);

        predict.Visual.line.SetPosition(6, corners[0]);
        predict.Visual.line.SetPosition(7, corners[2]);

        predict.Visual.line.SetPosition(8, corners[2]);

        predict.Visual.line.SetPosition(9, corners[1]);
        predict.Visual.line.SetPosition(10, corners[3]);





        predict.Visual.label.transform.LookAt(predict.Visual.label.transform.position + _camera.transform.rotation * Vector3.forward,
                                _camera.transform.rotation * Vector3.up);


    }

    private void DrawBorderFrame()
    {

        border.SetPosition(0, ScreenToWorld(1050,-60)); // bottom left
        border.SetPosition(1, ScreenToWorld(_camera.scaledPixelWidth-1050, -60)); //bootom right
        border.SetPosition(2, ScreenToWorld(_camera.scaledPixelWidth-1050, _camera.scaledPixelHeight-200)); // top right
        border.SetPosition(3, ScreenToWorld(1050, _camera.scaledPixelHeight-200)); //top left
        border.enabled = true;

    }


  

    /*private void DrawPridictedCross(PredictedVisual predict)
    {
        if (shieldRect == null || shieldImage == null || uiCamera == null)
            return;

        Vector2 min = predict.CurrentMin;
        Vector2 max = predict.CurrentMax;

        float minX = Mathf.Min(min.x, max.x);
        float maxX = Mathf.Max(min.x, max.x);
        float minY = Mathf.Min(min.y, max.y);
        float maxY = Mathf.Max(min.y, max.y);

        Vector2 center = new Vector2((minX + maxX) * 0.5f, (minY + maxY) * 0.5f);
        Vector2 size = new Vector2(maxX - minX, maxY - minY);

        size.x = Mathf.Max(size.x, 40f);
        size.y = Mathf.Max(size.y, 40f);

        RectTransform parentRect = shieldRect.parent as RectTransform;
        if (parentRect == null) return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, center, uiCamera, out Vector2 localPoint))
        {
            shieldRect.anchorMin = new Vector2(0.5f, 0.5f);
            shieldRect.anchorMax = new Vector2(0.5f, 0.5f);
            shieldRect.pivot = new Vector2(0.5f, 0.5f);

            shieldRect.anchoredPosition = localPoint;
            shieldRect.sizeDelta = size;
            shieldRect.localScale = Vector3.one;

            shieldRect.gameObject.SetActive(true);
            shieldImage.enabled = true;
            shieldImage.color = Color.white;
            shieldRect.SetAsLastSibling();

            Debug.Log($"screen center={center}, local={localPoint}, size={size}");
        }
    
    }*/

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
        new Vector3(x, y, _depthFromCamera));
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

    
    

    public void CheckForButton()
    {
        if (!trafficLightOnScreen)
        {
            border.enabled = false;
           if  (BusPowerUpTimer > BusPowerUpWindow)
            {
                return;
            }
            
        }

        

        InputDevice leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);


        if (leftController.TryGetFeatureValue(CommonUsages.primaryButton, out bool pressed))
        {
            if (pressed && !XbuttonPressed) {
                inv.TurnOnInv();
                XbuttonPressed = true;
            }
            }

            XbuttonPressed = pressed;
        }
        
           
        
    }
    #endregion


 