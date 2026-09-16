using UnityEngine;

public class PacStudentMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 3.0f;

    [Header("Animators & Audio")]
    public Animator animator;
    public AudioSource audioSource;
    public AudioClip moveSound;
    private Vector3[] waypoints = new Vector3[]
    {
        new Vector3(1f, -1f, 0f),
        new Vector3(6f, -1f, 0f),
        new Vector3(6f, -5f, 0f),
        new Vector3(1f, -5f, 0f) 
    };
    private int currentTargetIndex = 1;
    private float lerpT = 0f;
    private Vector3 startPosition;

    void Start()
    {
        transform.position = waypoints[0];
        startPosition = waypoints[0];
        
        if (audioSource != null && moveSound != null)
        {
            audioSource.clip = moveSound;
            audioSource.loop = true;
            audioSource.Play();
        }

        UpdateDirectionAnimation(waypoints[0], waypoints[1]);
    }

    void Update()
    {
        Vector3 targetPosition = waypoints[currentTargetIndex];
        float distance = Vector3.Distance(startPosition, targetPosition);
        lerpT += (speed / distance) * Time.deltaTime;
        transform.position = Vector3.Lerp(startPosition, targetPosition, lerpT);
        if (lerpT >= 1.0f)
        {
            transform.position = targetPosition;
            startPosition = targetPosition;
            lerpT = 0f;
            int previousIndex = currentTargetIndex;
            currentTargetIndex = (currentTargetIndex + 1) % waypoints.Length;
            UpdateDirectionAnimation(waypoints[previousIndex], waypoints[currentTargetIndex]);
        }
    }
    
    void UpdateDirectionAnimation(Vector3 from, Vector3 to)
    {
        if (animator == null) return;

        Vector3 dir = (to - from).normalized;
        if (dir.x > 0.5f)
        {
            animator.SetTrigger("MoveRight");
        }
        else if (dir.x < -0.5f)
        {
            animator.SetTrigger("MoveLeft");
        }
        else if (dir.y > 0.5f)
        {
            animator.SetTrigger("MoveUp");
        }
        else if (dir.y < -0.5f)
        {
            animator.SetTrigger("MoveDown");
        }
    }
}