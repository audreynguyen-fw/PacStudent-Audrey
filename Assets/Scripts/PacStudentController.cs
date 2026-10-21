using UnityEngine;

public class PacStudentController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float timePerTile = 0.2f;
    [HideInInspector] public float speedMultiplier = 1.0f;

    [Header("References")]
    public Tweener tweener;
    public LevelGenerator levelGenerator;
    public Animator animator;
    public AudioSource movementAudioSource;
    public AudioSource sfxAudioSource;
    public AudioClip moveSound;
    public AudioClip wallHitSound;
    public ParticleSystem dustParticle;
    public ParticleSystem wallHitParticle;

    private KeyCode lastInput = KeyCode.None;
    private KeyCode currentInput = KeyCode.None;
    private Vector2 currentDirection = Vector2.right;

    private bool isBumping = false;
    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;

        if (levelGenerator == null)
            Debug.LogWarning("PacStudentController: LevelGenerator is not assigned in the Inspector!");

        if (tweener == null)
            Debug.LogWarning("PacStudentController: Tweener is not assigned in the Inspector!");

        if (animator != null) animator.speed = 0f;
        if (dustParticle != null) dustParticle.Stop();
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.currentState != GameState.InGame)
        {
            if (GameManager.Instance.currentState == GameState.PacStudentDying)
            {
                return;
            }

            StopEffects();
            return;
        }

        GatherInput();

        bool isMoving = tweener != null && tweener.TweenExists(transform);

        if (!isMoving)
        {
            CheckTeleport();
            TryMoveInDirection();
        }
    }

    public void Die()
    {
        if (tweener != null) tweener.RemoveTween(transform);

        StopEffects();

        if (animator != null)
        {
            animator.speed = 1f;
            animator.SetTrigger("Die");
        }
    }

    public void ResetToStart()
    {
        if (tweener != null) tweener.RemoveTween(transform);

        transform.position = startPosition;
        lastInput = KeyCode.None;
        currentInput = KeyCode.None;
        currentDirection = Vector2.right;
        isBumping = false;
        speedMultiplier = 1.0f;

        if (animator != null)
        {
            animator.ResetTrigger("MoveUp");
            animator.ResetTrigger("MoveDown");
            animator.ResetTrigger("MoveLeft");
            animator.ResetTrigger("MoveRight");
            animator.ResetTrigger("Die");

            animator.Play("PacStudent_Walk_Right");
            animator.Update(0f); // Force Animator cập nhật ngay khung hình xuất phát
            animator.speed = 0f;
        }

        StopEffects();
    }

    private void GatherInput()
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) lastInput = KeyCode.W;
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) lastInput = KeyCode.S;
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) lastInput = KeyCode.A;
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) lastInput = KeyCode.D;
    }

    private void CheckTeleport()
    {
        int x = Mathf.RoundToInt(transform.position.x);
        int y = Mathf.RoundToInt(transform.position.y);

        if (y == -14)
        {
            if (x == 0 && currentDirection == Vector2.left)
            {
                transform.position = new Vector3(27f, -14f, 0f);
            }
            else if (x == 27 && currentDirection == Vector2.right)
            {
                transform.position = new Vector3(0f, -14f, 0f);
            }
        }
    }

    private void TryMoveInDirection()
    {
        Vector2 lastDir = KeyToVector(lastInput);

        if (lastDir != Vector2.zero && IsWalkable(lastDir))
        {
            currentInput = lastInput;
            isBumping = false;
            ExecuteTweenMove(lastDir);
        }
        else if (currentInput != KeyCode.None)
        {
            Vector2 currentDir = KeyToVector(currentInput);
            if (IsWalkable(currentDir))
            {
                isBumping = false;
                ExecuteTweenMove(currentDir);
            }
            else
            {
                HandleWallBump(currentDir);
                StopEffects();
            }
        }
        else
        {
            StopEffects();
        }
    }

    private void HandleWallBump(Vector2 bumpDir)
    {
        if (!isBumping)
        {
            isBumping = true;

            if (sfxAudioSource != null && wallHitSound != null)
            {
                sfxAudioSource.PlayOneShot(wallHitSound);
            }

            if (wallHitParticle != null)
            {
                Vector3 hitPoint = transform.position + new Vector3(bumpDir.x * 0.5f, bumpDir.y * 0.5f, 0f);
                wallHitParticle.transform.position = hitPoint;
                wallHitParticle.Play();
            }
        }
    }

    private void ExecuteTweenMove(Vector2 dir)
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + new Vector3(dir.x, dir.y, 0f);

        if (tweener != null)
        {
            float duration = timePerTile / Mathf.Max(0.1f, speedMultiplier);
            tweener.AddTween(transform, startPos, endPos, duration);
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
        if (movementAudioSource != null && movementAudioSource.isPlaying) movementAudioSource.Stop();
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
        if (movementAudioSource == null || moveSound == null) return;

        if (!movementAudioSource.isPlaying)
        {
            movementAudioSource.clip = moveSound;
            movementAudioSource.loop = true;
            movementAudioSource.Play();
        }
    }
}