using System.IO;
using UnityEngine;

[System.Serializable]
public class EnemyLocationData
{
    public SerializableVector3[] enemyPositions;
}

[System.Serializable]
public struct SerializableVector3
{
    public float x, y, z;

    public SerializableVector3(Vector3 v)
    {
        x = v.x;
        y = v.y;
        z = v.z;
    }

    public Vector3 ToVector3() => new Vector3(x, y, z);
}

public class EnemyLocationSaving : MonoBehaviour
{
    public static EnemyLocationSaving enemylocationsavingsystem;

    [Header("Enemy Spawning")]
    public GameObject[] enemyPrefabs;
    public int maxEnemies = 5;
    public float spawnRadius = 10f;
    public Transform centerPoint;

    private Transform[] enemyPositions;
    private string saveFilePath;

    private void Awake()
    {
        enemylocationsavingsystem = this;
        saveFilePath = Path.Combine(Application.persistentDataPath, "enemylocation.json");
    }

    private void Start()
    {
        LoadEnemyLocationsOrSpawn();
    }

    public void savingdata1()
    {
        SaveEnemyLocations();
    }

    private void OnApplicationQuit()
    {
        SaveEnemyLocations();
    }

    private void SaveEnemyLocations()
    {
        if (enemyPositions == null || enemyPositions.Length == 0)
            return;

        EnemyLocationData data = new EnemyLocationData();
        data.enemyPositions = new SerializableVector3[enemyPositions.Length];

        for (int i = 0; i < enemyPositions.Length; i++)
        {
            if (enemyPositions[i] != null)
                data.enemyPositions[i] = new SerializableVector3(enemyPositions[i].position);
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(saveFilePath, json);

        Debug.Log($"Enemy positions saved to {saveFilePath}");
    }

    private void LoadEnemyLocationsOrSpawn()
    {
        if (File.Exists(saveFilePath))
        {
            try
            {
                string json = File.ReadAllText(saveFilePath);
                EnemyLocationData data = JsonUtility.FromJson<EnemyLocationData>(json);

                if (data != null && data.enemyPositions != null && data.enemyPositions.Length > 0)
                {
                    int loadedCount = data.enemyPositions.Length;
                    enemyPositions = new Transform[maxEnemies];

                    for (int i = 0; i < loadedCount; i++)
                    {
                        Vector3 pos = data.enemyPositions[i].ToVector3();
                        GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
                        GameObject enemy = Instantiate(enemyPrefab, pos, Quaternion.identity);
                        enemy.tag = "Enemy";
                        enemyPositions[i] = enemy.transform;

                        Debug.Log($"Loaded enemy at {pos}");
                    }

                    for (int i = loadedCount; i < maxEnemies; i++)
                    {
                        Vector3 randomPos = centerPoint.position + Random.insideUnitSphere * spawnRadius;
                        randomPos.y = centerPoint.position.y;

                        GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
                        GameObject enemy = Instantiate(enemyPrefab, randomPos, Quaternion.identity);
                        enemy.tag = "Enemy";

                        enemyPositions[i] = enemy.transform;

                        Debug.Log($"Spawned new enemy at {randomPos}");
                    }

                    return;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("Failed to load enemy positions: " + ex.Message);
            }
        }

        SpawnEnemiesRandomly();
    }

    private void SpawnEnemiesRandomly()
    {
        enemyPositions = new Transform[maxEnemies];

        for (int i = 0; i < maxEnemies; i++)
        {
            Vector3 randomPos = centerPoint.position + Random.insideUnitSphere * spawnRadius;
            randomPos.y = centerPoint.position.y;

            GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            GameObject enemy = Instantiate(enemyPrefab, randomPos, Quaternion.identity);
            enemy.tag = "Enemy";

            enemyPositions[i] = enemy.transform;

            Debug.Log($"Spawned enemy at {randomPos}");
        }
    }

    public void respawnenemys()
    {
        int currentCount = 0;
        for (int i = 0; i < enemyPositions.Length; i++)
        {
            if (enemyPositions[i] != null)
                currentCount++;
            else
                enemyPositions[i] = null;
        }

        int enemiesToSpawn = maxEnemies - currentCount;

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            Vector3 randomPos = centerPoint.position + Random.insideUnitSphere * spawnRadius;
            randomPos.y = centerPoint.position.y;

            GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            GameObject enemy = Instantiate(enemyPrefab, randomPos, Quaternion.identity);
            enemy.tag = "Enemy";

            for (int j = 0; j < enemyPositions.Length; j++)
            {
                if (enemyPositions[j] == null)
                {
                    enemyPositions[j] = enemy.transform;
                    break;
                }
            }

            Debug.Log($"Respawned enemy at {randomPos}");
        }
    }
}
