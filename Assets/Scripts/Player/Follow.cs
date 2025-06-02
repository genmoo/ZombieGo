using UnityEngine;

public class Follow : MonoBehaviour
{
    RectTransform rect;
    public Transform target;
    public Vector3 offset;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
    }
    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject); 
            return;
        }

        if (Camera.main)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(target.position + offset);
            rect.position = screenPos;
        }
    }
}