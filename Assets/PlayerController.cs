using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    [Header("Cấu hình Tích lực")]
    public float maxChargeTime = 1.2f;
    public float minJumpDistance = 1.5f;
    public float maxJumpDistance = 5.5f;
    public float jumpHeight = 1.8f;
    public float jumpDuration = 0.45f;

    [Tooltip("Thời gian giữ tư thế đáp đất (giây) trước khi đứng về Idle")]
    public float landDuration = 0.15f;

    [Header("Hiển thị & Sprite")]
    public UnityEngine.Transform visualTransform;

    [Tooltip("Ảnh nhân vật lúc đứng im")]
    public Sprite idleSprite;

    [Tooltip("Ảnh nhân vật lúc đang bay trên không")]
    public Sprite jumpSprite;

    [Tooltip("Ảnh nhân vật lúc vừa chạm bục (Đáp)")]
    public Sprite landSprite;

    [Header("UI Game Over")]
    public GameObject gameOverPanel;

    private SpriteRenderer playerSpriteRenderer;
    private Vector3 originalScale;

    private bool isCharging = false;
    private float chargeTimer = 0f;
    private bool isJumping = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (visualTransform == null) visualTransform = transform;
        originalScale = visualTransform.localScale;
        playerSpriteRenderer = visualTransform.GetComponentInChildren<SpriteRenderer>();

        if (playerSpriteRenderer != null && idleSprite != null)
        {
            playerSpriteRenderer.sprite = idleSprite;
        }

        UpdateSortingOrder();
        UpdateFacingDirection();
    }

    void Update()
    {
        if (UIManager.Instance != null && !UIManager.Instance.isPlaying)
        {
            return;
        }
        // 1. BẮT ĐẦU NHẤN CHUỘT / PHÍM SPACE
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCharging();
        }
        else if (Input.GetMouseButtonDown(0))
        {
            if (IsPointerOverUI()) return;
            StartCharging();
        }

        // 2. NHẢ CHUỘT / PHÍM SPACE
        if (Input.GetKeyUp(KeyCode.Space))
        {
            ReleaseAndJump();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (isCharging)
            {
                ReleaseAndJump();
            }
        }

        // 3. HIỆU ỨNG NÉN NHÂN VẬT KHI ĐANG TÍCH LỰC
        if (isCharging)
        {
            chargeTimer += Time.deltaTime;
            float chargeRatio = Mathf.Clamp01(chargeTimer / maxChargeTime);

            float squashY = Mathf.Lerp(originalScale.y, originalScale.y * 0.65f, chargeRatio);
            float stretchX = Mathf.Lerp(originalScale.x, originalScale.x * 1.2f, chargeRatio);
            visualTransform.localScale = new Vector3(stretchX, squashY, originalScale.z);
        }
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;

        if (EventSystem.current.IsPointerOverGameObject()) return true;

        if (Input.touchCount > 0 && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
        {
            return true;
        }

        return false;
    }

    public void StartCharging()
    {
        if (isJumping) return;
        isCharging = true;
        chargeTimer = 0f;
    }

    public void ReleaseAndJump()
    {
        if (!isCharging || isJumping) return;

        isCharging = false;
        visualTransform.localScale = originalScale;

        float chargeRatio = Mathf.Clamp01(chargeTimer / maxChargeTime);
        float actualDistance = Mathf.Lerp(minJumpDistance, maxJumpDistance, chargeRatio);

        UnityEngine.Transform nextTarget = PlatformSpawner.Instance != null ? PlatformSpawner.Instance.GetNextTargetPlatform() : null;
        if (nextTarget == null) return;

        int dir = (nextTarget.position.x >= transform.position.x) ? 1 : -1;
        Vector3 moveDirection = PlatformSpawner.Instance.GetPlatformAxisDirection(dir);

        Vector3 landingPos = transform.position + moveDirection * actualDistance;
        landingPos.z = 0f;

        StartCoroutine(JumpRoutine(landingPos, nextTarget));
    }

    private IEnumerator JumpRoutine(Vector3 targetPos, UnityEngine.Transform targetPlatform)
    {
        isJumping = true;
        Vector3 startPos = transform.position;
        float timeElapsed = 0f;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayJump();
        }

        // 1. Lúc bắt đầu phóng lên: Dùng JUMP sprite
        if (playerSpriteRenderer != null && jumpSprite != null)
        {
            playerSpriteRenderer.sprite = jumpSprite;
        }

        bool switchedToLandSprite = false;

        while (timeElapsed < jumpDuration)
        {
            timeElapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(timeElapsed / jumpDuration);

            // 2. KHI ĐẠT ĐỈNH CỦA CÚ NHẢY (progress >= 0.5f): Chuyển sang LAND sprite
            if (!switchedToLandSprite && progress >= 0.5f)
            {
                switchedToLandSprite = true;
                if (playerSpriteRenderer != null && landSprite != null)
                {
                    playerSpriteRenderer.sprite = landSprite;
                }
            }

            Vector3 groundPos = Vector3.Lerp(startPos, targetPos, progress);
            float arc = 4f * jumpHeight * progress * (1f - progress);

            transform.position = new Vector3(groundPos.x, groundPos.y + arc, 0f);
            UpdateSortingOrder();
            yield return null;
        }

        transform.position = targetPos;
        UpdateSortingOrder();

        PolygonCollider2D platCollider = targetPlatform.GetComponentInChildren<PolygonCollider2D>();
        bool isLandedInside = false;

        if (platCollider != null)
        {
            isLandedInside = platCollider.OverlapPoint(transform.position);
        }
        else
        {
            Vector2 offset = transform.position - targetPlatform.position;
            float rx = targetPlatform.localScale.x * 0.85f;
            float ry = targetPlatform.localScale.y * 0.42f;
            isLandedInside = (Mathf.Pow(offset.x / rx, 2) + Mathf.Pow(offset.y / ry, 2)) <= 1.0f;
        }

        if (isLandedInside)
        {
            // 3. Vừa tiếp đất: Nếu muốn giữ dáng đáp một khoảnh khắc rất ngắn (landDuration) trước khi đứng thẳng
            if (landDuration > 0f)
            {
                yield return new WaitForSeconds(landDuration);
            }

            // Chuyển về IDLE sprite khi đã đứng vững
            if (playerSpriteRenderer != null && idleSprite != null)
            {
                playerSpriteRenderer.sprite = idleSprite;
            }

            isJumping = false;
            UpdateSortingOrder();

            if (UIManager.Instance != null)
            {
                UIManager.Instance.AddScore(1);
            }

            PlatformSpawner.Instance.OnPlayerLanded();
            UpdateFacingDirection();
        }
        else
        {
            StartCoroutine(FallDownRoutine(targetPlatform));
        }
    }

    private void UpdateFacingDirection()
    {
        UnityEngine.Transform nextTarget = PlatformSpawner.Instance != null ? PlatformSpawner.Instance.GetNextTargetPlatform() : null;
        if (nextTarget != null && playerSpriteRenderer != null)
        {
            playerSpriteRenderer.flipX = nextTarget.position.x < transform.position.x;
        }
    }

    private void UpdateSortingOrder()
    {
        if (playerSpriteRenderer != null)
        {
            playerSpriteRenderer.sortingOrder = Mathf.RoundToInt(-transform.position.y * 10) + 10;
        }
    }

    private IEnumerator FallDownRoutine(UnityEngine.Transform targetPlatform)
    {
        isJumping = true;

        if (targetPlatform != null && playerSpriteRenderer != null)
        {
            SpriteRenderer platSr = targetPlatform.GetComponentInChildren<SpriteRenderer>();
            int platSorting = platSr != null ? platSr.sortingOrder : Mathf.RoundToInt(-targetPlatform.position.y * 10);

            if (transform.position.y > targetPlatform.position.y)
            {
                playerSpriteRenderer.sortingOrder = platSorting - 5;
            }
            else
            {
                playerSpriteRenderer.sortingOrder = platSorting + 5;
            }
        }

        float fallDuration = 0.35f;
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Vector3 fallPos = startPos + Vector3.down * 0.9f;

        while (elapsed < fallDuration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, fallPos, elapsed / fallDuration);
            yield return null;
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowGameOver();
        }
        else if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.08f);
    }

    // Nhận Sprite mới khi level thay đổi
    public void UpdateSkin(Sprite newIdle, Sprite newJump, Sprite newLand)
    {
        if (newIdle != null) idleSprite = newIdle;
        if (newJump != null) jumpSprite = newJump;
        if (newLand != null) landSprite = newLand;

        if (!isJumping && playerSpriteRenderer != null && idleSprite != null)
        {
            playerSpriteRenderer.sprite = idleSprite;
        }
    }
}