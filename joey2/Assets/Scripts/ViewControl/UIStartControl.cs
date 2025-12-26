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
		m_View.BtnNewGame.onClick.AddListener(OnBtnNewGameClick);
		m_View.BtnGuide.onClick.AddListener(OnBtnGuideClick);
		m_View.BtnOver.onClick.AddListener(OnBtnOverClick);
		m_View.BtnContinue.onClick.AddListener(OnBtnContinueClick);
		m_View.ToggleScreen.onValueChanged.AddListener(OnToggleScreenValueChanged);
		m_View.ToggleScreen.isOn = isFullScreen;

		SetupButtonHoverEffect(m_View.BtnNewGame, m_View.EnvTrigger);
		SetupButtonHoverEffect(m_View.BtnGuide, m_View.GuideTrigger);
		SetupButtonHoverEffect(m_View.BtnOver, m_View.OverTrigger);
		SetupButtonHoverEffect(m_View.BtnContinue, m_View.TriggerAchieve);

		ApplyScreenResolution(isFullScreen);
		CheckContinueButtonState();
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

	private void OnBtnNewGameClick()
	{
		var ctrl = Asset.OpenUI<UILobbyControl>();
		ctrl.SetData(true, true);
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

	void OnBtnContinueClick()
	{
		var ctrl = Asset.OpenUI<UILobbyControl>();
		ctrl.SetData(false, true);
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
			// 默认窗口模式分辨率调大一些（原来是 800x450）
			const int targetWidth = 1280;
			const int targetHeight = 720;

			// 避免在小屏设备上设置超过屏幕的窗口尺寸
			int maxWidth = Screen.currentResolution.width;
			int maxHeight = Screen.currentResolution.height;
			float scale = Mathf.Min(1f, maxWidth / (float)targetWidth, maxHeight / (float)targetHeight);

			int width = Mathf.RoundToInt(targetWidth * scale);
			int height = Mathf.RoundToInt(targetHeight * scale);

			// 兜底：至少保证一个合理的最小值，同时保持 16:9
			width = Mathf.Max(width, 800);
			height = Mathf.RoundToInt(width * 9f / 16f);

			// 偶数更稳（部分平台/后处理对奇数尺寸不友好）
			if ((width & 1) == 1) width -= 1;
			if ((height & 1) == 1) height -= 1;

			Screen.SetResolution(width, height, FullScreenMode.Windowed);
		}
	}

	private void ClearPlayerData()
	{
		DataSystem.Instance.ResetDataJoeyPlayer();
	}

	private void CheckContinueButtonState()
	{
		DataJoeyPlayer playerData = DataSystem.Instance.GetDataJoeyPlayer();
		bool hasSaveData = playerData.EnvCardPool != null && playerData.EnvCardPool.Count > 0;
		m_View.BtnContinue.interactable = hasSaveData;
		m_View.TxtContinue.color = hasSaveData ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.5f);
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