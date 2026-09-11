using UnityEngine;
using System.Collections;

public class IntroToGhostMusic : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip firstClip;
    [SerializeField] private AudioClip loopingClip;
    [SerializeField] private float maxWaitSeconds = 3f;

// Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource.clip = firstClip;
        audioSource.loop = false;
        audioSource.Play();
        
        float waitTime = Mathf.Min(firstClip.length, maxWaitSeconds);
        
        StartCoroutine(SwitchToLoopAfter(waitTime));
    }

    private IEnumerator SwitchToLoopAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        audioSource.clip = loopingClip;
        audioSource.loop = true;
        audioSource.Play();
    }
    

    // Update is called once per frame
    void Update()
    {
        
    }
}
