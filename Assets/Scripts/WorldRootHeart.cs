using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class WorldRootHeart : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private UnitInfo heartInfo; // Optional: pulls health if assigned
    [SerializeField] private float fallbackMaxHealth = 500f;
    [SerializeField] private HealthTracker healthTracker; // Optional: world-space health bar

    [Header("Ending Video")]
    [Tooltip("Assign a VideoPlayer component (e.g., on a Canvas RawImage or Camera) with your ending clip.")]
    [SerializeField] private VideoPlayer endingVideoPlayer;
    [Tooltip("Optional UI parent/canvas holding the video screen to activate when destroyed.")]
    [SerializeField] private GameObject videoScreenUI;
    [SerializeField] private bool pauseGameplayDuringVideo = true;

    [Header("Post-Video Flow (Optional)")]
    [SerializeField] private bool loadSceneAfterVideo = false;
    [SerializeField] private string nextSceneName = "MainMenu";

    private float maxHealth;
    private float currentHealth;
    private bool isDestroyed = false;

    private void Start()
    {
        maxHealth = (heartInfo != null) ? heartInfo.health : fallbackMaxHealth;
        currentHealth = maxHealth;

        if (videoScreenUI != null)
        {
            videoScreenUI.SetActive(false);
        }

        UpdateHealthUI();
    }

    public void TakeDamage(float damageToInflict)
    {
        if (isDestroyed) return;

        currentHealth -= damageToInflict;
        currentHealth = Mathf.Max(0f, currentHealth);

        Debug.Log($"World Root Heart took {damageToInflict} damage! Health remaining: {currentHealth}/{maxHealth}");

        UpdateHealthUI();

        if (currentHealth <= 0f)
        {
            DestroyHeart();
        }
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    private void UpdateHealthUI()
    {
        if (healthTracker != null)
        {
            healthTracker.UpdateSliderValue(currentHealth, maxHealth);
        }
    }

    private void DestroyHeart()
    {
        if (isDestroyed) return;
        isDestroyed = true;

        // Remove from attackable layer and disable colliders so units stop swinging
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        foreach (Collider col in GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }

        if (healthTracker != null)
        {
            healthTracker.gameObject.SetActive(false);
        }

        PlayEndingVideo();
    }

    private void PlayEndingVideo()
    {
        if (videoScreenUI != null)
        {
            videoScreenUI.SetActive(true);
        }

        if (endingVideoPlayer != null)
        {
            if (loadSceneAfterVideo)
            {
                endingVideoPlayer.loopPointReached += OnVideoFinished;
            }

            endingVideoPlayer.Play();
        }
        else
        {
            Debug.LogWarning("WorldRootHeart destroyed, but no Ending VideoPlayer is assigned!");
        }

        if (pauseGameplayDuringVideo)
        {
            Time.timeScale = 0f;
        }
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        vp.loopPointReached -= OnVideoFinished;
        Time.timeScale = 1f;

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}