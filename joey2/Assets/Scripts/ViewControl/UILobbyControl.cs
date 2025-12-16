using Cysharp.Threading.Tasks;
using UnityEngine;

public class UILobbyControl : YViewControl
{
	private UILobbyView m_View;
	private UIShopControl m_ShopControl;
	private UIAchievementControl m_AchievementControl;
	private UICardProgressControl m_CardProgressControl;
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
		//m_View.BtnGame.onClick.AddListener(OnBtnGameClick);
		//m_View.BtnMerge.onClick.AddListener(OnBtnMergeClick);
		m_View.BtnCardProgress.onClick.AddListener(OnBtnCardProgressClick);
		m_View.BtnHardGame.onClick.AddListener(OnBtnHardGameClick);
		m_View.BtnSkip.onClick.AddListener(Close);
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

	public void SetData(bool isStartEnter = false)
	{
		m_View.BtnSkip.gameObject.SetActive(isStartEnter);
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
	}
}