using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public static class GameEvents
{
    // 游戏阶段改变
    public static event Action<GamePhase, GamePhase> OnGamePhaseChanged;
    public static void RaiseGamePhaseChanged(GamePhase oldGamePhase, GamePhase newGamePhase)
    {
        OnGamePhaseChanged?.Invoke(oldGamePhase, newGamePhase);
    }

    // 你也可以改成 Action<string, CardState> 只传 id 和状态
    public static event Action<GameObject> OnCardClicked;
    
    public static void RaiseCardClicked(GameObject cardGameObject)
    {
        OnCardClicked?.Invoke(cardGameObject);
    }

    // 心数改变
    public static event Action<int> OnHPChanged;
    public static void RaiseHPChanged(int hp)
    {
        OnHPChanged?.Invoke(hp);
    }

    // 攻击改变
    public static event Action<int> OnAttackChanged;
    public static void RaiseAttackChanged(int attack)
    {
        OnAttackChanged?.Invoke(attack);
    }

    // 防御改变
    public static event Action<int> OnDefenceChanged;
    public static void RaiseDefenceChanged(int defence)
    {
        OnDefenceChanged?.Invoke(defence);
    }

    // damage complete
    public static event Action<GameObject,bool> OnDamageComplete;
    public static void RaiseDamageComplete(GameObject enemy,bool monsterAttack=false)
    {
        OnDamageComplete?.Invoke(enemy,monsterAttack);
    }
}