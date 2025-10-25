using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum GamePhase
{
	battleStart, playerStart, playerAction, playerEnd, enemyStart, enemyAction, enemyEnd, battleEnd
}

public class BattleManager : MonoBehaviour
{
	public static BattleManager Instance;
    public int level =1 ;// 关卡等级
    public GamePhase gamePhase = GamePhase.battleStart;

    public Transform envPanel1;
    public Transform envPanel2;
    public Transform envPanel3;
    public Transform envPanel4;
    public Transform envPanel5;
    public Transform attackPanel;
    public Transform defencePanel;
    public Transform skillPanel;
    public Transform itemPanel;


	void Awake()
	{
		Instance = this;
	}

	void Start()
	{
		// 初始化数据（如需要从 GData 抽卡生成怪物/技能/道具等）
		GameStart();
	}

    public void GameStart()
    {
        List<List<string>> cardListEnv = CardDraw.Instance.DrawCardEnv(level);
        
    }
}