using UnityEngine;

public class AnimatedBorder : MonoBehaviour
{
    [SerializeField] private RectTransform borderRect;
    [SerializeField] private RectTransform[] dots;
    [SerializeField] private float speed = 0.12f;

    private float progress;
    private void Update()
    {
        if (borderRect == null || dots == null || dots.Length == 0)
        {
            return;
        }

        progress += speed * Time.unscaledDeltaTime;
        progress %= 1f;

        for (int i = 0; i < dots.Length; i++)
        {
            float offset = (float)i / dots.Length;
            float t = (progress + offset) % 1f;

            dots[i].anchoredPosition = GetBorderPosition(t);
        }
    }

    private Vector2 GetBorderPosition(float t)
    {
        Rect rect = borderRect.rect;

        float halfWidth = rect.width * 0.5f;
        float halfHeight = rect.height * 0.5f;

        float width = rect.width;
        float height = rect.height;

        float perimeter = (width * 2f) + (height * 2f);
        float distance = t * perimeter;

        if (distance < width)
        {
            return new Vector2(-halfWidth + distance, halfHeight);
        }

        distance -= width;

        if (distance < height)
        {
            return new Vector2(halfWidth, halfHeight - distance);
        }
        
        distance -=height;

        if (distance < width)
        {
            return new Vector2(halfWidth - distance, halfHeight);
        }

        distance -= width;

        return new Vector2(-halfWidth, -halfHeight + distance);
    }
}
