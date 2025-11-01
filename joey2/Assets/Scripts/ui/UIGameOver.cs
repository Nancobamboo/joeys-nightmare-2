using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// <summary>
/// GameOver UI manager
/// Shows when player dies, with restart and return to main menu options
/// </summary>
public class UIGameOver : MonoSingleton<UIGameOver>
{
    [Header("UI References")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button returnToMainMenuButton;

    private CanvasGroup canvasGroup;
    private bool isGameOver = false;
    private const string RESTART_BUTTON_NAME = "Restart";
    private const string RETURN_MAIN_MENU_BUTTON_NAME = "ReturnToMainMenu";

    private void Awake()
    {
        InitializeGameOverPanel();
    }

    private void Start()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (isGameOver && Input.GetMouseButtonDown(0))
        {
            HandleButtonClick();
        }
    }

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
                Debug.LogError("UIGameOver: Prefab not found in Resources/prefabs/UIGameOver");
                return;
            }
        }

        // Initialize CanvasGroup
        canvasGroup = gameOverPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameOverPanel.AddComponent<CanvasGroup>();
        }

        // Find buttons
        FindButtons();
    }

    /// <summary>
    /// Find buttons from the panel hierarchy
    /// </summary>
    private void FindButtons()
    {
        if (gameOverPanel == null) return;

        Transform canvasTransform = GetCanvasTransform();
        if (canvasTransform == null) return;

        if (restartButton == null)
        {
            restartButton = FindButton(canvasTransform, RESTART_BUTTON_NAME);
        }

        if (returnToMainMenuButton == null)
        {
            returnToMainMenuButton = FindButton(canvasTransform, RETURN_MAIN_MENU_BUTTON_NAME);
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

        // Fallback: search all children
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

        // Ensure buttons are found
        FindButtons();

        gameOverPanel.SetActive(true);
        SetupCanvas();
        SetupButtons();
        EnsureEventSystem();
    }

    /// <summary>
    /// Handle button clicks manually when EventSystem may not work properly
    /// </summary>
    private void HandleButtonClick()
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
    /// Hide GameOver UI
    /// </summary>
    public void HideGameOver()
    {
        isGameOver = false;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Setup Canvas for GameOver UI
    /// </summary>
    private void SetupCanvas()
    {
        // Canvas is a child of gameOverPanel, not parent
        Canvas canvas = gameOverPanel.GetComponentInChildren<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("UIGameOver: Canvas not found in children hierarchy!");
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
                Debug.LogError("UIGameOver: Main camera not found!");
            }
        }

        // Ensure GraphicRaycaster exists
        UnityEngine.UI.GraphicRaycaster raycaster = canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>();
        if (raycaster == null)
        {
            canvas.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }
        else
        {
            raycaster.enabled = true;
        }
        
        // Ensure Canvas is enabled
        canvas.enabled = true;
    }

    /// <summary>
    /// Setup buttons when menu opens
    /// </summary>
    private void SetupButtons()
    {
        SetupButton(restartButton, OnRestartClicked);
        SetupButton(returnToMainMenuButton, OnReturnToMainMenuClicked);

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
        if (button == null)
        {
            Debug.LogWarning("UIGameOver: Button is null in SetupButton!");
            return;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(onClickAction);
        button.interactable = true;
        button.enabled = true;
        button.gameObject.SetActive(true);
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
    /// Restart current level
    /// </summary>
    public void OnRestartClicked()
    {
        if (restartButton != null)
        {
            restartButton.interactable = false;
        }

        Time.timeScale = 1f;
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        SceneLoader.Instance.LoadScene(currentSceneName);
    }

    /// <summary>
    /// Return to main menu
    /// </summary>
    public void OnReturnToMainMenuClicked()
    {
        if (returnToMainMenuButton != null)
        {
            returnToMainMenuButton.interactable = false;
        }

        Time.timeScale = 1f;
        SceneLoader.Instance.LoadScene("Start");
    }

    private void OnDestroy()
    {
        // Ensure time scale is reset when menu is destroyed
        Time.timeScale = 1f;
    }
}

