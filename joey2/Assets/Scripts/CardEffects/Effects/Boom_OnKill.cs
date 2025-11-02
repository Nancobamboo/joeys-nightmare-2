// Scripts/CardEffects/Effects/BounceToRandomEnemy_OnDealDamage.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Boom_OnKill : ICardEffect
{
    public int baseExtra; // CSV 传入的基础额外伤害

    public Boom_OnKill(int baseExtra)
    {
        this.baseExtra = Mathf.Max(0, baseExtra);
    }
    public string Id => "Boom_OnKill";
    public bool MatchTrigger(CardTrigger trigger) => trigger == CardTrigger.OnKill;


    public IEnumerator Boom(GameObject enemy,int damage,GameObject attackerCardGO)
    {
        Debug.Log("Boom_OnKill: Boom: enemy = " + enemy.name);
        GameObject vfxPrefab = Resources.Load<GameObject>("VFX/VFX_boom");
        GameObject vfxInstance = Object.Instantiate(vfxPrefab, enemy.transform);
        vfxInstance.transform.position = enemy.transform.position; // 使用相同的坐标设置方式
        SFX.Instance.StartCoroutine(SFX.PlayAudioCoroutine(audioPath:"Audio/SFX/Battle/boom",startTime:0f));
        yield return new WaitForSeconds(0.3f);
        BattleManager.Instance.ApplyDamageToEnemy(enemy:enemy,damage:damage,monsterAttack:false,attackerCardGO:attackerCardGO);
        yield return new WaitForSeconds(0.35f);
        if (vfxInstance != null)
        {
            Object.Destroy(vfxInstance);
        }
    }

    public IEnumerator Execute(CardEffectContext ctx)
    {
        // 获取怪物
        var enemy = ctx.source;
        Debug.Log("Boom_OnKill: enemy = " + enemy.name);
        int envListIndex = UIGridHelper.FindEnvListIndexByCardGO(enemy,BattleManager.Instance.envCardListList);

        if (envListIndex == -1)
        {
            yield break;
        }

        GameObject enemy_left = null;
        // Debug.Log("Boom_OnKill: enemy_left = " + enemy_left.name);
        GameObject enemy_right = null;
        // Debug.Log("Boom_OnKill: enemy_right = " + enemy_right.name);
        if (envListIndex - 1 >= 0)
        {
            enemy_left = UIGridHelper.GetCardListOrderIndex0(BattleManager.Instance.envPanels[envListIndex - 1]);
            if (enemy_left.GetComponent<CardDisplay>().card.type == "monster" && enemy_left.GetComponent<CardDisplay>().card.health > 0)
            {
                //
            }
            else
            {
                enemy_left = null;
            }

        }
        if (envListIndex + 1 < BattleManager.Instance.envCardListList.Count)
        {
            enemy_right = UIGridHelper.GetCardListOrderIndex0(BattleManager.Instance.envPanels[envListIndex + 1]);
            if (enemy_right.GetComponent<CardDisplay>().card.type == "monster" && enemy_right.GetComponent<CardDisplay>().card.health > 0)
            {
                //
            }
            else
            {
                enemy_right = null;
            }
        }   
        if (enemy_left != null)
        {
            BattleManager.Instance.StartCoroutine(Boom(enemy:enemy_left,damage:baseExtra,attackerCardGO:ctx.source));
        }
        if (enemy_right != null)
        {
            BattleManager.Instance.StartCoroutine(Boom(enemy:enemy_right,damage:baseExtra,attackerCardGO:ctx.source));
        }
        yield return new WaitForSeconds(0.1f);
    }
}