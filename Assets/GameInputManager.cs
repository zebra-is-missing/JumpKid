using UnityEngine;
using UnityEngine.EventSystems;

public class GameInputManager : MonoBehaviour
{
    public static GameInputManager Instance { get; private set; }

    public bool IsInputLocked { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void LockInput()
    {
        IsInputLocked = true;
    }

    public void UnlockInput()
    {
        IsInputLocked = false;
        Input.ResetInputAxes();
    }

    public bool GetJumpPressDown()
    {
        if (IsInputLocked) return false;
        return Input.GetMouseButtonDown(0) && !IsPointerOverUI();
    }

    // Đang giữ tay
    public bool GetJumpPressHeld()
    {
        if (IsInputLocked) return false;
        return Input.GetMouseButton(0);
    }

    // Nhả tay ra
    public bool GetJumpPressUp()
    {
        if (IsInputLocked) return false;
        return Input.GetMouseButtonUp(0);
    }

    public bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;
        if (EventSystem.current.IsPointerOverGameObject()) return true;

        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(i).fingerId))
                    return true;
            }
        }
        return false;
    }
}