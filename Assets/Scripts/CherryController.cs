using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class CherryController : MonoBehaviour
{
    public GameObject prefab;
    private GameObject cherry;
    bool isSpawning = false;
    private Bounds[] bounds;
    private Vector2 mapPos;

    private void Awake()
    {
        bounds = new Bounds[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            bounds[i] = transform.GetChild(i).gameObject.GetComponent<BoxCollider2D>().bounds;
        }
    }

    public void Start()
    {
        mapPos = GameObject.FindGameObjectWithTag("Center").transform.position;
        SpawnCherry();
    }

    public void SpawnCherry()
    {
        StartCoroutine(SpawnTimer());
    }
    
    IEnumerator SpawnTimer()
    {
        isSpawning = true;
        yield return new WaitForSeconds(5f);
        cherry = Instantiate(prefab, RandomSpawnPos(), Quaternion.identity, transform);
        StartCoroutine(FlyThroughMap(Random.Range(10, 15), cherry.transform.position));
        isSpawning = false;
    }

    IEnumerator FlyThroughMap(float duration, Vector2 startPos)
    {
        // direction towards center of the map
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            if (cherry == null) yield break;
            cherry.transform.position = Vector2.Lerp(startPos, DestroyAtPos(startPos), t/duration);
            yield return null;
        }
        
        Destroy(cherry);
        StartCoroutine(SpawnTimer());
    }

    Vector2 DestroyAtPos(Vector2 startPos)
    {
        Vector2 dir = (startPos - mapPos).normalized;
        RaycastHit2D hit = Physics2D.Raycast(mapPos, -dir, 100, LayerMask.GetMask("Borders"));
        Vector2 endPoint = hit.collider ? hit.point : mapPos - dir * 14f; 
        
        return endPoint;
    }

    Vector2 RandomSpawnPos()
    {
        Bounds randomBounds = bounds[Random.Range(0, bounds.Length)];
        return new Vector2(Random.Range(randomBounds.min.x, randomBounds.max.x), Random.Range(randomBounds.min.y, randomBounds.max.y));
    }
}
