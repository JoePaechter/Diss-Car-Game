
using Oculus.Interaction;
using System.Collections.Generic;
using UnityEngine;



public class CoinSpawner : MonoBehaviour
{
    public GameObject coinPrefab;
    public GameObject magnetPrefab;
    public GameObject InvPrefab;
    public GameObject SpeedPrefab;
    public Transform track;
    public Transform kartAnchor;
    public Transform kart;

    public float spawnDistance = 12f;
    public float spawnHeight = 0.5f;
    public float trackHalfWidth = 2.2f;

    public float magnetChance = 0.20f;
    public float magnetInterval = 8f;
    private float magnetTimer;
    
    private float InvChance = 0f;
    public float InvInterval = 8f;
    private float InvTimer;

    private float SpeedChance = 0f;
    public float SpeedInterval = 8f;
    private float SpeedTimer;


    public float spawnInterval = 1.0f;
    private float timer;

    private static CoinSpawner instance;

    private List<GameObject> coinsSpawned = new List<GameObject>();
    private List<GameObject> powerUpsSpawned = new List<GameObject>();

    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        magnetTimer += Time.deltaTime;
        InvTimer += Time.deltaTime;
        SpeedTimer += Time.deltaTime;

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

        if (InvTimer > InvInterval)
        {
            SpawnInv();
            InvTimer = 0f;
        }

        if (SpeedTimer > SpeedInterval)
        {
            SpawnSpeed();
            SpeedTimer = 0f;
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

        coinsSpawned.Add(coin); 
    }

    void SpawnMagnet()
    {
        /*if(Random.value > magnetInterval)
        {
            return;
        }*/

        float x = Random.Range(-trackHalfWidth, trackHalfWidth);

        Vector3 spawnPos = kartAnchor.position + track.forward * spawnDistance + track.right * x;

        spawnPos.y = track.position.y + spawnHeight;

        GameObject magnet = Instantiate(magnetPrefab, spawnPos, Quaternion.identity);

        magnet.GetComponent<MagnetMovement>().track = track;

        powerUpsSpawned.Add(magnet);


    }

    void SpawnInv()
    {
        /*if (Random.value < InvChance)
        {
            return;
        }*/

        float x = Random.Range(-trackHalfWidth, trackHalfWidth);

        Vector3 spawnPos = kartAnchor.position + track.forward * spawnDistance + track.right * x;

        spawnPos.y = track.position.y + spawnHeight;

        GameObject inv = Instantiate(InvPrefab, spawnPos, Quaternion.identity);

        //do invincibility
        inv.GetComponent<InvMovement>().track = track;

        powerUpsSpawned.Add(inv);


    }

    void SpawnSpeed()
    {
        /*if (Random.value < SpeedChance)
        {
            return;
        }*/

        float x = Random.Range(-trackHalfWidth, trackHalfWidth);

        Vector3 spawnPos = kartAnchor.position + track.forward * spawnDistance + track.right * x;

        spawnPos.y = track.position.y + spawnHeight;

        GameObject speed = Instantiate(SpeedPrefab, spawnPos, Quaternion.identity);

        //do invincibility
        speed.GetComponent<SpeedMovement>().track = track;

        powerUpsSpawned.Add(speed);


    }

    public void resetCoinsAndPowerups()
    {
        foreach (var c in coinsSpawned)
        {
            Destroy(c);
        }

        foreach(var p in powerUpsSpawned)
        {
            Destroy(p);
        }

        coinsSpawned.Clear();
        powerUpsSpawned.Clear();

        timer = 0f;
        magnetTimer = 0f;
    }

    public void RemoveCoinFromList(GameObject coin)
    {
        coinsSpawned.Remove(coin);
    }
    public void RemovepowerUpFromList(GameObject powerUp)
    {
        powerUpsSpawned.Remove(powerUp);
    }


}
