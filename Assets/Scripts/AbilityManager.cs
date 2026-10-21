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

    public AbilityType CurrentAbility { get; private set; } = AbilityType.None;

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

        Debug.Log($"[AbilityManager] Used ability: {usedAbility}");
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
}