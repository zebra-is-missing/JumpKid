using DG.Tweening;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    [Header("Cấu hình Tích lực")]
    public float maxChargeTime = 1.5f;
    public float minJumpDistance = 1.5f;
    public float maxJumpDistance = 7.5f;
    public float jumpHeight = 1.8f;
    public float jumpDuration = 0.7f;
    public float landDuration = 0.0f;

    [Header("Hiển thị & Sprite")]
    public Transform visualTransform;
    public Sprite idleSprite;
    public Sprite jumpSprite;
    public Sprite landSprite;

    [Header("UI Game Over")]
    public GameObject gameOverPanel;

    [Header("Bán kính đáp an toàn")]
    public float landingTolerance = 1.2f;

    private SpriteRenderer spriteRenderer;
    private bool isCharging = false;
    private bool isJumping = false;
    private bool isFalling = false; // Cờ kiểm tra trạng thái rơi
    private float chargeTimer = 0f;
    private Vector3 jumpDirection = Vector3.right;
    private Transform currentTargetPlatform;

    void Awake()
    {
        Instance = this;
        if (visualTransform == null) visualTransform = transform;
        spriteRenderer = visualTransform.GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        SetSprite(idleSprite);
        AlignWithNextPlatform();
    }

    void Update()
    {
        if (UIManager.Instance != null && !UIManager.Instance.isPlaying) return;
        if (isJumping) return;

        var input = GameInputManager.Instance;
        if (input == null) return;

        // 1. Nếu bị khóa đột ngột (ví dụ hiện popup khi đang gồng tay) -> Hủy gồng
        if (input.IsInputLocked)
        {
            if (isCharging)
            {
                isCharging = false;
                chargeTimer = 0f;
                visualTransform.localScale = Vector3.one;
            }
            return;
        }

        // 2. Bắt đầu gồng
        if (input.GetJumpPressDown())
        {
            StartCharging();
        }

        // 3. Đang gồng
        if (isCharging)
        {
            chargeTimer += Time.deltaTime;
            chargeTimer = Mathf.Min(chargeTimer, maxChargeTime);

            float squashRatio = Mathf.Clamp01(chargeTimer / maxChargeTime);
            float scaleY = Mathf.Lerp(1.0f, 0.7f, squashRatio);
            float scaleX = Mathf.Lerp(1.0f, 1.25f, squashRatio);
            visualTransform.localScale = new Vector3(scaleX, scaleY, 1f);
        }

        // 4. Nhả tay -> Nhảy
        if (input.GetJumpPressUp() && isCharging)
        {
            ExecuteJump();
        }
    }

    private void StartCharging()
    {
        isCharging = true;
        chargeTimer = 0f;
        AlignWithNextPlatform();
    }

    private void AlignWithNextPlatform()
    {
        if (PlatformSpawner.Instance == null) return;

        currentTargetPlatform = PlatformSpawner.Instance.PeekNextPlatform();
        if (currentTargetPlatform != null)
        {
            Vector3 diff = currentTargetPlatform.position - transform.position;
            diff.z = 0f;
            jumpDirection = diff.normalized;

            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = (diff.x < 0);
            }
        }
    }

    private void ExecuteJump()
    {
        isCharging = false;
        isJumping = true;
        isFalling = false;

        visualTransform.localScale = Vector3.one;

        float chargeRatio = Mathf.Clamp01(chargeTimer / maxChargeTime);
        float jumpDistance = Mathf.Lerp(minJumpDistance, maxJumpDistance, chargeRatio);

        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + jumpDirection * jumpDistance;
        targetPos.z = 0f;

        SetSprite(jumpSprite);

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayJump();
        }

        transform.DOJump(targetPos, jumpHeight, 1, jumpDuration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                OnJumpFinished(targetPos);
            });
    }

    private void OnJumpFinished(Vector3 landedPos)
    {
        SetSprite(landSprite != null ? landSprite : idleSprite);

        bool isSuccess = false;
        if (currentTargetPlatform != null)
        {
            float distanceToCenter = Vector2.Distance(
                new Vector2(landedPos.x, landedPos.y),
                new Vector2(currentTargetPlatform.position.x, currentTargetPlatform.position.y)
            );

            if (distanceToCenter <= landingTolerance)
            {
                isSuccess = true;
            }
        }

        if (isSuccess)
        {
            if (landDuration > 0)
            {
                DOVirtual.DelayedCall(landDuration, () =>
                {
                    CompleteLanding();
                });
            }
            else
            {
                CompleteLanding();
            }
        }
        else
        {
            TriggerFallAndGameOver();
        }
    }

    private void CompleteLanding()
    {
        SetSprite(idleSprite);
        isJumping = false;
        isFalling = false;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.AddScore(1);
        }

        if (PlatformSpawner.Instance != null)
        {
            PlatformSpawner.Instance.OnPlayerLanded();
        }

        AlignWithNextPlatform();
    }

    private void TriggerFallAndGameOver()
    {
        isFalling = true; // Bật cờ trạng thái rơi trượt

        // Đưa Sorting Order của nhân vật chìm hẳn ra sau bục
        if (spriteRenderer != null)
        {
            if (currentTargetPlatform != null)
            {
                SpriteRenderer platSr = currentTargetPlatform.GetComponentInChildren<SpriteRenderer>();
                if (platSr != null)
                {
                    spriteRenderer.sortingOrder = platSr.sortingOrder - 10;
                }
                else
                {
                    spriteRenderer.sortingOrder = Mathf.RoundToInt(-currentTargetPlatform.position.y * 10) - 10;
                }
            }
            else
            {
                spriteRenderer.sortingOrder -= 50;
            }
        }

        transform.DOMoveY(transform.position.y - 4f, 0.5f).SetEase(Ease.InQuad).OnComplete(() =>
        {
            isJumping = false;

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowGameOver();
            }
            else if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }
        });
    }

    public void UpdateSkin(Sprite newIdle, Sprite newJump, Sprite newLand)
    {
        idleSprite = newIdle;
        jumpSprite = newJump;
        landSprite = newLand;

        if (!isJumping)
        {
            SetSprite(idleSprite);
        }
    }

    private void SetSprite(Sprite sprite)
    {
        if (spriteRenderer != null && sprite != null)
        {
            spriteRenderer.sprite = sprite;
        }
    }

    void LateUpdate()
    {
        // Khi rơi hụt bục thì giữ nguyên layer chìm phía sau
        if (isFalling) return;

        if (spriteRenderer != null)
        {
            // Cộng thêm +15 để chân nhân vật đứng trên mặt bục luôn nổi đè lên trên bục đó
            spriteRenderer.sortingOrder = Mathf.RoundToInt(-transform.position.y * 10) + 15;
        }
    }
}