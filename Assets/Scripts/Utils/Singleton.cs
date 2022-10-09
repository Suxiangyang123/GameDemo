using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> where T : class,new()
{
    private static T _instance = null;
    private static readonly object _locker = new object();
    //TODO:目前无法限制外部通过new 创建T
    public static T Instance
    {
        get{
            if (_instance == null)
            {
                lock(_locker){
                    if(_instance == null){
                        _instance = new T();
                    }
                }
            }
            return _instance;
        }
    }
}
