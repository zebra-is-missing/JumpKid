using UnityEngine;

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
}