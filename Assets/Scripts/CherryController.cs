using System.Collections;
using UnityEngine;

public class CherryController : MonoBehaviour
{
    [Header("Cherry Settings")]
    public GameObject cherryPrefab;
    public Tweener tweener;
    public float flyDuration = 5.0f;
    public float spawnInterval = 5.0f;

    private Vector3 mapCenter = Vector3.zero;

    void Start()
    {
        StartCoroutine(SpawnCherryRoutine());
    }
    private IEnumerator SpawnCherryRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            SpawnAndMoveCherry();
        }
    }

    private void SpawnAndMoveCherry()
    {
        if (cherryPrefab == null || tweener == null) return;

        float spawnRadius = 15.0f;
        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;

        Vector3 spawnPos = mapCenter + new Vector3(Mathf.Cos(randomAngle) * spawnRadius, Mathf.Sin(randomAngle) * spawnRadius, 0f);
        Vector3 targetPos = mapCenter - (spawnPos - mapCenter);

        GameObject cherryObj = Instantiate(cherryPrefab, spawnPos, Quaternion.identity);

        tweener.AddTween(cherryObj.transform, spawnPos, targetPos, flyDuration);
        StartCoroutine(DestroyCherryAfterDuration(cherryObj, flyDuration));
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