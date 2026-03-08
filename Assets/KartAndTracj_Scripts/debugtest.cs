using UnityEngine;
using UnityEngine.UI;

public class ForceShowUI : MonoBehaviour
{
    void Start()
    {
        var img = GetComponent<Image>();
        var rt = GetComponent<RectTransform>();

        gameObject.SetActive(true);
        img.enabled = true;
        img.color = Color.red;

        rt.anchorMin = new Vector2(0.3f, 0.3f);
        rt.anchorMax = new Vector2(0.6f, 0.6f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Debug.Log("ForceShowUI ran");
    }
}