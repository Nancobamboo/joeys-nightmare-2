using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// <summary>
/// Unified manager for pause and game over states
/// Handles ESC menu (pause) and death UI (game over) with shared logic
/// </summary>
public class MenuManager : MonoSingleton<MenuManager>
{
    [Header("Pause UI References")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private Button pauseRestartButton;
    [SerializeField] private Button pauseReturnToMainMenuButton;
    [SerializeField] private Button resumeButton;

    [Header("Game Over UI References")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button gameOverRestartButton;
    [SerializeField] private Button gameOverReturnToMainMenuButton;

    private bool isPaused = false;
    private bool isGameOver = false;
    private CanvasGroup pauseCanvasGroup;
    private CanvasGroup gameOverCanvasGroup;
    private const string RESTART_BUTTON_NAME = "Restart";
    private const string RETURN_MAIN_MENU_BUTTON_NAME = "ReturnToMainMenu";

    private void Awake()
    {
        InitializeGameOverPanel();
    }

    private void Start()
    {
        InitializePauseMenu();
        ResetGameStates();
        HideGameOverPanel();
    }

    private void Update()
    {
        // Handle ESC key for pause menu (only if not game over)
        if (!isGameOver && Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }

        // Handle button clicks manually when paused or game over
        if ((isPaused || isGameOver) && Input.GetMouseButtonDown(0))
        {
            if (isPaused)
            {
                HandleButtonClick(pauseRestartButton, pauseReturnToMainMenuButton, resumeButton);
            }
            else if (isGameOver)
            {
                HandleButtonClick(gameOverRestartButton, gameOverReturnToMainMenuButton, null);
            }
        }
    }

    #region Pause Logic

    /// <summary>
    /// Initialize pause menu UI references
    /// </summary>
    private void InitializePauseMenu()
    {
        if (pauseMenuPanel == null)
        {
            Debug.LogWarning("MenuManager: pauseMenuPanel not set in inspector, trying to find it.");
            pauseMenuPanel = GameObject.Find("PauseMenuPanel");
        }

        if (pauseMenuPanel != null)
        {
            pauseCanvasGroup = pauseMenuPanel.GetComponent<CanvasGroup>();
            if (pauseCanvasGroup == null)
            {
                pauseCanvasGroup = pauseMenuPanel.AddComponent<CanvasGroup>();
            }
            pauseMenuPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("MenuManager: pauseMenuPanel is null!");
        }

        SetupPauseButtonListeners();
    }

    /// <summary>
    /// Setup pause button listeners
    /// </summary>
    private void SetupPauseButtonListeners()
    {
        if (pauseRestartButton != null)
        {
            pauseRestartButton.onClick.RemoveAllListeners();
            pauseRestartButton.onClick.AddListener(OnRestartClicked);
        }
        else
        {
            Debug.LogError("MenuManager: pauseRestartButton is null! Please assign it in Inspector.");
        }

        if (pauseReturnToMainMenuButton != null)
        {
            pauseReturnToMainMenuButton.onClick.RemoveAllListeners();
            pauseReturnToMainMenuButton.onClick.AddListener(OnReturnToMainMenuClicked);
        }
        else
        {
            Debug.LogError("MenuManager: pauseReturnToMainMenuButton is null! Please assign it in Inspector.");
        }

        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveAllListeners();
            resumeButton.onClick.AddListener(OnResumeClicked);
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
        if (isGameOver) return; // Cannot pause if game is over

        isPaused = true;
        Time.timeScale = 0f;
        
        if (pauseMenuPanel == null)
        {
            Debug.LogError("MenuManager: pauseMenuPanel is null! Please assign it in Inspector.");
            return;
        }

        pauseMenuPanel.SetActive(true);
        SetupCanvas(pauseMenuPanel, pauseCanvasGroup);
        SetupPauseButtons();
        EnsureEventSystem();
    }

    /// <summary>
    /// Setup pause buttons when menu opens
    /// </summary>
    private void SetupPauseButtons()
    {
        SetupButton(pauseRestartButton, OnRestartClicked);
        SetupButton(pauseReturnToMainMenuButton, OnReturnToMainMenuClicked);
        SetupButton(resumeButton, OnResumeClicked);

        if (pauseCanvasGroup != null)
        {
            pauseCanvasGroup.interactable = true;
            pauseCanvasGroup.blocksRaycasts = true;
            pauseCanvasGroup.alpha = 1f;
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

    #endregion

    #region Game Over Logic

    /// <summary>
    /// Initialize GameOver panel - try to find in scene or load from Resources
    /// </summary>
    private void InitializeGameOverPanel()
    {
        if (gameOverPanel != null) return;

        gameOverPanel = GameObject.Find("UIGameOver");

        if (gameOverPanel == null)
        {
            GameObject prefab = Resources.Load<GameObject>("prefabs/UIGameOver");
            if (prefab != null)
            {
                gameOverPanel = Instantiate(prefab);
                gameOverPanel.name = "UIGameOver";
            }
            else
            {
                Debug.LogError("MenuManager: Prefab not found in Resources/prefabs/UIGameOver");
                return;
            }
        }

        gameOverCanvasGroup = gameOverPanel.GetComponent<CanvasGroup>();
        if (gameOverCanvasGroup == null)
        {
            gameOverCanvasGroup = gameOverPanel.AddComponent<CanvasGroup>();
        }

        FindGameOverButtons();
    }

    /// <summary>
    /// Find buttons from the game over panel hierarchy
    /// </summary>
    private void FindGameOverButtons()
    {
        if (gameOverPanel == null) return;

        Transform canvasTransform = GetCanvasTransform();
        if (canvasTransform == null) return;

        if (gameOverRestartButton == null)
        {
            gameOverRestartButton = FindButton(canvasTransform, RESTART_BUTTON_NAME);
        }

        if (gameOverReturnToMainMenuButton == null)
        {
            gameOverReturnToMainMenuButton = FindButton(canvasTransform, RETURN_MAIN_MENU_BUTTON_NAME);
        }
    }

    /// <summary>
    /// Get Canvas transform from the panel
    /// </summary>
    private Transform GetCanvasTransform()
    {
        Transform canvasTransform = gameOverPanel.transform.Find("Canvas");
        if (canvasTransform == null)
        {
            Canvas canvas = gameOverPanel.GetComponentInChildren<Canvas>();
            if (canvas != null)
            {
                canvasTransform = canvas.transform;
            }
        }
        return canvasTransform;
    }

    /// <summary>
    /// Find button by name in the canvas hierarchy
    /// </summary>
    private Button FindButton(Transform canvasTransform, string buttonName)
    {
        Transform buttonTransform = canvasTransform.Find(buttonName);
        if (buttonTransform != null)
        {
            return buttonTransform.GetComponent<Button>();
        }

        Button[] buttons = canvasTransform.GetComponentsInChildren<Button>(true);
        foreach (Button btn in buttons)
        {
            if (btn.gameObject.name == buttonName)
            {
                return btn;
            }
        }

        return null;
    }

    /// <summary>
    /// Show GameOver UI
    /// </summary>
    public void ShowGameOver()
    {
        if (isGameOver || gameOverPanel == null) return;

        isGameOver = true;
        isPaused = false; // Ensure pause is cleared
        Time.timeScale = 0f; // Pause game time

        FindGameOverButtons();

        gameOverPanel.SetActive(true);
        SetCanvasEnabled(gameOverPanel, true);
        SetupCanvas(gameOverPanel, gameOverCanvasGroup);
        SetupGameOverButtons();
        EnsureEventSystem();
    }

    /// <summary>
    /// Setup game over buttons when menu opens
    /// </summary>
    private void SetupGameOverButtons()
    {
        SetupButton(gameOverRestartButton, OnRestartClicked);
        SetupButton(gameOverReturnToMainMenuButton, OnReturnToMainMenuClicked);

        if (gameOverCanvasGroup != null)
        {
            gameOverCanvasGroup.interactable = true;
            gameOverCanvasGroup.blocksRaycasts = true;
            gameOverCanvasGroup.alpha = 1f;
        }
    }

    /// <summary>
    /// Hide GameOver UI
    /// </summary>
    public void HideGameOver()
    {
        isGameOver = false;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
            SetCanvasEnabled(gameOverPanel, false);
        }
    }

    /// <summary>
    /// Hide game over panel and ensure it's disabled
    /// </summary>
    private void HideGameOverPanel()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
            isGameOver = false;
            SetCanvasEnabled(gameOverPanel, false);
        }
    }

    #endregion

    #region Shared Logic

    /// <summary>
    /// Reset all game states to default values
    /// </summary>
    private void ResetGameStates()
    {
        isGameOver = false;
        isPaused = false;
        Time.timeScale = 1f;
        
        if (PData.Instance != null)
        {
            PData.Instance.canOperate = true;
        }
    }

    /// <summary>
    /// Handle button clicks manually when EventSystem may not work properly
    /// </summary>
    private void HandleButtonClick(Button restartButton, Button returnToMainMenuButton, Button resumeButton)
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null) return;

        PointerEventData pointerData = new PointerEventData(eventSystem)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (var result in results)
        {
            if (IsButtonClicked(result.gameObject, restartButton))
            {
                OnRestartClicked();
                return;
            }
            
            if (IsButtonClicked(result.gameObject, returnToMainMenuButton))
            {
                OnReturnToMainMenuClicked();
                return;
            }
            
            if (resumeButton != null && IsButtonClicked(result.gameObject, resumeButton))
            {
                OnResumeClicked();
                return;
            }
        }
    }

    /// <summary>
    /// Check if the clicked GameObject is the button or its child
    /// </summary>
    private bool IsButtonClicked(GameObject clickedObject, Button button)
    {
        return button != null && 
               (clickedObject == button.gameObject || 
                clickedObject.transform.IsChildOf(button.transform));
    }

    /// <summary>
    /// Setup Canvas for UI panel
    /// </summary>
    private void SetupCanvas(GameObject panel, CanvasGroup canvasGroup)
    {
        Canvas canvas = panel.GetComponentInChildren<Canvas>();
        if (canvas == null)
        {
            canvas = panel.GetComponentInParent<Canvas>();
        }

        if (canvas == null)
        {
            Debug.LogError("MenuManager: Canvas not found!");
            return;
        }

        canvas.sortingOrder = 100;

        if (canvas.renderMode == RenderMode.ScreenSpaceCamera && canvas.worldCamera == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                canvas.worldCamera = mainCamera;
            }
            else
            {
                Debug.LogError("MenuManager: Main camera not found!");
            }
        }

        GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
        if (raycaster == null)
        {
            canvas.gameObject.AddComponent<GraphicRaycaster>();
        }
        else
        {
            raycaster.enabled = true;
        }

        canvas.enabled = true;
    }

    /// <summary>
    /// Enable or disable Canvas component in panel
    /// </summary>
    private void SetCanvasEnabled(GameObject panel, bool enabled)
    {
        if (panel == null) return;
        
        Canvas canvas = panel.GetComponentInChildren<Canvas>();
        if (canvas != null)
        {
            canvas.enabled = enabled;
        }
    }

    /// <summary>
    /// Setup a single button
    /// </summary>
    private void SetupButton(Button button, UnityEngine.Events.UnityAction onClickAction)
    {
        if (button == null)
        {
            Debug.LogWarning("MenuManager: Button is null in SetupButton!");
            return;
        }

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
        EventSystem eventSystem = EventSystem.current ?? FindObjectOfType<EventSystem>();
        
        if (eventSystem == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<StandaloneInputModule>();
            eventSystem = eventSystemObj.GetComponent<EventSystem>();
        }
        else
        {
            EventSystem.current = eventSystem;
        }

        eventSystem.enabled = true;

        StandaloneInputModule inputModule = eventSystem.GetComponent<StandaloneInputModule>();
        if (inputModule == null)
        {
            eventSystem.gameObject.AddComponent<StandaloneInputModule>();
        }
        else
        {
            inputModule.enabled = true;
        }
    }

    /// <summary>
    /// Reset states and hide panels before scene transition
    /// </summary>
    private void PrepareForSceneTransition()
    {
        ResetGameStates();
        HideGameOverPanel();
        
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
        PrepareForSceneTransition();
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneLoader.Instance.LoadScene(currentSceneName);
    }

    /// <summary>
    /// Return to main menu
    /// </summary>
    public void OnReturnToMainMenuClicked()
    {
        PrepareForSceneTransition();
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
        Time.timeScale = 1f;
    }

    #endregion
}
