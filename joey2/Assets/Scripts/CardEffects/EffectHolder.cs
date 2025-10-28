// Scripts/CardEffects/CardEffectHolder.cs
using System.Collections.Generic;
using UnityEngine;

public class EffectHolder : MonoBehaviour
{
    // 直接序列化具体实现（简单好用）；若要纯数据驱动，可存 string ids 再用 Registry 生成
    public List<ICardEffect> effects = new();
}