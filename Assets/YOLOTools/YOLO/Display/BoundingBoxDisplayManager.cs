
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

    //list of bounding boxes created, dont wanna throw them away, saves memory
    private List<BoundingBoxVisual> box_list;

    private void Awake()
    {
        box_list = new List<BoundingBoxVisual>();
    }

    public void DisplayModels(List<DetectedObject> objects, Camera referenceCamera)
    {
        Profiler.BeginSample("ObjectDisplayManager.DisplayModels");

        _camera = referenceCamera;

        EnsureListSize(objects.Count);

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

    #region Model Methods
    private void  EnsureListSize(int size)
    {
        if (box_list == null)
            box_list= new List<BoundingBoxVisual>();

        while (box_list.Count < size)
        {
            box_list.Add(CreateVisual());
        }
    }

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
        foreach (var visual in box_list)
        {
            visual.line.gameObject.SetActive(false); 
            visual.label.gameObject.SetActive(false);   
        }
        
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
        Vector2 a = ImageToScreenForCollision(obj.BoundingBox.min);
        Vector2 b = ImageToScreenForCollision(obj.BoundingBox.max);

        float xMin = Mathf.Min(a.x, b.x);
        float xMax = Mathf.Max(a.x, b.x);
        float yMin = Mathf.Min(a.y, b.y);
        float yMax = Mathf.Max(a.y, b.y);

        return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
    }

    #endregion
}

