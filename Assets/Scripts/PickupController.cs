using UnityEngine;

public class PickupController : MonoBehaviour
{
    public AbilityType abilityType = AbilityType.SpeedBoost;

    [Header("Skill Sprites")]
    public Sprite speedSprite;
    public Sprite freezeSprite;
    public Sprite shieldSprite;

    [Header("Pulse Animation Settings")]
    [SerializeField] private float pulseAmplitude = 0.15f;
    [SerializeField] private float pulseSpeed = 6.0f;

    private SpriteRenderer spriteRenderer;
    private Vector3 baseScale;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        baseScale = transform.localScale;
    }

    private void Start()
    {
        ApplySpriteByAbilityType();
    }

    private void Update()
    {
        AnimatePulse();
    }

    public void SetType(AbilityType type)
    {
        abilityType = type;
        ApplySpriteByAbilityType();
    }

    private void ApplySpriteByAbilityType()
    {
        if (spriteRenderer == null) return;

        switch (abilityType)
        {
            case AbilityType.SpeedBoost:
                if (speedSprite != null) spriteRenderer.sprite = speedSprite;
                break;

            case AbilityType.Freeze:
                if (freezeSprite != null) spriteRenderer.sprite = freezeSprite;
                break;

            case AbilityType.Shield:
                if (shieldSprite != null) spriteRenderer.sprite = shieldSprite;
                break;

            default:
                break;
        }
    }

    private void AnimatePulse()
    {
        float scaleOffset = Mathf.Sin(Time.time * pulseSpeed) * pulseAmplitude;
        transform.localScale = baseScale * (1f + scaleOffset);
    }
}