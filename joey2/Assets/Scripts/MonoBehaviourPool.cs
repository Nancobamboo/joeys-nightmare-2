using System;
using System.Collections.Generic;
using UnityEngine;


public class MonoBehaviourPool<T> where T : Component
{
    private readonly List<T> m_Pool = new List<T>();
    private readonly Func<T> m_CreateFunc;

    public MonoBehaviourPool(Func<T> createFunc)
    {
        m_CreateFunc = createFunc ?? throw new ArgumentNullException(nameof(createFunc));
    }

    public T Get()
    {
        foreach (var obj in m_Pool)
        {
            if (!obj.gameObject.activeSelf)
            {
                obj.gameObject.SetActive(true);
                return obj;
            }
        }

        var newObj = m_CreateFunc();
        m_Pool.Add(newObj);
        newObj.gameObject.SetActive(true);
        return newObj;
    }

    public void Release(T obj)
    {
        obj.gameObject.SetActive(false);
    }

    public void ReleaseAll()
    {
        foreach (var obj in m_Pool)
        {
            obj.gameObject.SetActive(false);
        }
    }

    public int Count => m_Pool.Count;
}
