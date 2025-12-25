using Cysharp.Threading.Tasks;
using UnityEngine;

public class UILobbyControl : YViewControl
{
	private UILobbyView m_View;
	private UIShopControl m_ShopControl;
	private UIAchievementControl m_AchievementControl;
	private UICardProgressControl m_CardProgressControl;
	private UIGrowthControl m_GrowthControl;
	private EventTriggerListener m_DiffTooltipTrigger;
	private GameObject m_TooltipPanel;
	bool m_IsNewGame = false;

	public static EResType GetResType()
	{
		return EResType.UILobby;
	}

	protected override void OnInit()
	{
		base.OnInit();
		m_View = CreateView<UILobbyView>();
		m_View.BtnBuild.onClick.AddListener(OnBtnBuildClick);
		//m_View.BtnShop.onClick.AddListener(OnBtnShopClick);
		m_View.BtnGame.onClick.AddListener(OnBtnGameClick);
		//m_View.BtnMerge.onClick.AddListener(OnBtnMergeClick);
		m_View.BtnCardProgress.onClick.AddListener(OnBtnCardProgressClick);
		m_View.BtnNew.onClick.AddListener(OnBtnNewClick);
		m_View.BtnContinue.onClick.AddListener(OnBtnContinueClick);
		m_View.BtnSkip.onClick.AddListener(Close);
		if (m_View.BtnLeft != null)
		{
			m_View.BtnLeft.onClick.AddListener(OnBtnDiffLeftClick);
		}
		if (m_View.BtnRight != null)
		{
			m_View.BtnRight.onClick.AddListener(OnBtnDiffRightClick);
		}
		if (m_View.BtnGrowth != null)
		{
			m_View.BtnGrowth.onClick.AddListener(OnBtnGrowthClick);
		}

		// Setup difficulty tooltip
		SetupDifficultyTooltip();

		RefreshContinueButtonState();
	}

	void Update()
	{
		// Debug: Unlock all difficulties (F9)
		if (Input.GetKeyDown(KeyCode.F9))
		{
			DataDifficulty diffData = DataSystem.Instance.GetDataDifficulty();
			diffData.UnlockUpTo(8); // Unlock all 8 difficulty levels
			diffData.Current = 1; // Reset to difficulty 1
			DataSystem.Instance.SaveDataDifficulty();
			RefreshDifficultyUI();
			Debug.Log("[DEBUG] All difficulty levels unlocked (1-8)");
		}
	}

	public void OnBtnBuildClick()
	{
		if (m_AchievementControl == null)
		{
			m_AchievementControl = Asset.OpenUI<UIAchievementControl>();
		}
		m_AchievementControl.SetData();
	}

	void OnBtnCardProgressClick()
	{
		if (m_CardProgressControl == null)
		{
			m_CardProgressControl = Asset.OpenUI<UICardProgressControl>();
		}
		m_CardProgressControl.SetData();
	}

	void OnBtnGrowthClick()
	{
		if (m_GrowthControl == null)
		{
			m_GrowthControl = Asset.OpenUI<UIGrowthControl>();
		}
		m_GrowthControl.SetData();
		m_GrowthControl.gameObject.SetActive(true);
	}

	private void StartEnvGame(bool isNewGame)
	{
		if (isNewGame)
		{
			DataSystem.Instance.ResetDataJoeyPlayer();
		}

		// Start Env mode game with current difficulty
		DataSystem.Instance.IsHardGame = false;
		SceneLoader.Instance.LoadScene(ESceneName.BattleEnv.ToString());
	}

	private void OnBtnNewClick()
	{
		StartEnvGame(true);
	}

	private void OnBtnContinueClick()
	{
		RefreshContinueButtonState();
		if (m_View != null && m_View.BtnContinue != null && !m_View.BtnContinue.interactable)
		{
			return;
		}
		StartEnvGame(false);
	}

	private void RefreshContinueButtonState()
	{
		if (m_View == null || m_View.BtnContinue == null) return;
		DataJoeyPlayer playerData = DataSystem.Instance.GetDataJoeyPlayer();
		bool hasSaveData = playerData.EnvCardPool != null && playerData.EnvCardPool.Count > 0;
		m_View.BtnContinue.interactable = hasSaveData;
	}

	void OnBtnShopClick()
	{
		bool isNew = DataSystem.isNew;
		if (JoeyTestGameControl.Instance != null)
		{
			isNew = JoeyTestGameControl.Instance.isNew;
		}

		if (m_ShopControl == null)
		{
			m_ShopControl = Asset.OpenUI<UIShopControl>();
			m_ShopControl.SetData(isNew);
		}
		else
		{
			m_ShopControl.gameObject.SetActive(true);
		}
	}

	void OnBtnGameClick()
	{
		// This is now for debug mode only - kept for backward compatibility
		if (m_ShopControl != null)
		{
			m_ShopControl.Close();
		}
		Close();
		JoeyGameControl.Instance.LoadNextLevel(true);
	}

	void OnBtnMergeClick()
	{
	}

	private void OnBtnDiffLeftClick()
	{
		ChangeDifficulty(false);
	}

	private void OnBtnDiffRightClick()
	{
		ChangeDifficulty(true);
	}

	private void ChangeDifficulty(bool toRight)
	{
		DataDifficulty data = DataSystem.Instance.GetDataDifficulty();
		int currentDiff = data.Current;
		int maxUnlocked = data.MaxUnlocked;

		// Calculate next difficulty within unlocked range (no wrapping)
		int next = toRight ? currentDiff + 1 : currentDiff - 1;

		// Clamp to unlocked range [1, maxUnlocked] without wrapping
		if (next > maxUnlocked)
		{
			next = maxUnlocked; // Stay at max
		}
		else if (next < 1)
		{
			next = 1; // Stay at min
		}

		data.Current = next;
		data.Normalize();
		DataSystem.Instance.SaveDataDifficulty();
		RefreshDifficultyUI();
	}

	private void RefreshDifficultyUI()
	{
		if (m_View == null) return;

		DataDifficulty data = DataSystem.Instance.GetDataDifficulty();
		int cur = data.Current;
		int maxUnlocked = data.MaxUnlocked;

		if (m_View.TextDiff != null)
		{
			m_View.TextDiff.text = $"难度{cur}";
		}

		// Enable/disable buttons based on current position
		if (m_View.BtnLeft != null)
		{
			m_View.BtnLeft.interactable = cur > 1; // Can go left if not at minimum
		}
		if (m_View.BtnRight != null)
		{
			m_View.BtnRight.interactable = cur < maxUnlocked; // Can go right if not at max unlocked
		}
	}

	private void SetupDifficultyTooltip()
	{
		if (m_View.TextDiff != null)
		{
			// Add EventTriggerListener if not already present
			m_DiffTooltipTrigger = m_View.TextDiff.gameObject.GetComponent<EventTriggerListener>();
			if (m_DiffTooltipTrigger == null)
			{
				m_DiffTooltipTrigger = m_View.TextDiff.gameObject.AddComponent<EventTriggerListener>();
			}

			m_DiffTooltipTrigger.onEnter = OnDifficultyHoverEnter;
			m_DiffTooltipTrigger.onExit = OnDifficultyHoverExit;
		}
	}

	private void OnDifficultyHoverEnter(GameObject go, UnityEngine.EventSystems.BaseEventData data)
	{
		int cur = DataSystem.Instance.GetCurrentDifficulty();
		DifficultyConfig config = GData.Instance.GetDifficultyConfig(cur);
		if (config != null)
		{
			string tooltipText = config.description;
			if (!string.IsNullOrEmpty(config.comment))
			{
				tooltipText += "\n" + config.comment;
			}

			// Simple debug log for now - could be enhanced with a proper UI tooltip
			Debug.Log($"[Difficulty {cur}] {tooltipText}");

			// If you want to show text on the UI, you can create a simple text display
			// For now, using debug log as specified in requirements
		}
	}

	private void OnDifficultyHoverExit(GameObject go, UnityEngine.EventSystems.BaseEventData data)
	{
		// Hide tooltip if needed
	}

	public void SetData(bool isNewGame, bool isUIStartEnter = false)
	{
        m_IsNewGame = isNewGame;

        m_View.BtnSkip.gameObject.SetActive(isUIStartEnter);
		bool isDebug = JoeyGameControl.Instance != null && JoeyGameControl.Instance.GameMode == EGameMode.Debug;
		m_View.BtnGame.gameObject.SetActive(isDebug);
		m_View.BtnNew.gameObject.SetActive(!isDebug);
		m_View.BtnContinue.gameObject.SetActive(!isDebug);
		RefreshContinueButtonState();
		RefreshDifficultyUI();
	}

	protected override void OnReturn()
	{
		base.OnReturn();
	}

	private void OnDestroy()
	{
		if (m_ShopControl != null)
		{
			m_ShopControl.Close();
		}
		if (m_AchievementControl != null)
		{
			m_AchievementControl.Close();
		}
		if (m_CardProgressControl != null)
		{
			m_CardProgressControl.Close();
		}
		if (m_GrowthControl != null)
		{
			m_GrowthControl.Close();
		}
	}
}