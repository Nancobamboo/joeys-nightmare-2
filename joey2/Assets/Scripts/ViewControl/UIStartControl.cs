using System.Collections;
using System.Collections.Generic;
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
	private const float HoverScale = 1.1f;
	private const float AnimationDuration = 0.2f;
	private const float ClickDelay = 0.3f;
	private Dictionary<Button, Vector3> m_ButtonOriginalScales = new Dictionary<Button, Vector3>();
	private Dictionary<Button, Coroutine> m_ButtonCoroutines = new Dictionary<Button, Coroutine>();
	private bool m_IsProcessingClick = false;

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

		SetupButtonHoverEffectByTrigger(m_View.BtnEnv, m_View.EnvTrigger);
		SetupButtonHoverEffectByTrigger(m_View.BtnGuide, m_View.GuideTrigger);
		SetupButtonHoverEffectByTrigger(m_View.BtnOver, m_View.OverTrigger);
	}

	private void SetupButtonHoverEffectByTrigger(Button button, EventTriggerListener trigger)
	{
		if (button == null || trigger == null) return;

		m_ButtonOriginalScales[button] = button.transform.localScale;

		trigger.onEnter = (go, eventData) => OnButtonPointerEnter(button);
		trigger.onExit = (go, eventData) => OnButtonPointerExit(button);
	}

	private void OnButtonPointerEnter(Button button)
	{
		if (button == null || !m_ButtonOriginalScales.ContainsKey(button)) return;

		if (m_ButtonCoroutines.ContainsKey(button) && m_ButtonCoroutines[button] != null)
		{
			StopCoroutine(m_ButtonCoroutines[button]);
		}

		Vector3 targetScale = m_ButtonOriginalScales[button] * HoverScale;
		Coroutine coroutine = StartCoroutine(ScaleButtonCoroutine(button, targetScale, AnimationDuration));
		m_ButtonCoroutines[button] = coroutine;
	}

	private void OnButtonPointerExit(Button button)
	{
		if (button == null || !m_ButtonOriginalScales.ContainsKey(button)) return;

		if (m_ButtonCoroutines.ContainsKey(button) && m_ButtonCoroutines[button] != null)
		{
			StopCoroutine(m_ButtonCoroutines[button]);
		}

		Vector3 originalScale = m_ButtonOriginalScales[button];
		Coroutine coroutine = StartCoroutine(ScaleButtonCoroutine(button, originalScale, AnimationDuration));
		m_ButtonCoroutines[button] = coroutine;
	}

	private IEnumerator ScaleButtonCoroutine(Button button, Vector3 targetScale, float duration)
	{
		if (button == null) yield break;

		Vector3 startScale = button.transform.localScale;
		float elapsed = 0f;

		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			float t = elapsed / duration;
			t = t * t * (3f - 2f * t);
			button.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
			yield return null;
		}

		button.transform.localScale = targetScale;
	}

	private void OnBtnRoguelikeClick()
	{
		if (m_IsProcessingClick) return;
		PlayButtonClickEffect(m_View.BtnEnv, () =>
		{
			ClearPlayerData();
			SceneLoader.Instance.LoadScene(ESceneName.BattleEnv.ToString());
		});
	}

	private void OnBtnGuideClick()
	{
		if (m_IsProcessingClick) return;
		PlayButtonClickEffect(m_View.BtnGuide, () =>
		{
			ClearPlayerData();
			SceneLoader.Instance.LoadScene(ESceneName.BattleGuide.ToString());
		});
	}

	private void OnBtnOverClick()
	{
		if (m_IsProcessingClick) return;
		PlayButtonClickEffect(m_View.BtnOver, () =>
		{
			Application.Quit();

#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;

#endif
		});
	}

	private void PlayButtonClickEffect(Button button, System.Action onComplete)
	{
		if (button == null || !m_ButtonOriginalScales.ContainsKey(button)) return;

		m_IsProcessingClick = true;

		if (m_ButtonCoroutines.ContainsKey(button) && m_ButtonCoroutines[button] != null)
		{
			StopCoroutine(m_ButtonCoroutines[button]);
		}

		Vector3 originalScale = m_ButtonOriginalScales[button];
		Coroutine coroutine = StartCoroutine(ButtonClickEffectCoroutine(button, originalScale, onComplete));
		m_ButtonCoroutines[button] = coroutine;
	}

	private IEnumerator ButtonClickEffectCoroutine(Button button, Vector3 originalScale, System.Action onComplete)
	{
		if (button == null) yield break;

		Vector3 scale1 = originalScale * 0.9f;
		Vector3 scale2 = originalScale * 1.15f;

		yield return StartCoroutine(ScaleButtonCoroutine(button, scale1, 0.1f));
		yield return StartCoroutine(ScaleButtonCoroutine(button, scale2, 0.15f));
		yield return StartCoroutine(ScaleButtonCoroutine(button, originalScale, 0.15f));

		yield return StartCoroutine(DelayExecuteActionCoroutine(onComplete));
	}

	private IEnumerator DelayExecuteActionCoroutine(System.Action action)
	{
		yield return new WaitForSeconds(ClickDelay);
		m_IsProcessingClick = false;
		action?.Invoke();
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

		foreach (var kvp in m_ButtonCoroutines)
		{
			if (kvp.Value != null)
			{
				StopCoroutine(kvp.Value);
			}
		}
		m_ButtonCoroutines.Clear();
		m_ButtonOriginalScales.Clear();
	}
}