using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AbilityManager : MonoBehaviour
{
    public static AbilityManager Instance { get; private set; }

    [Header("UI References")]
    public Image slotIcon;

    [Header("Ability Sprites")]
    public Sprite speedSprite;
    public Sprite freezeSprite;
    public Sprite shieldSprite;

    [Header("Ability Settings")]
    public PacStudentController pacStudent;
    [SerializeField] private float speedBoostDuration = 4.0f;
    [SerializeField] private float speedMultiplierValue = 1.6f;
    [SerializeField] private float freezeDuration = 4.0f;
    [SerializeField] private float shieldGraceDuration = 1.0f;

    public AbilityType CurrentAbility { get; private set; } = AbilityType.None;
    public bool IsSpeedBoostActive { get; private set; } = false;
    public bool IsFreezeActive { get; private set; } = false;
    public bool IsShieldActive { get; private set; } = false;
    public bool IsInGracePeriod { get; private set; } = false;

    private Coroutine speedBoostRoutine;
    private Coroutine freezeRoutine;
    private Coroutine gracePeriodRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (pacStudent == null)
        {
            pacStudent = Object.FindFirstObjectByType<PacStudentController>();
        }

        UpdateSlotUI();
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.currentState != GameState.InGame)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space) && CurrentAbility != AbilityType.None)
        {
            UseCurrentAbility();
        }
    }

    public void CollectAbility(AbilityType type)
    {
        CurrentAbility = type;
        UpdateSlotUI();
    }

    private void UseCurrentAbility()
    {
        AbilityType usedAbility = CurrentAbility;
        CurrentAbility = AbilityType.None;
        UpdateSlotUI();

        switch (usedAbility)
        {
            case AbilityType.SpeedBoost:
                TriggerSpeedBoost();
                break;

            case AbilityType.Freeze:
                TriggerFreeze();
                break;

            case AbilityType.Shield:
                IsShieldActive = true;
                Debug.Log("[AbilityManager] Shield activated!");
                break;
        }
    }

    public bool TryAbsorbHit()
    {
        if (IsShieldActive)
        {
            IsShieldActive = false;
            StartGracePeriod();
            Debug.Log("[AbilityManager] Shield absorbed a hit!");
            return true;
        }

        if (IsInGracePeriod)
        {
            Debug.Log("[AbilityManager] Hit absorbed during grace period!");
            return true;
        }

        return false;
    }

    private void StartGracePeriod()
    {
        if (gracePeriodRoutine != null)
        {
            StopCoroutine(gracePeriodRoutine);
        }
        gracePeriodRoutine = StartCoroutine(GracePeriodRoutine());
    }

    private IEnumerator GracePeriodRoutine()
    {
        IsInGracePeriod = true;
        yield return new WaitForSeconds(shieldGraceDuration);
        IsInGracePeriod = false;
        gracePeriodRoutine = null;
    }

    private void TriggerSpeedBoost()
    {
        if (speedBoostRoutine != null)
        {
            StopCoroutine(speedBoostRoutine);
        }

        speedBoostRoutine = StartCoroutine(SpeedBoostRoutine());
    }

    private IEnumerator SpeedBoostRoutine()
    {
        IsSpeedBoostActive = true;

        if (pacStudent != null)
        {
            pacStudent.speedMultiplier = speedMultiplierValue;
        }

        yield return new WaitForSeconds(speedBoostDuration);

        if (pacStudent != null)
        {
            pacStudent.speedMultiplier = 1.0f;
        }

        IsSpeedBoostActive = false;
        speedBoostRoutine = null;
    }

    private void TriggerFreeze()
    {
        if (freezeRoutine != null)
        {
            StopCoroutine(freezeRoutine);
        }

        freezeRoutine = StartCoroutine(FreezeRoutine());
    }

    private IEnumerator FreezeRoutine()
    {
        IsFreezeActive = true;
        SetAllGhostsFrozen(true);

        yield return new WaitForSeconds(freezeDuration);

        SetAllGhostsFrozen(false);
        IsFreezeActive = false;
        freezeRoutine = null;
    }

    private void SetAllGhostsFrozen(bool freeze)
    {
        if (GameManager.Instance == null || GameManager.Instance.ghosts == null) return;

        foreach (GhostController ghost in GameManager.Instance.ghosts)
        {
            if (ghost != null)
            {
                if (freeze && ghost.CurrentState == GhostState.Dead)
                {
                    continue;
                }
                ghost.SetFrozen(freeze);
            }
        }
    }

    private void UpdateSlotUI()
    {
        if (slotIcon == null) return;

        if (CurrentAbility == AbilityType.None)
        {
            slotIcon.enabled = false;
        }
        else
        {
            switch (CurrentAbility)
            {
                case AbilityType.SpeedBoost:
                    if (speedSprite != null) slotIcon.sprite = speedSprite;
                    break;

                case AbilityType.Freeze:
                    if (freezeSprite != null) slotIcon.sprite = freezeSprite;
                    break;

                case AbilityType.Shield:
                    if (shieldSprite != null) slotIcon.sprite = shieldSprite;
                    break;
            }

            slotIcon.enabled = true;
        }
    }

    public void ResetAllAbilities()
    {
        if (speedBoostRoutine != null)
        {
            StopCoroutine(speedBoostRoutine);
            speedBoostRoutine = null;
        }

        if (freezeRoutine != null)
        {
            StopCoroutine(freezeRoutine);
            freezeRoutine = null;
        }

        if (gracePeriodRoutine != null)
        {
            StopCoroutine(gracePeriodRoutine);
            gracePeriodRoutine = null;
        }

        if (pacStudent != null)
        {
            pacStudent.speedMultiplier = 1.0f;
        }

        SetAllGhostsFrozen(false);

        IsSpeedBoostActive = false;
        IsFreezeActive = false;
        IsShieldActive = false;
        IsInGracePeriod = false;
        CurrentAbility = AbilityType.None;
        UpdateSlotUI();
    }
}