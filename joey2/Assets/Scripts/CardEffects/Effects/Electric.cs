// Scripts/CardEffects/Effects/BounceToRandomEnemy_OnDealDamage.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Electric : ICardEffect
{
    public int baseExtra; // CSV 传入的基础额外伤害

    public Electric(int baseExtra)
    {
        this.baseExtra = Mathf.Max(0, baseExtra);
    }
    public string Id => "Electric";
    public bool MatchTrigger(CardTrigger trigger) => trigger == CardTrigger.OnPlay;

    public IEnumerator Execute(CardEffectContext ctx)
    {
        // 获取当前场景所有的怪物
        List<GameObject> allEnemies = EnemyManager.GetAllEnemies(BattleManager.Instance.envPanels);
        if (allEnemies.Count == 0)
        {
            yield break;
        }
        List<GameObject> vfxInstances = new List<GameObject>();
        foreach (var enemy in allEnemies)
        {
            GameObject vfxPrefab = Resources.Load<GameObject>("VFX/VFX_LeiDan");
            GameObject vfxInstance = Object.Instantiate(vfxPrefab, enemy.transform);
            vfxInstance.transform.position = enemy.transform.position; // 使用相同的坐标设置方式
            vfxInstances.Add(vfxInstance);
            SFX.Instance.StartCoroutine(SFX.PlayAudioCoroutine(audioPath:"Audio/SFX/Battle/electric",startTime:0f));
        }
        yield return new WaitForSeconds(0.35f);
        foreach (var enemy in allEnemies)
        {
            BattleManager.Instance.ApplyDamageToEnemy(enemy:enemy,damage:baseExtra,monsterAttack:false,attackerCardGO:ctx.source);
        }
        yield return new WaitForSeconds(0.35f);
        foreach (var vfxInstance in vfxInstances)
        {
            Object.Destroy(vfxInstance);
        }
    }
}