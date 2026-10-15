using UnityEngine;

public class PacStudentController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float timePerTile = 0.2f;

    [Header("References")]
    public Tweener tweener;
    public LevelGenerator levelGenerator;
    public Animator animator;
    public AudioSource audioSource;
    public AudioClip moveSound;
    public ParticleSystem dustParticle;

    private KeyCode lastInput = KeyCode.None;
    private KeyCode currentInput = KeyCode.None;
    private Vector2 currentDirection = Vector2.right;

    void Start()
    {
        if (levelGenerator == null)
            Debug.LogWarning("PacStudentController: LevelGenerator is not assigned in the Inspector!");

        if (tweener == null)
            Debug.LogWarning("PacStudentController: Tweener is not assigned in the Inspector!");

        if (animator != null) animator.speed = 0f;
        if (dustParticle != null) dustParticle.Stop();
    }

    void Update()
    {
        GatherInput();

        bool isMoving = tweener != null && tweener.TweenExists(transform);

        if (!isMoving)
        {
            TryMoveInDirection();
        }
    }

    private void GatherInput()
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) lastInput = KeyCode.W;
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) lastInput = KeyCode.S;
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) lastInput = KeyCode.A;
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) lastInput = KeyCode.D;
    }

    private void TryMoveInDirection()
    {
        Vector2 lastDir = KeyToVector(lastInput);

        if (lastDir != Vector2.zero && IsWalkable(lastDir))
        {
            currentInput = lastInput;
            ExecuteTweenMove(lastDir);
        }
        else if (currentInput != KeyCode.None)
        {
            Vector2 currentDir = KeyToVector(currentInput);
            if (IsWalkable(currentDir))
            {
                ExecuteTweenMove(currentDir);
            }
            else
            {
                StopEffects();
            }
        }
        else
        {
            StopEffects();
        }
    }

    private void ExecuteTweenMove(Vector2 dir)
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + new Vector3(dir.x, dir.y, 0f);

        if (tweener != null)
        {
            tweener.AddTween(transform, startPos, endPos, timePerTile);
        }

        if (dir != currentDirection)
        {
            UpdateDirectionAnimation(dir);
            currentDirection = dir;
        }

        if (animator != null) animator.speed = 1f;

        if (dustParticle != null && !dustParticle.isPlaying)
            dustParticle.Play();

        PlayMovementAudio();
    }

    private void StopEffects()
    {
        if (animator != null) animator.speed = 0f;
        if (dustParticle != null && dustParticle.isPlaying) dustParticle.Stop();
        if (audioSource != null && audioSource.isPlaying) audioSource.Stop();
    }

    private Vector2 KeyToVector(KeyCode key)
    {
        switch (key)
        {
            case KeyCode.W: return Vector2.up;
            case KeyCode.S: return Vector2.down;
            case KeyCode.A: return Vector2.left;
            case KeyCode.D: return Vector2.right;
            default: return Vector2.zero;
        }
    }

    private void UpdateDirectionAnimation(Vector3 dir)
    {
        if (animator == null) return;

        if (dir.x > 0.5f) animator.SetTrigger("MoveRight");
        else if (dir.x < -0.5f) animator.SetTrigger("MoveLeft");
        else if (dir.y > 0.5f) animator.SetTrigger("MoveUp");
        else if (dir.y < -0.5f) animator.SetTrigger("MoveDown");
    }

    private bool IsWalkable(Vector2 dir)
    {
        if (levelGenerator == null) return false;
        Vector3 nextTargetPos = transform.position + new Vector3(dir.x, dir.y, 0f);
        return levelGenerator.IsWalkableGrid(nextTargetPos);
    }

    private void PlayMovementAudio()
    {
        if (audioSource == null || moveSound == null) return;

        if (!audioSource.isPlaying)
        {
            audioSource.clip = moveSound;
            audioSource.loop = true;
            audioSource.Play();
        }
    }
}