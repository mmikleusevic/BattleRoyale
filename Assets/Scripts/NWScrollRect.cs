using UnityEngine;
using UnityEngine.UI;

public class NWScrollRect : ScrollRect
{
    /// <summary>
    /// ScrollRect.LateUpdate calls this function with very tiny values every frame,
    /// only if scrolling is not needed and even when velocity is zero.
    /// This makes text jitter. Check before setting position.
    /// </summary>

    protected override void SetContentAnchoredPosition(Vector2 position)
    {
        if (Application.isPlaying && verticalScrollbar != null && !verticalScrollbar.IsActive())
        {
            if (content.anchoredPosition != Vector2.zero)
            {
                position = Vector2.zero;
            }
            else
            {
                return;
            }
        }

        // fires every frame in editor, but without this text jitters
        if (position != Vector2.zero && Approximately(content.anchoredPosition, position)
            && Approximately(position, Vector2.zero))
        {
            position = Vector2.zero;
        }

        base.SetContentAnchoredPosition(position);
    }

    /// <summary>
    /// Called when scrolling would occur.
    /// Prevent setting when vertical scrollbar is disabled and scrolling is not needed to prevent jittering.
    /// </summary>
    protected override void SetNormalizedPosition(float value, int axis)
    {
        if (Application.isPlaying && verticalScrollbar != null && !verticalScrollbar.IsActive())
        {
            return;
        }
        base.SetNormalizedPosition(value, axis);
    }

    private static bool Approximately(Vector2 vec1, Vector2 vec2, float threshold = 0.01f)
    {
        return ((vec1.x < vec2.x) ? (vec2.x - vec1.x) : (vec1.x - vec2.x)) <= threshold
            && ((vec1.y < vec2.y) ? (vec2.y - vec1.y) : (vec1.y - vec2.y)) <= threshold;
    }
}