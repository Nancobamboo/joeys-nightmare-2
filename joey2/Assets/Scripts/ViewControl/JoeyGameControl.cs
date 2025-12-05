using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;

public enum EGamePhase
{
	Default,
	BattleStart,
	PlayerStart,
}

public enum EGameMode
{
	Battle,
	Guide,
	Debug,
	Env,
}

public class JoeyGameControl : YViewControl
{
	private JoeyGameView m_View;
	public static JoeyGameControl Instance { get; private set; }

	private EGamePhase m_CurrentGamePhase = EGamePhase.Default;
	private EGamePhase m_LastGamePhase = EGamePhase.Default;
	private UIGamePhaseControl m_GamePhaseControl;
	private DataJoeyPlayer m_DataJoeyPlayer;
	private UIPauseControl m_PauseControl;
	private UIGameOverControl m_GameOverControl;
	private Dictionary<int, MonoBehaviourPool<Transform>> VFXPoolDict = new Dictionary<int, MonoBehaviourPool<Transform>>();
	private Dictionary<Transform, CancellationTokenSource> CancelTokenDict = new Dictionary<Transform, CancellationTokenSource>();
	private SingleDelayAction m_GlobalDelayAction = new SingleDelayAction();
	private bool m_IsLobbyEnter = false;

	private class GameStateCache
	{
		public Dictionary<int, List<string>> EnvCardDict = new Dictionary<int, List<string>>();
		public Dictionary<ECardType, List<string>> BagCardDict = new Dictionary<ECardType, List<string>>();
		public int PlayerHealth;
		public int PlayerMaxHealth;
	}

	private GameStateCache m_GameStateCache;

	public EGameMode GameMode = EGameMode.Battle;
	public string[] DebugEnvCardIds = new string[0];
	public string[] DebugBagCardIds = new string[0];
	public int DebugLevelId = 1;

	public static EResType GetResType()
	{
		return EResType.None;
	}

	protected override void OnInit()
	{
		Instance = this;
		base.OnInit();
		m_View = CreateView<JoeyGameView>();
		m_DataJoeyPlayer = DataSystem.Instance.GetDataJoeyPlayer();
		GData.Instance.LoadAll();
	}

	void Start()
	{
		if (GameMode == EGameMode.Battle && m_DataJoeyPlayer.SelfCardDict.Count == 0)
		{
			RoguelikeCharacter characterData = GData.Instance.GetRoguelikeCharacter();
			if (characterData != null)
			{
				DataSystem.Instance.InitRoguelikeCharacterData(characterData);
			}
		}

		if (GameMode == EGameMode.Env && m_DataJoeyPlayer.EnvCardPool.Count == 0)
		{
			RoguelikeCharacter characterData = GData.Instance.GetRoguelikeCharacter();
			if (characterData != null)
			{
				DataSystem.Instance.InitEnvModeCharacterData(characterData);
			}
		}

		m_GamePhaseControl = Asset.OpenUI<UIGamePhaseControl>();
		if (m_DataJoeyPlayer.currentLevel <= 0)
		{
			m_DataJoeyPlayer.currentLevel = 1;
		}
		if (GameMode == EGameMode.Env)
		{
			SetGamePhase(EGamePhase.BattleStart);
		}
		else if (GameMode == EGameMode.Guide || GameMode == EGameMode.Battle)
		{
			SetGamePhase(EGamePhase.BattleStart);
		}
		else
		{
			Asset.OpenUI<UILobbyControl>();
			SetGamePhase(EGamePhase.Default);
		}
	}

	void Update()
	{
		CheckButtonInput();

		if (m_CurrentGamePhase != m_LastGamePhase)
		{
			m_LastGamePhase = m_CurrentGamePhase;
			switch (m_CurrentGamePhase)
			{
				case EGamePhase.Default:
					Default();
					break;
				case EGamePhase.BattleStart:
					BattleStart();
					break;
				case EGamePhase.PlayerStart:
					PlayerStart();
					break;
				default:
					break;
			}
		}
	}

	void CheckButtonInput()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			if (m_GameOverControl != null && m_GameOverControl.gameObject.activeSelf)
			{
				return;
			}
			if (m_PauseControl == null)
			{
				m_PauseControl = Asset.OpenUI<UIPauseControl>();
			}
			else
			{
				bool isActive = m_PauseControl.gameObject.activeSelf;
				m_PauseControl.gameObject.SetActive(!isActive);
			}
		}
	}

	private void Default()
	{
	}

	private void BattleStart()
	{
		SetLevelData();
		SetGamePhase(EGamePhase.PlayerStart);
	}

	private void PlayerStart()
	{
	}

	public void SetGamePhase(EGamePhase gamePhase)
	{
		m_CurrentGamePhase = gamePhase;
	}

	public void SetLevelData()
	{
		StartCoroutine(SFX.PlayAudioCoroutine(audioPath: "Audio/SFX/shuffle_cards", startTime: 0f));

		int stageId = m_DataJoeyPlayer.StageId;
		m_GamePhaseControl.SetBackgroundByStageId(stageId);

		int levelId = GameMode == EGameMode.Debug ? DebugLevelId : m_DataJoeyPlayer.currentLevel;

		if (GameMode != EGameMode.Battle && GameMode != EGameMode.Env)
		{
			(int health, int maxHealth)? playerData = GData.Instance.GetTutorialPlayerData(levelId);
			if (playerData.HasValue)
			{
				m_DataJoeyPlayer.lastPlayerHealth = m_DataJoeyPlayer.playerHealth;
				m_DataJoeyPlayer.playerHealth = playerData.Value.health;
				m_DataJoeyPlayer.playerMaxHealth = playerData.Value.maxHealth;
			}
		}

		if (GameMode == EGameMode.Env)
		{
			m_GamePhaseControl.SetData();

			int envLevelId = m_DataJoeyPlayer.StageId;
			List<string> playerCardPool = new List<string>(m_DataJoeyPlayer.EnvCardPool);
			List<List<string>> envModeCardList = CardDraw.Instance.DrawCardEnvMode(envLevelId, playerCardPool);
			for (int i = 0; i < envModeCardList.Count; i++)
			{
				List<string> cardIdList = envModeCardList[i];
				m_GamePhaseControl.AddEnvCardList(cardIds: cardIdList, index: i);
			}
			SaveGameStateCache();
			return;
		}

		if (GameMode == EGameMode.Battle)
		{
			if (m_IsLobbyEnter == false)
			{
				m_GamePhaseControl.SetDataWithoutBagClear();

			}
			else
			{
				m_GamePhaseControl.SetData();
			}
		}
		else
		{
			m_GamePhaseControl.SetData();
		}

		if (m_IsLobbyEnter)
		{
			m_GamePhaseControl.AddSelfCardList();
			m_IsLobbyEnter = false;
		}

		List<List<string>> cardIdListEnv = CardDraw.Instance.DrawCardEnv(levelId);
		for (int i = 0; i < cardIdListEnv.Count; i++)
		{
			List<string> cardIdList = cardIdListEnv[i];
			m_GamePhaseControl.AddEnvCardList(cardIds: cardIdList, index: i);
		}

		if (GameMode != EGameMode.Battle)
		{
			Dictionary<string, List<string>> equipmentDeck;

			equipmentDeck = GData.Instance.GetTutorialEquipmentDeck(levelId);


			foreach (var kv in equipmentDeck)
			{
				string cardTypeStr = kv.Key;
				List<string> cardIds = kv.Value;

				ECardType cardType = (ECardType)System.Enum.Parse(typeof(ECardType), cardTypeStr);
				Debug.Log("cardType: " + cardTypeStr + " " + cardType.ToString());

				m_GamePhaseControl.AddCardList(cardType: cardType, cardIds: cardIds);
			}
		}

		if (GameMode == EGameMode.Debug)
		{
			if (DebugEnvCardIds != null && DebugEnvCardIds.Length > 0)
			{
				List<string> debugEnvCardList = new List<string>(DebugEnvCardIds);
				m_GamePhaseControl.AddEnvCardList(cardIds: debugEnvCardList, index: 0);
			}

			if (DebugBagCardIds != null && DebugBagCardIds.Length > 0)
			{
				Dictionary<ECardType, List<string>> bagCardDict = new Dictionary<ECardType, List<string>>();
				for (int i = 0; i < DebugBagCardIds.Length; i++)
				{
					string cardId = DebugBagCardIds[i];
					if (string.IsNullOrEmpty(cardId))
					{
						continue;
					}
					Card card = m_GamePhaseControl.CreateCard(cardId);
					ECardType cardType = card.GetCardType();
					if (!bagCardDict.ContainsKey(cardType))
					{
						bagCardDict[cardType] = new List<string>();
					}
					bagCardDict[cardType].Add(cardId);
				}

				foreach (var kv in bagCardDict)
				{
					m_GamePhaseControl.AddCardList(cardType: kv.Key, cardIds: kv.Value);
				}
			}
		}

		if (GameMode == EGameMode.Battle || GameMode == EGameMode.Env)
		{
			SaveGameStateCache();
		}
	}

	private void SaveGameStateCache()
	{
		if (m_GamePhaseControl == null)
		{
			return;
		}

		m_GameStateCache = new GameStateCache();
		m_GameStateCache.PlayerHealth = m_DataJoeyPlayer.playerHealth;
		m_GameStateCache.PlayerMaxHealth = m_DataJoeyPlayer.playerMaxHealth;

		Dictionary<int, List<UICardSimpleControl>> envCardDict = m_GamePhaseControl.GetEnvCardDict();
		foreach (var kvp in envCardDict)
		{
			List<string> cardIds = new List<string>();
			for (int i = 0; i < kvp.Value.Count; i++)
			{
				UICardSimpleControl cardControl = kvp.Value[i];
				if (cardControl != null && cardControl.CardData != null)
				{
					cardIds.Add(cardControl.CardData.id);
				}
			}
			m_GameStateCache.EnvCardDict[kvp.Key] = cardIds;
		}

		Dictionary<int, List<UICardSimpleControl>> bagCardDict = m_GamePhaseControl.GetBagCardDict();
		foreach (var kvp in bagCardDict)
		{
			ECardType cardType = (ECardType)kvp.Key;
			List<string> cardIds = new List<string>();
			for (int i = 0; i < kvp.Value.Count; i++)
			{
				UICardSimpleControl cardControl = kvp.Value[i];
				if (cardControl != null && cardControl.CardData != null)
				{
					cardIds.Add(cardControl.CardData.id);
				}
			}
			m_GameStateCache.BagCardDict[cardType] = cardIds;
		}
	}

	private void RestoreGameStateCache()
	{
		if (m_GameStateCache == null || m_GamePhaseControl == null)
		{
			return;
		}

		m_DataJoeyPlayer.playerHealth = m_GameStateCache.PlayerHealth;
		m_DataJoeyPlayer.playerMaxHealth = m_GameStateCache.PlayerMaxHealth;

		m_GamePhaseControl.SetData();

		foreach (var kvp in m_GameStateCache.EnvCardDict)
		{
			m_GamePhaseControl.AddEnvCardList(cardIds: kvp.Value, index: kvp.Key);
		}

		foreach (var kvp in m_GameStateCache.BagCardDict)
		{
			m_GamePhaseControl.AddCardList(cardType: kvp.Key, cardIds: kvp.Value);
		}
	}

	public async void LoadNextLevel(bool IsLobbyEnter = false)
	{
		m_IsLobbyEnter = IsLobbyEnter;
		if (GameMode == EGameMode.Battle)
		{
			DataSystem.Instance.LoadNextRoguelikeStage();
		}
		else if (GameMode == EGameMode.Env)
		{
			DataSystem.Instance.SaveDataJoeyPlayer();
		}
		else if (GameMode == EGameMode.Guide)
		{
			m_DataJoeyPlayer.currentLevel++;
			DataSystem.Instance.SaveDataJoeyPlayer();
		}
		m_GamePhaseControl.ClearCardQueue();
		await UniTask.WaitForSeconds(0.5f);


		SetGamePhase(EGamePhase.BattleStart);
	}

	public void EnterBattleStart()
	{
		if ((GameMode == EGameMode.Battle || GameMode == EGameMode.Env) && m_GameStateCache != null)
		{
			RestoreGameStateCache();
			SetGamePhase(EGamePhase.PlayerStart);
		}
		else
		{
			SetGamePhase(EGamePhase.BattleStart);
		}
	}

	public void EndGamePhase()
	{
		if (m_GamePhaseControl != null)
		{
			m_GamePhaseControl.ClearEnvCardList();
		}

		if (GameMode == EGameMode.Battle)
		{
			RoguelikeStage currentStage = GData.Instance.GetRoguelikeStage(m_DataJoeyPlayer.StageId);

			if (currentStage != null && currentStage.type == EStageType.final)
			{
				DataSystem.Instance.isFinishGame = true;
				ClearAllUniTasks();
				SceneLoader.Instance.LoadScene(ESceneName.Start.ToString());
				return;
			}
			else if (currentStage != null && currentStage.type == EStageType.boss)
			{
				if (m_GamePhaseControl != null)
				{
					m_GamePhaseControl.ClearBagCardList();
				}
				UISelectControl selectControl = Asset.OpenUI<UISelectControl>();
				selectControl.SetRelicData();
				selectControl.OnSelectComplete = () => Asset.OpenUI<UILobbyControl>();
			}
			else if (currentStage != null && currentStage.type == EStageType.elite)
			{
				if (m_GamePhaseControl != null)
				{
					m_GamePhaseControl.ClearBagCardList();
				}
				UISelectControl selectControl = Asset.OpenUI<UISelectControl>();
				selectControl.SetRelicData();
				selectControl.OnSelectComplete = () => LoadNextLevel();
			}
			else
			{
				UISelectControl selectControl = Asset.OpenUI<UISelectControl>();
				selectControl.SetData();
				selectControl.OnSelectComplete = () => LoadNextLevel();
			}

			m_DataJoeyPlayer.StageId++;
		}
		else if (GameMode == EGameMode.Env)
		{
			int envLevelId = m_DataJoeyPlayer.StageId;
			EStageType stageType = GetEnvStageType(envLevelId);

			if (stageType == EStageType.final)
			{
				DataSystem.Instance.isFinishGame = true;
				ClearAllUniTasks();
				SceneLoader.Instance.LoadScene(ESceneName.Start.ToString());
				return;
			}
			else if (stageType == EStageType.boss)
			{
				UISelectControl selectControl = Asset.OpenUI<UISelectControl>();
				selectControl.SetRelicData();
				selectControl.OnSelectComplete = () =>
				{
					var ctrl = Asset.OpenUI<UIShopSuperControl>();
					ctrl.SetData();

				};
			}
			else if (stageType == EStageType.elite)
			{
				UISelectControl selectControl = Asset.OpenUI<UISelectControl>();
				selectControl.SetRelicData();
				selectControl.OnSelectComplete = () => LoadNextLevel();
			}
			else
			{

				UISelectControl selectControl = Asset.OpenUI<UISelectControl>();
				selectControl.SetData();
				selectControl.OnSelectComplete = () => LoadNextLevel();

				m_GamePhaseControl.MoveBagCardsToCoinAndShowReward();

			}

			m_DataJoeyPlayer.StageId++;
		}
		else if (GameMode == EGameMode.Guide)
		{
			if (m_DataJoeyPlayer.currentLevel == 5)
			{
				ClearAllUniTasks();
				SceneLoader.Instance.LoadScene(ESceneName.Start.ToString());
				return;
			}

			LoadNextLevel();

		}
		else
		{
			LoadNextLevel();
		}
	}

	public void ReturnToMainMenu()
	{
		ClearAllUniTasks();
		SceneLoader.Instance.LoadScene("Start");
	}

	public void SetData()
	{
		;
	}

	private EStageType GetEnvStageType(int level)
	{
		return GData.Instance.GetEnvStageType(level);
	}

	public void PlayVFX(EVFXName vfxName, Transform parent, float delayTime)
	{
		int key = (int)vfxName;

		if (!VFXPoolDict.TryGetValue(key, out MonoBehaviourPool<Transform> pool))
		{
			string prefabPath = "VFX/" + vfxName.ToString();
			GameObject prefab = Resources.Load<GameObject>(prefabPath);
			pool = new MonoBehaviourPool<Transform>(() =>
			{
				GameObject instance = Instantiate(prefab, parent);
				instance.gameObject.name = vfxName.ToString();

				return instance.transform;
			});
			VFXPoolDict[key] = pool;
		}

		Transform vfxTransform = pool.Get();
		vfxTransform.SetParent(parent);
		vfxTransform.localPosition = Vector3.zero;
		vfxTransform.localScale = Vector3.one;

		var cts = new CancellationTokenSource();
		CancelTokenDict[vfxTransform] = cts;

		DelayHideVFX(vfxTransform, delayTime, cts, key).Forget();
	}

	public Transform GetVFX(EVFXName vfxName, Transform parent)
	{
		int key = (int)vfxName;

		if (!VFXPoolDict.TryGetValue(key, out MonoBehaviourPool<Transform> pool))
		{
			string prefabPath = "VFX/" + vfxName.ToString();
			GameObject prefab = Resources.Load<GameObject>(prefabPath);
			pool = new MonoBehaviourPool<Transform>(() =>
			{
				GameObject instance = Instantiate(prefab, parent);
				instance.gameObject.name = vfxName.ToString();
				instance.transform.localPosition = Vector3.zero;

				return instance.transform;
			});
			VFXPoolDict[key] = pool;
		}

		Transform vfxTransform = pool.Get();
		vfxTransform.SetParent(parent);
		vfxTransform.localPosition = Vector3.zero;
		vfxTransform.localScale = Vector3.one;
		vfxTransform.gameObject.SetActive(true);

		return vfxTransform;
	}

	public void PlayEnvVFX(EVFXName vfxName, int envIndex, float delayTime)
	{
		if (m_GamePhaseControl != null)
		{
			Transform effectRoot = m_GamePhaseControl.GetEffectRoot(envIndex);
			if (effectRoot != null)
			{
				PlayVFX(vfxName, effectRoot, delayTime);
			}
		}
	}

	public void ReturnVFXPool(Transform vfxTransform, int envIndex)
	{
		vfxTransform.gameObject.SetActive(false);
		Transform effectRoot = m_GamePhaseControl.GetEffectRoot(envIndex);
		vfxTransform.SetParent(effectRoot);
		vfxTransform.localPosition = Vector3.zero;
		vfxTransform.localScale = Vector3.one;
	}

	private async UniTaskVoid DelayHideVFX(Transform vfxTransform, float delayTime, CancellationTokenSource cts, int key)
	{
		await UniTask.WaitForSeconds(delayTime, cancellationToken: cts.Token);
		if (vfxTransform != null && vfxTransform.gameObject != null && VFXPoolDict.TryGetValue(key, out MonoBehaviourPool<Transform> pool))
		{
			pool.Release(vfxTransform);
		}
		if (CancelTokenDict.TryGetValue(vfxTransform, out CancellationTokenSource tokenSource))
		{
			CancelTokenDict.Remove(vfxTransform);
			tokenSource.Dispose();
		}
	}

	public void AddGlobalDelayCall(Action action, float delayTime)
	{
		m_GlobalDelayAction.AddDelayCall(action, delayTime);
	}

	public bool HasBagCard(ECardType cardType)
	{
		if (m_GamePhaseControl != null)
		{
			return m_GamePhaseControl.HasBagCard(cardType);
		}
		return false;
	}

	public bool IsPlayerHalfHealth()
	{
		return m_DataJoeyPlayer.playerHealth < m_DataJoeyPlayer.playerMaxHealth / 2;
	}

	public void ShowGameOver()
	{
		if (m_GameOverControl == null)
		{
			m_GameOverControl = Asset.OpenUI<UIGameOverControl>();
		}
		else
		{
			m_GameOverControl.gameObject.SetActive(true);
		}
	}

	public void ClearAllUniTasks()
	{
		m_GamePhaseControl.Close();
		m_GlobalDelayAction.Cancel();

		foreach (var kvp in CancelTokenDict)
		{
			var cts = kvp.Value;
			if (cts != null && !cts.IsCancellationRequested)
			{
				cts.Cancel();
				cts.Dispose();
			}
		}
		CancelTokenDict.Clear();
	}

	private void OnDestroy()
	{
		m_GlobalDelayAction.Cancel();

		foreach (KeyValuePair<Transform, CancellationTokenSource> kvp in CancelTokenDict)
		{
			CancellationTokenSource cts = kvp.Value;
			if (cts != null && !cts.IsCancellationRequested)
			{
				cts.Cancel();
				cts.Dispose();
			}
		}
		CancelTokenDict.Clear();

		foreach (KeyValuePair<int, MonoBehaviourPool<Transform>> kvp in VFXPoolDict)
		{
			MonoBehaviourPool<Transform> pool = kvp.Value;
			if (pool != null)
			{
				pool.DestroyAll();
			}
		}
		VFXPoolDict.Clear();

		m_GameStateCache = null;
		m_GamePhaseControl = null;
		m_PauseControl = null;
		m_GameOverControl = null;
		m_View = null;
		m_DataJoeyPlayer = null;

		if (Instance == this)
		{
			Instance = null;
		}
	}
}