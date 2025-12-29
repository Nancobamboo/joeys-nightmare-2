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
	private UIDescExtControl m_DifficultyDescExtControl;
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

		SetupDifficultyTooltip();
		RefreshContinueButtonState();
		RefreshTips();
	}

	void Update()
	{
		RefreshTips();

		DebugHotkeyConfig debugCfg = DebugHotkeyConfig.Get();
		if (debugCfg.enableDebugHotkeys && debugCfg.enableF9UnlockAllDifficulty && Input.GetKeyDown(KeyCode.F9))
		{
			DataDifficulty diffData = DataSystem.Instance.GetDataDifficulty();
			diffData.UnlockUpTo(12);
			diffData.Current = 1;
			DataSystem.Instance.SaveDataDifficulty();
			RefreshDifficultyUI();
			Debug.Log("[DEBUG] All difficulty levels unlocked (1-12)");
		}

		// F10：解锁全部 Joey 成长（growth.csv）
		if (debugCfg.enableDebugHotkeys && debugCfg.enableF10UnlockAllGrowth && Input.GetKeyDown(KeyCode.F10))
		{
			DataGrowth growth = DataSystem.Instance.GetDataGrowth();

			GData.Instance.LoadGrowthInfo();
			foreach (var kv in GData.Instance.GrowthInfoDict)
			{
				GrowthInfo info = kv.Value;
				if (info == null) continue;
				growth.Unlock(info.id);
			}

			DataSystem.Instance.SaveDataGrowth();
			DataSystem.Instance.ApplyGrowthUnlocks(); // 立刻应用到卡池/遗物池/局外加成等

			// 如果成长界面已打开，刷新一次
			if (m_GrowthControl != null && m_GrowthControl.gameObject.activeInHierarchy)
			{
				m_GrowthControl.SetData();
			}
			RefreshTips();
			Debug.Log("[DEBUG] All growth nodes unlocked");
		}
	}

	private void RefreshTips()
	{
		m_View.TipCard.SetActive(DataSystem.Instance.IsNewCardUnlock);
		m_View.TipGrowth.SetActive(DataSystem.Instance.HasAffordableConnectedGrowthNode());
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
		DataSystem.Instance.IsNewCardUnlock = false;
		RefreshTips();
		if (m_CardProgressControl == null)
		{
			m_CardProgressControl = Asset.OpenUI<UICardProgressControl>();
		}
		m_CardProgressControl.SetData();
	}

	void OnBtnGrowthClick()
	{
		DataSystem.Instance.IsGrowthUnlock = false;
		RefreshTips();
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
		if (!m_View.BtnContinue.interactable)
		{
			return;
		}
		StartEnvGame(false);
	}

	private void RefreshContinueButtonState()
	{
		DataJoeyPlayer playerData = DataSystem.Instance.GetDataJoeyPlayer();
		bool hasSaveData = playerData.EnvCardPool != null && playerData.EnvCardPool.Count > 0;
		m_View.BtnContinue.interactable = hasSaveData;
		m_View.TxtContinue.color = hasSaveData ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.5f);
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

			m_DifficultyDescExtControl = Asset.OpenUI<UIDescExtControl>(Asset.UIRoot);
			m_DifficultyDescExtControl.SetData(tooltipText);
			RectTransform diffRect = m_View.TextDiff.transform as RectTransform;
			m_DifficultyDescExtControl.SetPositionRelativeTo(diffRect);
		}
	}

	private void OnDifficultyHoverExit(GameObject go, UnityEngine.EventSystems.BaseEventData data)
	{
		if (m_DifficultyDescExtControl != null)
		{
			m_DifficultyDescExtControl.Close();
			m_DifficultyDescExtControl = null;
		}
	}

	public void SetData(bool isNewGame, bool isUIStartEnter = false)
	{
		m_IsNewGame = isNewGame;

		DataDifficulty diffData = DataSystem.Instance.GetDataDifficulty();
		diffData.Current = diffData.MaxUnlocked;
		//DataSystem.Instance.SaveDataDifficulty();

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
		if (m_DifficultyDescExtControl != null)
		{
			m_DifficultyDescExtControl.Close();
			m_DifficultyDescExtControl = null;
		}
	}
}