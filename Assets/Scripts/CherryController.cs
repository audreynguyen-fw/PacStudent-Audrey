using System.Collections;
using UnityEngine;

public class CherryController : MonoBehaviour
{
    [Header("Cherry Settings")]
    public GameObject cherryPrefab;
    public Tweener tweener;
    public float flyDuration = 6.0f;
    public float spawnInterval = 5.0f;

    private Vector3 mapCenter = new Vector3(13.5f, -14.0f, 0f);
    private GameObject currentCherry = null;

    void Start()
    {
        if (cherryPrefab == null)
            Debug.LogWarning("CherryController: Missing Cherry Prefab reference!");
        if (tweener == null)
            Debug.LogWarning("CherryController: Missing Tweener reference!");

        StartCoroutine(SpawnCherryRoutine());
    }

    private IEnumerator SpawnCherryRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            SpawnAndMoveCherry();

            while (currentCherry != null)
            {
                yield return null;
            }
        }
    }

    private void SpawnAndMoveCherry()
    {
        if (cherryPrefab == null || tweener == null) return;

        float halfWidth = 16.0f;
        float halfHeight = 16.0f;

        int side = Random.Range(0, 4);
        Vector3 spawnPos = Vector3.zero;

        switch (side)
        {
            case 0:
                spawnPos = mapCenter + new Vector3(-halfWidth, Random.Range(-halfHeight, halfHeight), 0f);
                break;
            case 1:
                spawnPos = mapCenter + new Vector3(halfWidth, Random.Range(-halfHeight, halfHeight), 0f);
                break;
            case 2:
                spawnPos = mapCenter + new Vector3(Random.Range(-halfWidth, halfWidth), halfHeight, 0f);
                break;
            case 3:
                spawnPos = mapCenter + new Vector3(Random.Range(-halfWidth, halfWidth), -halfHeight, 0f);
                break;
        }

        Vector3 targetPos = mapCenter - (spawnPos - mapCenter);

        currentCherry = Instantiate(cherryPrefab, spawnPos, Quaternion.identity);

        SpriteRenderer sr = currentCherry.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingOrder = 30;
            sr.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        }

        tweener.AddTween(currentCherry.transform, spawnPos, targetPos, flyDuration);
        StartCoroutine(DestroyCherryAfterDuration(currentCherry, flyDuration));
    }

    private IEnumerator DestroyCherryAfterDuration(GameObject cherry, float duration)
    {
        yield return new WaitForSeconds(duration);
        if (cherry != null)
        {
            Destroy(cherry);
        }
    }
}