using UnityEngine;

public class CoinSpawner : MonoBehaviour
{
    public GameObject coinPrefab;
    public GameObject magnetPrefab;
    public Transform track;
    public Transform kartAnchor;
    public Transform kart;

    public float spawnDistance = 12f;
    public float spawnHeight = 0.5f;
    public float trackHalfWidth = 2.2f;

    public float magnetChance = 0.20f;
    public float magnetInterval = 8f;

    private float magnetTimer;
    public float spawnInterval = 1.0f;
    private float timer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        magnetTimer += Time.deltaTime;

        if (timer > spawnInterval)
        {
            SpawnCoin();
            timer = 0f;
        }

        if (magnetTimer > magnetInterval)
        {
            SpawnMagnet();
            magnetTimer = 0f;
        }
    }

    void SpawnCoin()
    {
        float x = Random.Range(-trackHalfWidth, trackHalfWidth);

        Vector3 spawnPos = kartAnchor.position + track.forward * spawnDistance + track.right * x;

        spawnPos.y = track.position.y + spawnHeight;

        GameObject coin = Instantiate(coinPrefab, spawnPos, Quaternion.identity);

        coin.GetComponent<CoinMovement>().track = track;
        coin.GetComponent<CoinMovement>().kart = kart;
    }

    void SpawnMagnet()
    {
        if(Random.value > magnetInterval)
        {
            return;
        }

        float x = Random.Range(-trackHalfWidth, trackHalfWidth);

        Vector3 spawnPos = kartAnchor.position + track.forward * spawnDistance + track.right * x;

        spawnPos.y = track.position.y + spawnHeight;

        GameObject magnet = Instantiate(magnetPrefab, spawnPos, Quaternion.identity);

        magnet.GetComponent<MagnetMovement>().track = track;


    }
}
