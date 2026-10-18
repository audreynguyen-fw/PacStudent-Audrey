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

    private Animator animator;
    private string currentAnimName = "";

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

    public void SetState(GhostState newState)
    {
        CurrentState = newState;
        UpdateAnimation();
    }

    public void SetDirection(GhostDirection newDirection)
    {
        CurrentDirection = newDirection;
        UpdateAnimation();
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
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SetState(GhostState.Normal);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SetState(GhostState.Scared);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SetState(GhostState.Recovering);
        if (Input.GetKeyDown(KeyCode.Alpha0)) SetState(GhostState.Dead);
    }
}