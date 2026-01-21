using Meta.XR;
using Oculus.Platform;
using System.Collections.Generic;
using UnityEngine;
using YOLOTools.Utilities;
using YOLOTools.YOLO.ObjectDetection;

public class CarProxyManager : MonoBehaviour
{
    public YOLOTools.YOLO.YOLOHandler yoloHandler;
    public Camera xrCamera;
    public GameObject carProxy;

    Dictionary<int, CarProxy> activeProxies = new();

    const int CAR_CLASS_ID = 2;

    public EnvironmentRaycastManager environmentRaycastManager;

    private void Start()
    {
        Unity.XR.Oculus.Utils.SetupEnvironmentDepth(
            new Unity.XR.Oculus.Utils.EnvironmentDepthCreateParams()
);
    }
    void Update()
    {
        var detections = yoloHandler.CurrentDetections;
        if (detections == null) return;

        int index = 0;

        foreach (var detection in detections)
        {
            if (detection.CocoClass != CAR_CLASS_ID)
                continue;

            EnsureProxy(index);
            UpdateProxyFromDetection(activeProxies[index], detection);
            index++;
        }

        DisableUnused(index);
    }

    void EnsureProxy(int id)
    {
        if (activeProxies.ContainsKey(id)) return;

        GameObject go = Instantiate(carProxy);
        var proxy = go.GetComponent<CarProxy>();

        if (proxy == null)
        {
            Debug.LogError("CarProxy prefab missing CarProxy component!");
            Destroy(go);
            return;
        }

        activeProxies[id] = proxy;
    }

    void DisableUnused(int usedCount)
    {
        foreach (var kv in activeProxies)
        {
            kv.Value.gameObject.SetActive(kv.Key < usedCount);
        }
    }

    void UpdateProxyFromDetection(CarProxy proxy, DetectedObject obj)
    {
        Vector2 screenCenter = ImageToScreenCoordinates(obj.BoundingBox.center);
        Ray ray = xrCamera.ScreenPointToRay(screenCenter);

        if (environmentRaycastManager.Raycast(ray, out EnvironmentRaycastHit hit))
        {
            Vector3 pos = hit.point;
            float depth = Vector3.Distance(xrCamera.transform.position, hit.point);

            Vector3 size = EstimateCarSize(obj, depth);

            proxy.gameObject.SetActive(true);
            proxy.UpdateProxy(
                pos,
                size,
                Quaternion.LookRotation(-xrCamera.transform.forward)
            );
        }
        else
        {
            proxy.gameObject.SetActive(false);
        }
    }

    Vector3 EstimateCarSize(DetectedObject obj, float depth)
    {
        float pixelWidth = obj.BoundingBox.size.x;
        float pixelHeight = obj.BoundingBox.size.y;

        float metersPerPixel =
            2f * depth * Mathf.Tan(xrCamera.fieldOfView * Mathf.Deg2Rad / 2f)
            / xrCamera.pixelHeight;

        float width = pixelWidth * metersPerPixel;
        float height = pixelHeight * metersPerPixel;

        return new Vector3(
            width,
            Mathf.Clamp(height * 0.5f, 1.2f, 2.0f),
            width * 2.2f
        );
    }

    Vector2 ImageToScreenCoordinates(Vector2 coords)
    {
        FeedDimensions feed = yoloHandler.YOLOCamera.GetFeedDimensions();

        var xOffset = (xrCamera.scaledPixelWidth - feed.Width) / 2f;
        var yOffset = (xrCamera.scaledPixelHeight - feed.Height) / 2f;

        var newX = coords.x + xOffset;
        var newY = (feed.Height - coords.y) + yOffset;

        
        return new Vector2(newX, newY);
    }
}
