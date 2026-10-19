using System.Collections.Generic;
using UnityEngine;

public enum GhostState
{
    Normal,
    Scared,
    Recovering,
    Dead
}

public enum GhostDirection
{
    Down,
    Up,
    Left,
    Right
}

public class GhostController : MonoBehaviour
{
    public GhostState CurrentState { get; private set; } = GhostState.Normal;
    public GhostDirection CurrentDirection { get; private set; } = GhostDirection.Down;

    [Header("Ghost Identity")]
    public int ghostId = 1;

    [Header("Movement References")]
    public Tweener tweener;
    public LevelGenerator levelGenerator;
    public PacStudentController pacStudent;

    private Animator animator;
    private string currentAnimName = "";
    private bool isLeavingHouse = true;
    private Vector3 spawnPosition;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
            if (animator == null)
            {
                Debug.LogWarning($"GhostController on {gameObject.name}: Missing Animator component!");
            }
        }
    }

    private void Start()
    {
        spawnPosition = transform.position;

        if (tweener == null) Debug.LogWarning($"GhostController on {gameObject.name}: Tweener is missing!");
        if (levelGenerator == null) Debug.LogWarning($"GhostController on {gameObject.name}: LevelGenerator is missing!");
        if (pacStudent == null) Debug.LogWarning($"GhostController on {gameObject.name}: PacStudentController is missing!");
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.currentState != GameState.InGame)
        {
            return;
        }

        if (tweener != null && !tweener.TweenExists(transform))
        {
            ChooseNextMoveAndMove();
        }
    }

    public void SetState(GhostState newState)
    {
        CurrentState = newState;

        if (newState == GhostState.Dead && tweener != null)
        {
            tweener.RemoveTween(transform);
        }

        UpdateAnimation();
    }

    public void SetDirection(GhostDirection newDirection)
    {
        CurrentDirection = newDirection;
        UpdateAnimation();
    }

    public void ResetLeavingHouseState()
    {
        isLeavingHouse = true;
    }

    private void ChooseNextMoveAndMove()
    {
        if (levelGenerator == null || tweener == null) return;

        if (CurrentState == GhostState.Dead)
        {
            MoveDeadToHome();
            return;
        }

        if (isLeavingHouse)
        {
            MoveToExitHouse();
        }
        else
        {
            MoveInMaze();
        }
    }

    private void MoveDeadToHome()
    {
        float distanceToHome = Vector3.Distance(transform.position, spawnPosition);

        if (distanceToHome < 0.01f)
        {
            transform.position = spawnPosition;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGhostReachedHome(this);
            }

            ResetLeavingHouseState();
            SetDirection(GhostDirection.Down);
            return;
        }

        float timePerTile = GetSpeedForCurrentState();
        float totalDuration = distanceToHome * timePerTile;

        tweener.AddTween(transform, transform.position, spawnPosition, totalDuration);
    }

    private void MoveToExitHouse()
    {
        int currentX = Mathf.RoundToInt(transform.position.x);
        int currentY = Mathf.RoundToInt(transform.position.y);

        int targetX = (currentX <= 13) ? 13 : 14;

        GhostDirection nextDir;

        if (currentX != targetX)
        {
            nextDir = (currentX < targetX) ? GhostDirection.Right : GhostDirection.Left;
        }
        else
        {
            bool exitUp = (ghostId % 2 != 0);
            nextDir = exitUp ? GhostDirection.Up : GhostDirection.Down;

            if (exitUp && currentY >= -11)
            {
                isLeavingHouse = false;
                MoveInMaze();
                return;
            }
            else if (!exitUp && currentY <= -17)
            {
                isLeavingHouse = false;
                MoveInMaze();
                return;
            }
        }

        SetDirection(nextDir);

        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + DirectionToVector3(nextDir);
        float duration = GetSpeedForCurrentState();

        tweener.AddTween(transform, startPos, endPos, duration);
    }

    private void MoveInMaze()
    {
        List<GhostDirection> validNonReverseDirs = GetValidNonReverseDirections();

        if (validNonReverseDirs.Count == 0)
        {
            GhostDirection oppDir = GetOppositeDirection(CurrentDirection);
            Vector3 targetPos = transform.position + DirectionToVector3(oppDir);

            if (levelGenerator.IsWalkableForGhost(targetPos))
            {
                ExecuteMove(oppDir);
            }
            return;
        }

        bool isScaredOrRecovering = (CurrentState == GhostState.Scared || CurrentState == GhostState.Recovering);
        
        List<GhostDirection> candidateDirs = new List<GhostDirection>();

        if (isScaredOrRecovering || ghostId == 1)
        {
            candidateDirs = FilterDirectionsByDistance(validNonReverseDirs, awayFromPacStudent: true);
        }
        else if (ghostId == 2)
        {
            candidateDirs = FilterDirectionsByDistance(validNonReverseDirs, awayFromPacStudent: false);
        }

        if (candidateDirs.Count == 0)
        {
            candidateDirs = validNonReverseDirs;
        }

        GhostDirection chosenDir = candidateDirs[Random.Range(0, candidateDirs.Count)];
        ExecuteMove(chosenDir);
    }

    private List<GhostDirection> GetValidNonReverseDirections()
    {
        GhostDirection oppDir = GetOppositeDirection(CurrentDirection);
        List<GhostDirection> result = new List<GhostDirection>();

        GhostDirection[] allDirections = new GhostDirection[]
        {
            GhostDirection.Up,
            GhostDirection.Down,
            GhostDirection.Left,
            GhostDirection.Right
        };

        foreach (GhostDirection dir in allDirections)
        {
            if (dir == oppDir) continue;

            Vector3 targetPos = transform.position + DirectionToVector3(dir);
            if (levelGenerator.IsWalkableForGhost(targetPos))
            {
                result.Add(dir);
            }
        }

        return result;
    }

    private List<GhostDirection> FilterDirectionsByDistance(List<GhostDirection> inputDirections, bool awayFromPacStudent)
    {
        List<GhostDirection> filtered = new List<GhostDirection>();

        if (pacStudent == null) return filtered;

        Vector3 pacPos = pacStudent.transform.position;
        float currentSqrDist = (transform.position - pacPos).sqrMagnitude;

        foreach (GhostDirection dir in inputDirections)
        {
            Vector3 nextTilePos = transform.position + DirectionToVector3(dir);
            float nextSqrDist = (nextTilePos - pacPos).sqrMagnitude;

            if (awayFromPacStudent)
            {
                if (nextSqrDist >= currentSqrDist)
                {
                    filtered.Add(dir);
                }
            }
            else
            {
                if (nextSqrDist <= currentSqrDist)
                {
                    filtered.Add(dir);
                }
            }
        }

        return filtered;
    }

    private void ExecuteMove(GhostDirection chosenDir)
    {
        SetDirection(chosenDir);

        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + DirectionToVector3(chosenDir);
        float duration = GetSpeedForCurrentState();

        tweener.AddTween(transform, startPos, endPos, duration);
    }

    public float GetSpeedForCurrentState()
    {
        float baseTimePerTile = (pacStudent != null) ? pacStudent.timePerTile : 0.2f;

        switch (CurrentState)
        {
            case GhostState.Normal:
                return baseTimePerTile / 0.9f;

            case GhostState.Scared:
            case GhostState.Recovering:
            case GhostState.Dead:
                return (baseTimePerTile / 0.9f) * 2f;

            default:
                return baseTimePerTile / 0.9f;
        }
    }

    private GhostDirection GetOppositeDirection(GhostDirection dir)
    {
        switch (dir)
        {
            case GhostDirection.Up: return GhostDirection.Down;
            case GhostDirection.Down: return GhostDirection.Up;
            case GhostDirection.Left: return GhostDirection.Right;
            case GhostDirection.Right: return GhostDirection.Left;
            default: return GhostDirection.Down;
        }
    }

    private Vector3 DirectionToVector3(GhostDirection dir)
    {
        switch (dir)
        {
            case GhostDirection.Up: return new Vector3(0f, 1f, 0f);
            case GhostDirection.Down: return new Vector3(0f, -1f, 0f);
            case GhostDirection.Left: return new Vector3(-1f, 0f, 0f);
            case GhostDirection.Right: return new Vector3(1f, 0f, 0f);
            default: return Vector3.zero;
        }
    }

    private void UpdateAnimation()
    {
        if (animator == null) return;

        string stateName = "";
        string dirString = CurrentDirection.ToString();

        switch (CurrentState)
        {
            case GhostState.Normal:
                stateName = "Ghost1_Walk_" + dirString;
                break;

            case GhostState.Scared:
                stateName = "Ghost_Scared_" + dirString;
                break;

            case GhostState.Recovering:
                stateName = "Ghost_Recover";
                break;

            case GhostState.Dead:
                stateName = "Ghost_Dead";
                break;
        }

        if (stateName == currentAnimName) return;

        animator.Play(stateName);
        currentAnimName = stateName;
    }
}