using UnityEngine;
using UnityEngine.UI;

public class CameraSettingsUI : MonoBehaviour
{
    // Shared PlayerPrefs keys used by both the Settings scene and RTSCameraController
    public const string KEY_KEYBOARD = "Cam_MoveWithKeyboard";
    public const string KEY_EDGE_SCROLL = "Cam_MoveWithEdgeScrolling";
    public const string KEY_MOUSE_DRAG = "Cam_MoveWithMouseDrag";

    [Header("UI Toggles")]
    [SerializeField] private Toggle keyboardToggle;
    [SerializeField] private Toggle edgeScrollingToggle;
    [SerializeField] private Toggle mouseDragToggle;

    private void Start()
    {
        // Load saved states (defaulting to 1 / true if never set before)
        bool keyboardEnabled = PlayerPrefs.GetInt(KEY_KEYBOARD, 1) == 1;
        bool edgeEnabled = PlayerPrefs.GetInt(KEY_EDGE_SCROLL, 1) == 1;
        bool dragEnabled = PlayerPrefs.GetInt(KEY_MOUSE_DRAG, 1) == 1;

        // Set initial toggle visuals without firing listeners prematurely
        if (keyboardToggle != null)
        {
            keyboardToggle.SetIsOnWithoutNotify(keyboardEnabled);
            keyboardToggle.onValueChanged.AddListener(SetKeyboardMovement);
        }

        if (edgeScrollingToggle != null)
        {
            edgeScrollingToggle.SetIsOnWithoutNotify(edgeEnabled);
            edgeScrollingToggle.onValueChanged.AddListener(SetEdgeScrolling);
        }

        if (mouseDragToggle != null)
        {
            mouseDragToggle.SetIsOnWithoutNotify(dragEnabled);
            mouseDragToggle.onValueChanged.AddListener(SetMouseDrag);
        }
    }

    public void SetKeyboardMovement(bool isEnabled)
    {
        PlayerPrefs.SetInt(KEY_KEYBOARD, isEnabled ? 1 : 0);
        PlayerPrefs.Save();

        // Also update immediately if RTSCameraController is currently loaded in an additive/pause menu
        if (RTSCameraController.instance != null)
        {
            RTSCameraController.instance.SetMoveWithKeyboard(isEnabled);
        }
    }

    public void SetEdgeScrolling(bool isEnabled)
    {
        PlayerPrefs.SetInt(KEY_EDGE_SCROLL, isEnabled ? 1 : 0);
        PlayerPrefs.Save();

        if (RTSCameraController.instance != null)
        {
            RTSCameraController.instance.SetMoveWithEdgeScrolling(isEnabled);
        }
    }

    public void SetMouseDrag(bool isEnabled)
    {
        PlayerPrefs.SetInt(KEY_MOUSE_DRAG, isEnabled ? 1 : 0);
        PlayerPrefs.Save();

        if (RTSCameraController.instance != null)
        {
            RTSCameraController.instance.SetMoveWithMouseDrag(isEnabled);
        }
    }

    private void OnDestroy()
    {
        if (keyboardToggle != null) keyboardToggle.onValueChanged.RemoveListener(SetKeyboardMovement);
        if (edgeScrollingToggle != null) edgeScrollingToggle.onValueChanged.RemoveListener(SetEdgeScrolling);
        if (mouseDragToggle != null) mouseDragToggle.onValueChanged.RemoveListener(SetMouseDrag);
    }
}