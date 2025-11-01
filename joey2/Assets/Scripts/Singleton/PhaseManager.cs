using System.Collections.Generic;
using System.IO;
using UnityEngine;

public enum GamePhase
{
	Default,battleStart, playerStart, playerAction, playerEnd, enemyStart, enemyAction, enemyEnd, battleEnd
}

public class PhaseManager : MonoSingleton<PhaseManager>
{
    public GamePhase oldGamePhase { get; set; } = GamePhase.Default;
	public GamePhase gamePhase { get; set; } = GamePhase.Default;

    void OnEnable()
    {
        GameEvents.OnGamePhaseChanged += OnGamePhaseChanged;
    }
    void OnDisable()
    {
        GameEvents.OnGamePhaseChanged -= OnGamePhaseChanged;
    }


	public void SetGamePhase(GamePhase gamePhase)
	{
		oldGamePhase = this.gamePhase;
		this.gamePhase = gamePhase;
		GameEvents.RaiseGamePhaseChanged(oldGamePhase:oldGamePhase, newGamePhase:gamePhase);
		Debug.Log($"GamePhase changed: {oldGamePhase} -> {gamePhase}");
	}

    public void OnGamePhaseChanged(GamePhase oldGamePhase, GamePhase gamePhase)
    {
        Debug.Log($"OnGamePhaseChanged: {oldGamePhase} -> {gamePhase}");
        switch (gamePhase)
        {
            case GamePhase.battleStart:
                BattleStart();
                break;
            case GamePhase.playerStart:
                PlayerStart();
                break;
            case GamePhase.playerAction:
                PlayerAction();
                break;
            case GamePhase.playerEnd:
                PlayerEnd();
                break;
            case GamePhase.enemyStart:
                EnemyStart();
                break;
            case GamePhase.enemyAction:
                EnemyAction();
                break;
            case GamePhase.enemyEnd:
                EnemyEnd();
                break;
            case GamePhase.battleEnd:
                BattleEnd();
                break;
            case GamePhase.Default:
                Default();
                break;
            default:
                Debug.LogError("OnGamePhaseChanged: 未知游戏阶段");
                break;
        }
    }

    public void Default()
    {
        Debug.Log("Default");
    }
    public void BattleStart()
    {
        Debug.Log("BattleStart");
        BattleManager.Instance.GameStart();
        SetGamePhase(GamePhase.playerStart);
    }
    public void PlayerStart()
    {
        Debug.Log("PlayerStart");
        SetGamePhase(GamePhase.playerAction);
    }
    public void PlayerAction()
    {
        Debug.Log("PlayerAction");
    }
    public void PlayerEnd()
    {
        Debug.Log("PlayerEnd");
        SetGamePhase(GamePhase.enemyStart);
    }
    public void EnemyStart()
    {
        Debug.Log("EnemyStart");
        SetGamePhase(GamePhase.enemyAction);
    }
    public void EnemyAction()
    {
        Debug.Log("EnemyAction");
        SetGamePhase(GamePhase.enemyEnd);
    }
    public void EnemyEnd()
    {
        Debug.Log("EnemyEnd");
        SetGamePhase(GamePhase.battleEnd);
    }

    public void BattleEnd()
    {
        Debug.Log("BattleEnd");
        
        // Check if player is dead
        if (PData.Instance.playerHealth <= 0)
        {
            // Show GameOver UI
            if (MenuManager.Instance != null)
            {
                MenuManager.Instance.ShowGameOver();
            }
            else
            {
                Debug.LogError("PhaseManager: MenuManager.Instance is null! Cannot show GameOver UI.");
            }
        }
        
        SetGamePhase(GamePhase.Default);
    }


}