using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class GameInputManager : MonoBehaviour
{
    public static GameInputManager Instance { get; private set; }

    public bool IsInputLocked { get; private set; }

    private bool wasPressedLastFrame = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void LateUpdate()
    {
        wasPressedLastFrame = IsCurrentlyPressed();
    }

    public void LockInput()
    {
        IsInputLocked = true;
        wasPressedLastFrame = false;
    }

    public void UnlockInput()
    {
        IsInputLocked = false;
        wasPressedLastFrame = false;
    }

    // Bắt đầu bấm xuống
    public bool GetJumpPressDown()
    {
        if (IsInputLocked) return false;

        bool isPressed = IsCurrentlyPressed();
        if (isPressed && !wasPressedLastFrame)
        {
            if (IsPointerOverUI()) return false;
            return true;
        }

        return false;
    }

    // Đang giữ tay
    public bool GetJumpPressHeld()
    {
        if (IsInputLocked) return false;
        return IsCurrentlyPressed();
    }

    // Nhả tay ra
    public bool GetJumpPressUp()
    {
        if (IsInputLocked) return false;

        bool isPressed = IsCurrentlyPressed();
        return !isPressed && wasPressedLastFrame;
    }

    // Kiểm tra trạng thái chạm màn hình hoặc bấm chuột / phím Space
    private bool IsCurrentlyPressed()
    {
        // 1. Chạm màn hình cảm ứng Android
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            return true;
        }

        // 2. Click chuột trái (cho Editor / PC)
        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            return true;
        }

        // 3. Phím Space
        if (Keyboard.current != null && Keyboard.current.spaceKey.isPressed)
        {
            return true;
        }

        return false;
    }

    public bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;

        // Chuột trên Editor / PC
        if (Mouse.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return true;
        }

        // Cảm ứng trên điện thoại Android
        if (Touchscreen.current != null)
        {
            foreach (var touch in Touchscreen.current.touches)
            {
                if (touch.isInProgress)
                {
                    int fingerId = touch.touchId.ReadValue();
                    if (EventSystem.current.IsPointerOverGameObject(fingerId))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }
}