using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("Cấu hình Tích lực")]
    public float maxChargeTime = 1.2f;
    public float minJumpDistance = 1.5f;
    public float maxJumpDistance = 5.5f;
    public float jumpHeight = 1.8f;
    public float jumpDuration = 0.45f;

    [Header("Hiển thị & Sprite")]
    public UnityEngine.Transform visualTransform;

    [Tooltip("Ảnh nhân vật lúc đứng im")]
    public Sprite idleSprite;

    [Tooltip("Ảnh nhân vật lúc đang bay trên không")]
    public Sprite jumpSprite;

    [Header("UI Game Over")]
    public GameObject gameOverPanel;

    private SpriteRenderer playerSpriteRenderer;
    private Vector3 originalScale;

    private bool isCharging = false;
    private float chargeTimer = 0f;
    private bool isJumping = false;

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
        // 1. BẮT ĐẦU NHẤN CHUỘT / PHÍM SPACE
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCharging();
        }
        else if (Input.GetMouseButtonDown(0))
        {
            // Nếu click trúng UI (nút FaceID, Pause, v.v.), bỏ qua hoàn toàn
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
            // Chỉ thực hiện nhảy nếu trước đó đã thực sự bắt đầu tích lực thành công
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

    // Hàm kiểm tra con trỏ chuột hoặc ngón tay cảm ứng có chạm trúng UI không
    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;

        // Chuột trên PC / Editor
        if (EventSystem.current.IsPointerOverGameObject()) return true;

        // Cảm ứng đa điểm trên màn hình điện thoại
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

        if (playerSpriteRenderer != null && jumpSprite != null)
        {
            playerSpriteRenderer.sprite = jumpSprite;
        }

        while (timeElapsed < jumpDuration)
        {
            timeElapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(timeElapsed / jumpDuration);

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
            // TRUYỀN targetPlatform VÀO ĐÂY:
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

        // Hiện màn hình Game Over khi kết thúc hiệu ứng rơi
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    // Hàm gọi khi bấm nút Replay
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.08f);
    }
}