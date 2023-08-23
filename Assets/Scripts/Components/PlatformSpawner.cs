using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject platformPrefab;
    private int count = 3;

    [SerializeField]
    private float timeBetSpawnMin = 3.5f;
    [SerializeField]
    private float timeBetSpawnMax = 5f;
    private float timeBetSpawn;

    [SerializeField]
    private float xPos = 25f;
    [SerializeField]
    private float yMin = -1.75f;
    [SerializeField]
    private float yMax = 3.5f;

    private GameObject[] platforms;
    private int currentIndex = 0;

    private Vector2 poolPosition = new Vector2(0, -25); // �ʹݿ� ������ ������ ȭ�� �ۿ� ���ܵ� ��ġ
    private float lastSpawnTime;

    void Start()
    {
        platforms = new GameObject[count];

        for (int i = 0; i < count; i++)
        {
            platforms[i] = Instantiate(platformPrefab, poolPosition, Quaternion.identity);
        }

        lastSpawnTime = 0f;
        timeBetSpawn = 0f;
    }

    void Update()
    {
        if (GameManager.instance.isGameOver)
        {
            return;
        }

        // ������ ��ġ �������� timeBetSpawn �̻� �ð��� �귶�ٸ�
        if (Time.time >= lastSpawnTime + timeBetSpawn)
        {
            lastSpawnTime = Time.time;
            timeBetSpawn = Random.Range(timeBetSpawnMin, timeBetSpawnMax);
            float yPos = Random.Range(yMin, yMax);

            platforms[currentIndex].SetActive(false);
            platforms[currentIndex].SetActive(true);
            platforms[currentIndex].transform.position = new Vector2(xPos, yPos);
            currentIndex++;

            if (currentIndex >= count)
                currentIndex = 0;
        }
    }
}