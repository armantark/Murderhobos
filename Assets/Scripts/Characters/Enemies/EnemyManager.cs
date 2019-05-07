using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyManager : MonoBehaviour
{
    public PlayerController1 playerController1;
    public PlayerController2 playerController2;
    public GameObject[] enemies;
    public float spawnTime = 3f;
    public Transform[] spawnPoints;
    public int maxEnemies;
    public static int numEnemies = 0;
    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating(nameof(Spawn), spawnTime, spawnTime);
    }
    private void Update()
    {
//        Debug.Log(numEnemies);
    }
    void Spawn()
    {
        //this part should be changed when we actually have a death screen
        if (playerController1.currentHealth <= 0 && playerController2.currentHealth <= 0)
        {
            return;
        }
        int spawnPointIndex = Random.Range(0, spawnPoints.Length);
//        Debug.Log(enemy);
        if (numEnemies < maxEnemies)
        {
            int randIndex = Random.Range(0, enemies.Length);
            Instantiate(enemies[randIndex], spawnPoints[spawnPointIndex].position, spawnPoints[spawnPointIndex].rotation);
        }
    }
}
