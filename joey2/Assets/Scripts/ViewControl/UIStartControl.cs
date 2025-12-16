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

	public static EResType GetResType()
	{
		return EResType.UIStart;
	}

	protected override void OnInit()
	{
		base.OnInit();
		Screen.SetResolution(1920, 1080, FullScreenMode.FullScreenWindow);
		m_View = CreateView<UIStartView>();
		m_View.BtnEnv.onClick.AddListener(OnBtnRoguelikeClick);
		m_View.BtnGuide.onClick.AddListener(OnBtnGuideClick);
		m_View.BtnOver.onClick.AddListener(OnBtnOverClick);
		m_View.BtnAchievement.onClick.AddListener(OnBtnAchievementClick);

		SetupButtonHoverEffect(m_View.BtnEnv, m_View.EnvTrigger);
		SetupButtonHoverEffect(m_View.BtnGuide, m_View.GuideTrigger);
		SetupButtonHoverEffect(m_View.BtnOver, m_View.OverTrigger);
		SetupButtonHoverEffect(m_View.BtnAchievement, m_View.TriggerAchieve);
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
		DataSystem.Instance.IsHardGame = false;
		ClearPlayerData();
		SceneLoader.Instance.LoadScene(ESceneName.BattleEnv.ToString());
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
		ctrl.SetData();
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