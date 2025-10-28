using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class  PureSingleton<T> where T : class, new()
{
    private static bool _can_construct = false;
    private static T instance = null;

    public PureSingleton()
    {
        if (!_can_construct)
            Debug.LogError("[Error] can't call constructer of a singleton!");
    }

    public static T GetInstance()
    {
        if (instance == null)
        {
            _can_construct = true;
            instance = new T();
            _can_construct = false;
        }
        return instance;
    }


    public static T Instance
    {
        get
        {
            return GetInstance();
        }
    }

    
}
