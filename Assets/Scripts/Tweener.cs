using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tweener : MonoBehaviour
{
    private List<Tween> activeTweens = new List<Tween>();

    void Update()
    {
        for (int i = activeTweens.Count - 1; i >= 0; i--)
        {
            Tween tween = activeTweens[i];
            if (tween == null || tween.Target == null)
            {
                activeTweens.RemoveAt(i);
                continue;
            }

            float elapsedTime = Time.time - tween.StartTime;
            float percent = elapsedTime / tween.Duration;

            if (percent >= 1.0f)
            {
                tween.Target.position = tween.EndPos;
                activeTweens.RemoveAt(i);
            }
            else
            {
                tween.Target.position = Vector3.Lerp(tween.StartPos, tween.EndPos, percent);
            }
        }
    }

    public void AddTween(Transform target, Vector3 startPos, Vector3 endPos, float duration)
    {
        if (!TweenExists(target))
        {
            activeTweens.Add(new Tween(target, startPos, endPos, Time.time, duration));
        }
    }

    public bool TweenExists(Transform target)
    {
        foreach (Tween tween in activeTweens)
        {
            if (tween.Target == target) return true;
        }
        return false;
    }

    public void RemoveTween(Transform target)
    {
        if (target == null) return;

        for (int i = activeTweens.Count - 1; i >= 0; i--)
        {
            if (activeTweens[i] != null && activeTweens[i].Target == target)
            {
                activeTweens.RemoveAt(i);
            }
        }
    }
}