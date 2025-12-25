using Cysharp.Threading.Tasks;
using UnityEngine;

public class UILobbyControl : YViewControl
{
	private UILobbyView m_View;
	private UIShopControl m_ShopControl;
	private UIAchievementControl m_AchievementControl;
	private UICardProgressControl m_CardProgressControl;
	private UIGrowthControl m_GrowthControl;

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
		m_View.BtnHardGame.onClick.AddListener(OnBtnHardGameClick);
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

	void OnBtnHardGameClick()
	{
		DataSystem.Instance.IsHardGame = true;
		DataSystem.Instance.ResetDataJoeyPlayer();
		SceneLoader.Instance.LoadScene(ESceneName.BattleEnv.ToString());
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
		int next = data.GetNext(toRight);
		data.Current = next;
		data.Normalize();
		DataSystem.Instance.SaveDataDifficulty();
		RefreshDifficultyUI();
	}

	private void RefreshDifficultyUI()
	{
		if (m_View == null) return;
		if (m_View.TextDiff != null)
		{
			int cur = DataSystem.Instance.GetCurrentDifficulty();
			m_View.TextDiff.text = $"难度{cur}";
		}

		// 只有一个难度解锁时，左右按钮禁用（避免误导）
		int maxUnlocked = DataSystem.Instance.GetMaxUnlockedDifficulty();
		bool canSwitch = maxUnlocked > 1;
		if (m_View.BtnLeft != null) m_View.BtnLeft.interactable = canSwitch;
		if (m_View.BtnRight != null) m_View.BtnRight.interactable = canSwitch;
	}

	public void SetData(bool isUIStartEnter = false)
	{
		m_View.BtnSkip.gameObject.SetActive(isUIStartEnter);
		bool isDebug = JoeyGameControl.Instance != null && JoeyGameControl.Instance.GameMode == EGameMode.Debug;
		m_View.BtnGame.gameObject.SetActive(isDebug);
		m_View.BtnHardGame.gameObject.SetActive(!isDebug);
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