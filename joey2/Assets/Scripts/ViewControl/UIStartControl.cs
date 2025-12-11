using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
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
	private float m_ShakeTimer = 0f;
	private const float ShakeInterval = 2f;
	private const float HoverScale = 1.1f;
	private const float AnimationDuration = 0.2f;
	private const float ClickDelay = 0.3f;
	private Dictionary<Button, Vector3> m_ButtonOriginalScales = new Dictionary<Button, Vector3>();
	private Dictionary<Button, Tween> m_ButtonTweens = new Dictionary<Button, Tween>();
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

		if (m_ButtonTweens.ContainsKey(button) && m_ButtonTweens[button] != null)
		{
			m_ButtonTweens[button].Kill();
		}

		Vector3 targetScale = m_ButtonOriginalScales[button] * HoverScale;
		Tween tween = button.transform.DOScale(targetScale, AnimationDuration)
			.SetEase(Ease.OutQuad);

		m_ButtonTweens[button] = tween;
	}

	private void OnButtonPointerExit(Button button)
	{
		if (button == null || !m_ButtonOriginalScales.ContainsKey(button)) return;

		if (m_ButtonTweens.ContainsKey(button) && m_ButtonTweens[button] != null)
		{
			m_ButtonTweens[button].Kill();
		}

		Vector3 originalScale = m_ButtonOriginalScales[button];
		Tween tween = button.transform.DOScale(originalScale, AnimationDuration)
			.SetEase(Ease.OutQuad);

		m_ButtonTweens[button] = tween;
	}

	private void Update()
	{
		if (DataSystem.Instance.isFinishGame)
		{
			m_ShakeTimer += Time.deltaTime;
			if (m_ShakeTimer >= ShakeInterval)
			{
				m_ShakeTimer = 0f;
				ShakeTransform().Forget();
			}
		}
	}

	private async UniTaskVoid ShakeTransform()
	{
		RectTransform rectTransform = transform as RectTransform;
		Vector2 originalPosition = rectTransform.anchoredPosition;
		float duration = 0.2f;
		float intensity = 20f;
		float elapsed = 0f;

		while (elapsed < duration)
		{
			float progress = elapsed / duration;
			float currentIntensity = intensity * (1f - progress);
			Vector2 randomOffset = Random.insideUnitCircle * currentIntensity;
			rectTransform.anchoredPosition = originalPosition + randomOffset;

			elapsed += Time.deltaTime;
			await UniTask.Yield();
		}

		rectTransform.anchoredPosition = originalPosition;
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

		if (m_ButtonTweens.ContainsKey(button) && m_ButtonTweens[button] != null)
		{
			m_ButtonTweens[button].Kill();
		}

		Vector3 originalScale = m_ButtonOriginalScales[button];

		Sequence clickSequence = DOTween.Sequence();

		clickSequence.Append(button.transform.DOScale(originalScale * 0.9f, 0.1f)
			.SetEase(Ease.OutQuad));

		clickSequence.Append(button.transform.DOScale(originalScale * 1.15f, 0.15f)
			.SetEase(Ease.OutBack));

		clickSequence.Append(button.transform.DOScale(originalScale, 0.15f)
			.SetEase(Ease.InQuad));

		clickSequence.AppendCallback(() =>
		{
			DelayExecuteAction(onComplete).Forget();
		});

		m_ButtonTweens[button] = clickSequence;
	}

	private async UniTaskVoid DelayExecuteAction(System.Action action)
	{
		await UniTask.Delay(System.TimeSpan.FromSeconds(ClickDelay));
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

		List<Tween> tweenList = new List<Tween>(m_ButtonTweens.Values);
		for (int i = 0; i < tweenList.Count; i++)
		{
			Tween tween = tweenList[i];
			if (tween != null && tween.IsActive())
			{
				tween.Kill();
			}
		}
		m_ButtonTweens.Clear();
		m_ButtonOriginalScales.Clear();
	}
}