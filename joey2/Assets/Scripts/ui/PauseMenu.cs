using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// <summary>
/// Pause menu manager for Battle scene
/// Handles ESC menu with restart and return to main menu options
/// </summary>
public class PauseMenu : MonoSingleton<PauseMenu>
{
    [Header("UI References")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button returnToMainMenuButton;
    [SerializeField] private Button resumeButton;

    private bool isPaused = false;
    private CanvasGroup canvasGroup;

    private void Start()
    {
        // Initialize UI references if not set in inspector
        if (pauseMenuPanel == null)
        {
            Debug.LogWarning("PauseMenu: pauseMenuPanel not set in inspector, trying to find it.");
            pauseMenuPanel = GameObject.Find("PauseMenuPanel");
        }

        // Get or add CanvasGroup for better control
        if (pauseMenuPanel != null)
        {
            canvasGroup = pauseMenuPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = pauseMenuPanel.AddComponent<CanvasGroup>();
            }
        }
        else
        {
            Debug.LogError("PauseMenu: pauseMenuPanel is null!");
        }

        // Setup button listeners
        SetupButtonListeners();

        // Initially hide the menu
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
    }

    private void Update()
    {
        // Check for ESC key press
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }
        
        // Handle button clicks manually when paused (EventSystem may not work properly with Time.timeScale = 0)
        if (isPaused && Input.GetMouseButtonDown(0))
        {
            HandleButtonClick();
        }
    }

    /// <summary>
    /// Setup button listeners in Start
    /// </summary>
    private void SetupButtonListeners()
    {
        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(OnRestartClicked);
        }
        else
        {
            Debug.LogError("PauseMenu: restartButton is null! Please assign it in Inspector.");
        }

        if (returnToMainMenuButton != null)
        {
            returnToMainMenuButton.onClick.RemoveAllListeners();
            returnToMainMenuButton.onClick.AddListener(OnReturnToMainMenuClicked);
        }
        else
        {
            Debug.LogError("PauseMenu: returnToMainMenuButton is null! Please assign it in Inspector.");
        }

        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveAllListeners();
            resumeButton.onClick.AddListener(OnResumeClicked);
        }
    }

    /// <summary>
    /// Handle button clicks manually when paused
    /// </summary>
    private void HandleButtonClick()
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null) return;

        PointerEventData pointerData = new PointerEventData(eventSystem);
        pointerData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (var result in results)
        {
            // Check if clicked on button or its children
            if (restartButton != null && 
                (result.gameObject == restartButton.gameObject || 
                 result.gameObject.transform.IsChildOf(restartButton.transform)))
            {
                OnRestartClicked();
                return;
            }
            else if (returnToMainMenuButton != null && 
                     (result.gameObject == returnToMainMenuButton.gameObject || 
                      result.gameObject.transform.IsChildOf(returnToMainMenuButton.transform)))
            {
                OnReturnToMainMenuClicked();
                return;
            }
            else if (resumeButton != null && 
                     (result.gameObject == resumeButton.gameObject || 
                      result.gameObject.transform.IsChildOf(resumeButton.transform)))
            {
                OnResumeClicked();
                return;
            }
        }
    }

    /// <summary>
    /// Toggle pause menu visibility
    /// </summary>
    public void TogglePauseMenu()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    /// <summary>
    /// Pause the game and show menu
    /// </summary>
    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        
        if (pauseMenuPanel == null)
        {
            Debug.LogError("PauseMenu: pauseMenuPanel is null! Please assign it in Inspector.");
            return;
        }

        pauseMenuPanel.SetActive(true);
        SetupCanvas();
        SetupButtons();
        EnsureEventSystem();
    }

    /// <summary>
    /// Setup Canvas for pause menu
    /// </summary>
    private void SetupCanvas()
    {
        Canvas canvas = pauseMenuPanel.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("PauseMenu: Canvas not found in parent hierarchy!");
            return;
        }

        // Set high sorting order to ensure menu is on top
        canvas.sortingOrder = 100;

        // For ScreenSpaceCamera mode, ensure camera is set correctly
        if (canvas.renderMode == RenderMode.ScreenSpaceCamera && canvas.worldCamera == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                canvas.worldCamera = mainCamera;
            }
            else
            {
                Debug.LogError("PauseMenu: Main camera not found!");
            }
        }

        // Ensure GraphicRaycaster exists
        GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
        if (raycaster == null)
        {
            canvas.gameObject.AddComponent<GraphicRaycaster>();
        }
        else
        {
            raycaster.enabled = true;
        }
    }

    /// <summary>
    /// Setup buttons when menu opens
    /// </summary>
    private void SetupButtons()
    {
        SetupButton(restartButton, OnRestartClicked);
        SetupButton(returnToMainMenuButton, OnReturnToMainMenuClicked);
        SetupButton(resumeButton, OnResumeClicked);

        // Ensure CanvasGroup allows interaction
        if (canvasGroup != null)
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1f;
        }
    }

    /// <summary>
    /// Setup a single button
    /// </summary>
    private void SetupButton(Button button, UnityEngine.Events.UnityAction onClickAction)
    {
        if (button == null) return;

        button.interactable = true;
        button.enabled = true;
        button.gameObject.SetActive(true);
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(onClickAction);
    }

    /// <summary>
    /// Ensure EventSystem exists and is enabled
    /// </summary>
    private void EnsureEventSystem()
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null)
        {
            // Try to find existing EventSystem
            eventSystem = FindObjectOfType<EventSystem>();
            if (eventSystem == null)
            {
                // Create new EventSystem if none exists
                GameObject eventSystemObj = new GameObject("EventSystem");
                eventSystemObj.AddComponent<EventSystem>();
                eventSystemObj.AddComponent<StandaloneInputModule>();
            }
        }
        else
        {
            eventSystem.enabled = true;
        }
    }

    /// <summary>
    /// Resume the game and hide menu
    /// </summary>
    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Restart current level
    /// </summary>
    public void OnRestartClicked()
    {
        if (restartButton != null)
        {
            restartButton.interactable = false; // Prevent double-clicking
        }
        
        // Resume game before reloading scene
        Time.timeScale = 1f;
        
        // Reload current scene
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneLoader.Instance.LoadScene(currentSceneName);
    }

    /// <summary>
    /// Return to main menu
    /// </summary>
    public void OnReturnToMainMenuClicked()
    {
        if (returnToMainMenuButton != null)
        {
            returnToMainMenuButton.interactable = false; // Prevent double-clicking
        }
        
        // Resume game before loading scene
        Time.timeScale = 1f;
        
        // Load main menu scene (Start scene)
        SceneLoader.Instance.LoadScene("Start");
    }

    /// <summary>
    /// Resume button click handler
    /// </summary>
    public void OnResumeClicked()
    {
        ResumeGame();
    }

    private void OnDestroy()
    {
        // Ensure time scale is reset when menu is destroyed
        Time.timeScale = 1f;
    }
}

