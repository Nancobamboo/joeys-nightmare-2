using UnityEngine;
using UnityEngine.UI;

public enum ESceneName
{
	AStar,
	Battle,
	BattleEnv,
	BattleGuide,
	BattleTest,
	BuildNew,
	main,
	main2,
	SceneTest,
	Start,
	Upper
}

public class UIStartControl : YViewControl
{
	private UIStartView m_View;
	private const string PREF_KEY_FULLSCREEN = "IsFullScreen";

	public static EResType GetResType()
	{
		return EResType.UIStart;
	}

	protected override void OnInit()
	{
		base.OnInit();

		bool isFullScreen = PlayerPrefs.GetInt(PREF_KEY_FULLSCREEN, 1) == 1;

		m_View = CreateView<UIStartView>();
		m_View.BtnEnv.onClick.AddListener(OnBtnRoguelikeClick);
		m_View.BtnGuide.onClick.AddListener(OnBtnGuideClick);
		m_View.BtnOver.onClick.AddListener(OnBtnOverClick);
		m_View.BtnAchievement.onClick.AddListener(OnBtnAchievementClick);
		m_View.ToggleScreen.onValueChanged.AddListener(OnToggleScreenValueChanged);
		m_View.ToggleScreen.isOn = isFullScreen;

		SetupButtonHoverEffect(m_View.BtnEnv, m_View.EnvTrigger);
		SetupButtonHoverEffect(m_View.BtnGuide, m_View.GuideTrigger);
		SetupButtonHoverEffect(m_View.BtnOver, m_View.OverTrigger);
		SetupButtonHoverEffect(m_View.BtnAchievement, m_View.TriggerAchieve);

		ApplyScreenResolution(isFullScreen);
	}

	private void SetupButtonHoverEffect(Button button, EventTriggerListener trigger)
	{
		if (button == null || trigger == null) return;

		trigger.onEnter = (go, eventData) => OnButtonPointerEnter(button);
		trigger.onExit = (go, eventData) => OnButtonPointerExit(button);
	}

	private void OnButtonPointerEnter(Button button)
	{
		if (button == null) return;
		button.transform.localScale = Vector3.one * 1.1f;
	}

	private void OnButtonPointerExit(Button button)
	{
		if (button == null) return;
		button.transform.localScale = Vector3.one;
	}

	private void OnBtnRoguelikeClick()
	{
		// Open lobby instead of directly starting the game
		var ctrl = Asset.OpenUI<UILobbyControl>();
		ctrl.SetData(false); // false表示不是从成就界面进入
	}

	private void OnBtnGuideClick()
	{
		ClearPlayerData();
		SceneLoader.Instance.LoadScene(ESceneName.BattleGuide.ToString());
	}

	private void OnBtnOverClick()
	{
		Application.Quit();

#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#endif
	}

	void OnBtnAchievementClick()
	{
		var ctrl = Asset.OpenUI<UILobbyControl>();
		ctrl.SetData(true);
	}

	void OnToggleScreenValueChanged(bool isOn)
	{
		PlayerPrefs.SetInt(PREF_KEY_FULLSCREEN, isOn ? 1 : 0);
		PlayerPrefs.Save();
		ApplyScreenResolution(isOn);
	}

	void ApplyScreenResolution(bool isFullScreen)
	{
		if (isFullScreen)
		{
			Screen.SetResolution(1920, 1080, FullScreenMode.FullScreenWindow);
		}
		else
		{
			Screen.SetResolution(800, 450, FullScreenMode.Windowed);
		}
	}

	private void ClearPlayerData()
	{
		DataSystem.Instance.ResetDataJoeyPlayer();
	}

	public void SetData()
	{
		;
	}

	protected override void OnReturn()
	{
		base.OnReturn();
	}
}