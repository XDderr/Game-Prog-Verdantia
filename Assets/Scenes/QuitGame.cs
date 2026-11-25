using UnityEngine;

public class QuitGame : MonoBehaviour
{
    public void ExitApplication()
    {
        Application.Quit();

        // Exits Play Mode when testing inside the Unity Editor
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
