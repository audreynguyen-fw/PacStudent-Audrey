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

    public AbilityType CurrentAbility { get; private set; } = AbilityType.None;
    public bool IsSpeedBoostActive { get; private set; } = false;

    private Coroutine speedBoostRoutine;

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
                Debug.Log("[AbilityManager] Used Freeze (Logic in 4b)");
                break;

            case AbilityType.Shield:
                Debug.Log("[AbilityManager] Used Shield (Logic in 4c)");
                break;
        }
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

        if (pacStudent != null)
        {
            pacStudent.speedMultiplier = 1.0f;
        }

        IsSpeedBoostActive = false;
        CurrentAbility = AbilityType.None;
        UpdateSlotUI();
    }
}