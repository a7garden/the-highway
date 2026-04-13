using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SpawnObject : MonoBehaviour
{
    public GameObject[] spawnObjects;
    public bool spawn = false;
    [Range(0.2f,10)]
    public float interval = 1;
    private int index = 0;
    public int maxSpawnCount = -1;
    private int currentSpawnCount = 0;

    private void Start()
    {
        if (spawn)
        {
            InvokeRepeating("Spawn", interval, interval);
        }
    }

    private void Spawn()
    {
        GameObject.Instantiate(spawnObjects[index], transform.position, Quaternion.LookRotation(transform.forward));
        index++;
        if (index == spawnObjects.Length)
            index = 0;
        if (maxSpawnCount > 0)
        {
            if(++currentSpawnCount == maxSpawnCount)
            {
                CancelInvoke("Spawn");
            }
        }
    }
}
