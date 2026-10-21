using UnityEngine;

public class PacStudentCollision : MonoBehaviour
{
    private const string TagPellet = "Pellet";
    private const string TagPowerPellet = "PowerPellet";
    private const string TagCherry = "Cherry";
    private const string TagGhost = "Ghost";
    private const string TagPickup = "Pickup";

    private void Start()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("PacStudentCollision: GameManager instance was not found in the scene!");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.currentState != GameState.InGame) return;

        if (other.CompareTag(TagPellet))
        {
            GameManager.Instance.OnPelletEaten();
            Destroy(other.gameObject);
        }
        else if (other.CompareTag(TagPowerPellet))
        {
            GameManager.Instance.OnPowerPelletEaten();
            Destroy(other.gameObject);
        }
        else if (other.CompareTag(TagCherry))
        {
            GameManager.Instance.OnCherryEaten();
            Destroy(other.gameObject);
        }
        else if (other.CompareTag(TagGhost))
        {
            GameManager.Instance.OnGhostTouched(other.gameObject);
        }
        else if (other.CompareTag(TagPickup))
        {
            PickupController pickup = other.GetComponent<PickupController>();
            if (pickup != null)
            {
                Debug.Log($"[PacStudentCollision] Picked up Skill Orb: {pickup.abilityType}");
            }
            Destroy(other.gameObject);
        }
    }
}