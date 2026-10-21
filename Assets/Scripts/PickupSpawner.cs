using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupSpawner : MonoBehaviour
{
    [Header("References")]
    public GameObject[] orbPrefabs;
    public PacStudentController pacStudent;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnInterval = 8.0f;
    [SerializeField] private int maxOrbsOnMap = 2;
    [SerializeField] private float orbLifetime = 12.0f;
    [SerializeField] private float minDistanceFromPacStudent = 5.0f;

    private List<GameObject> activeOrbs = new List<GameObject>();

    private void Start()
    {
        if (orbPrefabs == null || orbPrefabs.Length == 0)
        {
            Debug.LogWarning("PickupSpawner: orbPrefabs array is missing or empty in the Inspector!");
        }

        if (pacStudent == null)
        {
            Debug.LogWarning("PickupSpawner: pacStudent reference is missing in the Inspector!");
        }

        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (GameManager.Instance == null || GameManager.Instance.currentState != GameState.InGame)
            {
                continue;
            }

            activeOrbs.RemoveAll(orb => orb == null);

            if (activeOrbs.Count >= maxOrbsOnMap)
            {
                continue;
            }

            TrySpawnOrb();
        }
    }

    private void TrySpawnOrb()
    {
        if (orbPrefabs == null || orbPrefabs.Length == 0 || pacStudent == null) return;

        // 1. Chọn ngẫu nhiên 1 prefab trong mảng 3 prefabs
        GameObject selectedPrefab = orbPrefabs[Random.Range(0, orbPrefabs.Length)];
        if (selectedPrefab == null) return;

        // 2. Tìm tất cả Pellet hợp lệ
        GameObject[] pellets = GameObject.FindGameObjectsWithTag("Pellet");
        if (pellets.Length == 0) return;

        List<GameObject> candidatePellets = new List<GameObject>();
        Vector3 pacPos = pacStudent.transform.position;

        foreach (GameObject pellet in pellets)
        {
            if (pellet != null)
            {
                float distance = Vector3.Distance(pellet.transform.position, pacPos);
                if (distance >= minDistanceFromPacStudent)
                {
                    candidatePellets.Add(pellet);
                }
            }
        }

        if (candidatePellets.Count == 0) return;

        // 3. Chọn ngẫu nhiên vị trí ô Pellet đủ khoảng cách
        GameObject chosenPellet = candidatePellets[Random.Range(0, candidatePellets.Count)];
        Vector3 spawnPos = chosenPellet.transform.position;

        // 4. Instantiate prefab orb được chọn (không cần SetType nữa vì prefab đã có sẵn abilityType)
        GameObject newOrb = Instantiate(selectedPrefab, spawnPos, Quaternion.identity);

        Destroy(newOrb, orbLifetime);
        activeOrbs.Add(newOrb);
    }
}