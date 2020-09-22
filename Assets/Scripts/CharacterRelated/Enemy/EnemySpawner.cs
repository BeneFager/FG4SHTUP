using UnityEngine;
using System.Collections;

/// <summary>
/// Spawns enemies at random spawning points based on the wave it should spawn
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    public enum SpawnState { SPAWNING, WAITING, COUNTING }

    /// <summary>
    /// Wave class for customising waves
    /// </summary>
    [System.Serializable]
    public class Wave
    {
        public string name;
        public int count;
        public float spawnRate;
    }

    public Wave[] waves;
    public Transform[] spawnPoints;

    public float timeBetweenWaves = 5f;
    public float waveCountDown = 0f;

    int nextWaveIndex = 0;
    float searchCountDown = 1f;
    SpawnState state = SpawnState.COUNTING;
    int enemiesSpawned = 0;
    int maxEnemies = 20;
    ObjectPooler objectpooler;

    void Start()
    {
        if (spawnPoints.Length == 0)
        {
            Debug.LogError("No spawnPoints");
        }

        objectpooler = ObjectPooler.instance;
        waveCountDown = timeBetweenWaves;
    }

    void Update()
    {
        if (state == SpawnState.WAITING)
        {
            if (EnemyIsAlive())
            {
                WaveCompleted();
                return;
            }
            else
            {
                return;
            }
        }

        if (waveCountDown <= 0)
        {
            if (state != SpawnState.SPAWNING)
            {
                StartCoroutine(SpawnWave(waves[nextWaveIndex]));
            }
        }
        else
        {
            waveCountDown -= Time.deltaTime;
        }
    }

    /// <summary>
    /// This is expensive but it is dont infequently
    /// </summary>
    /// <returns></returns>
    bool EnemyIsAlive()
    {
        searchCountDown -= Time.deltaTime;
        if (searchCountDown <= 0)
        {
            searchCountDown = 1f;
            if (GameObject.FindGameObjectWithTag("Enemy") == null)
            {
                return false;
            }
        }
        return true;
    }

    void WaveCompleted()
    {
        state = SpawnState.COUNTING;
        waveCountDown = timeBetweenWaves;
        if (nextWaveIndex + 1 > waves.Length - 1)
        {
            nextWaveIndex = 0;
            //looping
            //starta nästa lvl här/slut på spelet etc
        }
        else
        {
            nextWaveIndex++;
        }
    }

    /// <summary>
    /// Spawns waves at random spawnpoints
    /// </summary>
    /// <param name="_wave"></param>
    /// <returns></returns>
    IEnumerator SpawnWave(Wave _wave)
    {
        state = SpawnState.SPAWNING;

        for (int i = 0; i < _wave.count; i++)
        {
            SpawnEnemies();
            yield return new WaitForSeconds(1f / _wave.spawnRate);
        }

        state = SpawnState.WAITING;
        yield break;
    }

    void SpawnEnemies()
    {
        if (enemiesSpawned < maxEnemies)
        {
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

            objectpooler.SpawnFromPool("Enemy", spawnPoint.position, spawnPoint.rotation);
            enemiesSpawned++;
        }
    }
}
