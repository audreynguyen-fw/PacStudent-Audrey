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

    [Header("Visual Indicators")]
    public GameObject shieldIndicator;
    public GameObject speedIndicator;

    [Header("Audio")]
    public AudioSource sfxSource;
    public AudioClip sfxPickup;
    public AudioClip sfxSpeed;
    public AudioClip sfxFreeze;
    public AudioClip sfxShieldOn;
    public AudioClip sfxShieldBreak;

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

    public float SpeedRemaining { get; private set; } = 0f;
    public float FreezeRemaining { get; private set; } = 0f;

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

        SetIndicatorActive(shieldIndicator, false);
        SetIndicatorActive(speedIndicator, false);

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
        PlaySfx(sfxPickup);
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
                PlaySfx(sfxSpeed);
                TriggerSpeedBoost();
                break;

            case AbilityType.Freeze:
                PlaySfx(sfxFreeze);
                TriggerFreeze();
                break;

            case AbilityType.Shield:
                IsShieldActive = true;
                PlaySfx(sfxShieldOn);
                SetIndicatorActive(shieldIndicator, true);
                break;
        }
    }

    public bool TryAbsorbHit()
    {
        if (IsShieldActive)
        {
            IsShieldActive = false;
            PlaySfx(sfxShieldBreak);
            SetIndicatorActive(shieldIndicator, false);
            StartGracePeriod();
            return true;
        }

        if (IsInGracePeriod)
        {
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
        SpeedRemaining = speedBoostDuration;

        if (pacStudent != null)
        {
            pacStudent.speedMultiplier = speedMultiplierValue;
        }

        while (SpeedRemaining > 0)
        {
            if (SpeedRemaining <= 1.0f)
            {
                bool show = Mathf.FloorToInt(SpeedRemaining * 6f) % 2 == 0;
                SetIndicatorActive(speedIndicator, show);
            }
            else
            {
                SetIndicatorActive(speedIndicator, true);
            }

            SpeedRemaining -= Time.deltaTime;
            yield return null;
        }

        if (pacStudent != null)
        {
            pacStudent.speedMultiplier = 1.0f;
        }

        SetIndicatorActive(speedIndicator, false);
        IsSpeedBoostActive = false;
        SpeedRemaining = 0f;
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
        FreezeRemaining = freezeDuration;
        SetAllGhostsFrozen(true);

        while (FreezeRemaining > 0)
        {
            bool showIndicator = true;
            if (FreezeRemaining <= 1.0f)
            {
                showIndicator = Mathf.FloorToInt(FreezeRemaining * 6f) % 2 == 0;
            }

            UpdateGhostsFreezeIndicator(showIndicator);

            FreezeRemaining -= Time.deltaTime;
            yield return null;
        }

        SetAllGhostsFrozen(false);
        IsFreezeActive = false;
        FreezeRemaining = 0f;
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

    private void UpdateGhostsFreezeIndicator(bool visible)
    {
        if (GameManager.Instance == null || GameManager.Instance.ghosts == null) return;

        foreach (GhostController ghost in GameManager.Instance.ghosts)
        {
            if (ghost != null)
            {
                ghost.SetFreezeIndicatorVisible(visible);
            }
        }
    }

    private void PlaySfx(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    private void SetIndicatorActive(GameObject indicator, bool active)
    {
        if (indicator != null)
        {
            indicator.SetActive(active);
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

        SetIndicatorActive(shieldIndicator, false);
        SetIndicatorActive(speedIndicator, false);

        IsSpeedBoostActive = false;
        IsFreezeActive = false;
        IsShieldActive = false;
        IsInGracePeriod = false;
        SpeedRemaining = 0f;
        FreezeRemaining = 0f;
        CurrentAbility = AbilityType.None;
        UpdateSlotUI();
    }
}