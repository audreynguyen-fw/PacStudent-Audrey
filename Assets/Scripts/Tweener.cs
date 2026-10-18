using System.Collections.Generic;
using UnityEngine;

public class Tweener : MonoBehaviour
{
    private List<Tween> activeTweens = new List<Tween>();

    public bool TweenExists(Transform target)
    {
        foreach (Tween t in activeTweens)
        {
            if (t.Target == target) return true;
        }

        return false;
    }
    public bool AddTween(Transform targetObject, Vector3 startPos, Vector3 endPos, float duration)
    {
        if (!TweenExists(targetObject))
        {
            activeTweens.Add(new Tween(targetObject, startPos, endPos, Time.time, duration));
            return true;
        }

        return false;
    }
    void Update()
    {
        for (int i = activeTweens.Count - 1; i >= 0; i--)
        {
            Tween activeTween = activeTweens[i];
            if (activeTween.Target == null)
            {
                activeTweens.RemoveAt(i);
                continue;
            }
            float timeFraction = (Time.time - activeTween.StartTime) / activeTween.Duration;

            if (timeFraction < 1.0f)
            {
                activeTween.Target.position = Vector3.Lerp(activeTween.StartPos, activeTween.EndPos, timeFraction);
            }
            else
            {
                activeTween.Target.position = activeTween.EndPos;
                activeTweens.RemoveAt(i);
            }
        }
    }
}