using System.Collections.Generic;
using UnityEngine;

public class PlatformSpawner : MonoBehaviour
{
    public static PlatformSpawner Instance;

    [Header("Prefab & Cấu hình")]
    public GameObject platformPrefab;
    public Transform firstPlatform;
    public int initialPlatforms = 8;

    [Header("Góc nghiêng trục Isometric (Tỉ lệ X : Y)")]
    public Vector2 rightSlope = new Vector2(3.8f, 1.45f);
    public Vector2 leftSlope = new Vector2(3.8f, 1.45f);

    [Header("Khoảng cách giữa các bục")]
    public float minDistance = 3.8f;
    public float maxDistance = 4.2f;

    [Header("Cấu hình Đổi hướng Zic-zac")]
    public int minStepsBeforeTurn = 2;
    public int maxStepsBeforeTurn = 4;

    private Vector3 lastPlatformPosition;
    private Vector3 basePlatformScale;
    private List<GameObject> activePlatforms = new List<GameObject>();

    private int currentDirection = 1;
    private int remainingStepsInDirection = 3;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (firstPlatform != null)
        {
            lastPlatformPosition = firstPlatform.position;
            basePlatformScale = firstPlatform.localScale;
            activePlatforms.Add(firstPlatform.gameObject);

            // Gán sprite Level 1 cho bục đứng ban đầu và tính vào bộ đếm bục số 0
            if (UIManager.Instance != null)
            {
                Sprite firstPlatSprite = UIManager.Instance.GetCurrentPlatformSprite();
                SpriteRenderer sr = firstPlatform.GetComponentInChildren<SpriteRenderer>();
                if (sr != null && firstPlatSprite != null)
                {
                    sr.sprite = firstPlatSprite;
                }
            }

            UpdatePlatformSorting(firstPlatform.gameObject);
        }
        else
        {
            lastPlatformPosition = Vector3.zero;
            basePlatformScale = Vector3.one;
        }

        remainingStepsInDirection = Random.Range(minStepsBeforeTurn, maxStepsBeforeTurn + 1);

        for (int i = 0; i < initialPlatforms; i++)
        {
            SpawnNewPlatform();
        }
    }

    public Vector3 GetPlatformAxisDirection(int direction)
    {
        Vector2 slope = (direction > 0) ? rightSlope : leftSlope;
        Vector3 dir = new Vector3(slope.x * direction, slope.y, 0f);
        return dir.normalized;
    }

    public int GetCurrentDirection()
    {
        return currentDirection;
    }

    private void SpawnNewPlatform()
    {
        if (remainingStepsInDirection <= 0)
        {
            currentDirection *= -1;
            remainingStepsInDirection = Random.Range(minStepsBeforeTurn, maxStepsBeforeTurn + 1);
        }

        Vector3 spawnDirection = GetPlatformAxisDirection(currentDirection);
        float distance = Random.Range(minDistance, maxDistance);

        Vector3 newPos = lastPlatformPosition + spawnDirection * distance;
        newPos.z = 0f;

        GameObject newPlat = Instantiate(platformPrefab, newPos, Quaternion.identity);
        newPlat.transform.localScale = basePlatformScale;

        // Lấy Sprite bục từ UIManager theo Level hiện tại
        Sprite platformSprite = null;
        if (UIManager.Instance != null)
        {
            platformSprite = UIManager.Instance.GetCurrentPlatformSprite();
        }

        if (platformSprite != null)
        {
            SpriteRenderer sr = newPlat.GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                sr.sprite = platformSprite;
            }
        }

        UpdatePlatformSorting(newPlat);

        remainingStepsInDirection--;
        activePlatforms.Add(newPlat);
        lastPlatformPosition = newPos;
    }

    private void UpdatePlatformSorting(GameObject plat)
    {
        SpriteRenderer sr = plat.GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingOrder = Mathf.RoundToInt(-plat.transform.position.y * 10);
        }
    }

    public Transform GetNextTargetPlatform()
    {
        if (activePlatforms.Count > 1 && activePlatforms[1] != null)
        {
            return activePlatforms[1].transform;
        }

        if (activePlatforms.Count > 0)
        {
            SpawnNewPlatform();
            if (activePlatforms.Count > 1) return activePlatforms[1].transform;
        }

        return null;
    }

    public void OnPlayerLanded()
    {
        SpawnNewPlatform();

        if (activePlatforms.Count > 0)
        {
            GameObject oldPlat = activePlatforms[0];
            activePlatforms.RemoveAt(0);
            if (oldPlat != null)
            {
                Destroy(oldPlat, 1.0f);
            }
        }
    }
}