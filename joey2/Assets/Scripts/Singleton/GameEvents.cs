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


    // damage to player complete
    public static event Action OnDamageToPlayerComplete;
    public static void RaiseDamageToPlayerComplete()
    {
        OnDamageToPlayerComplete?.Invoke();
    }


    public static event Action<GameObject,GameObject,int,bool> OnAttackPre;
    public static void RaiseAttackPre(GameObject attackerCardGO,GameObject targetCardGO,int damage,bool monsterAttack)
    {
        OnAttackPre?.Invoke(attackerCardGO,targetCardGO,damage,monsterAttack);
    }

    public static event Action<GameObject> OnAttackPreFinish;
    public static void RaiseAttackPreFinish(GameObject attackerCardGO)
    {
        OnAttackPreFinish?.Invoke(attackerCardGO);
    }

    // Attack initiated event - fired when an attack starts
    public static event Action<GameObject, GameObject> OnAttackInitiated;
    public static void RaiseAttackInitiated(GameObject attackerCardGO, GameObject targetCardGO)
    {
        OnAttackInitiated?.Invoke(attackerCardGO, targetCardGO);
    }

    public static event Action<GameObject> OnMonsterAttackPre;
    public static void RaiseMonsterAttackPre(GameObject attackerCardGO)
    {
        OnMonsterAttackPre?.Invoke(attackerCardGO);
    }

    public static event Action<GameObject> OnMonsterAttackPreFinish;
    public static void RaiseMonsterAttackPreFinish(GameObject monsterCardGO)
    {
        OnMonsterAttackPreFinish?.Invoke(monsterCardGO);
    }

    // game over event
    public static event Action OnGameOver;
    public static void RaiseGameOver()
    {
        OnGameOver?.Invoke();
    }

    // next level event
    public static event Action OnNextLevelRequested;
    public static void RaiseNextLevelRequested()
    {
        OnNextLevelRequested?.Invoke();
    }

    public static event Action<GameObject> OnCardFinished;
    public static void RaiseCardFinished(GameObject cardGO)
    {
        OnCardFinished?.Invoke(cardGO);
    }


}