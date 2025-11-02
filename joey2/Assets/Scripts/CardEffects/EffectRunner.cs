// Scripts/CardEffects/EffectRunner.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectRunner : MonoSingleton<EffectRunner>
{

    public event System.Action OnQueueStart;
    public event System.Action OnQueueEmpty;
    private readonly Queue<IEnumerator> queue = new();
    private bool running = false;

    public void Enqueue(IEnumerator co)
    {
        if (co == null) return;
        queue.Enqueue(co);
        if (!running) StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        running = true;
        // PData.Instance.canOperate = false;
        OnQueueStart?.Invoke();
        while (queue.Count > 0)
        {
            yield return queue.Dequeue();
        }
        OnQueueEmpty?.Invoke();
        // PData.Instance.canOperate = true;
        running = false;
    }

    // 触发器入口：给任意卡牌的效果触发
    public void Raise(CardTrigger trigger, GameObject source, GameObject target = null, int value = 0, Dictionary<string, object> extra = null)
    {
        var holder = source?.GetComponent<EffectHolder>();
        if (holder == null || holder.effects == null) return;

        var ctx = new CardEffectContext { source = source, target = target, value = value, extra = extra };
        foreach (var eff in holder.effects)
        {
            if (eff != null && eff.MatchTrigger(trigger))
            {
                Enqueue(eff.Execute(ctx));
            }
        }
    }
}