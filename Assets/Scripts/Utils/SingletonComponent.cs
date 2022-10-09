using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonComponent<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance = null;
    private static readonly object _locker = new object();
    public static T Instance{
        get{
            if(_instance == null){
                lock(_locker){
                    if(_instance == null){
                        _instance = (T)FindObjectOfType(typeof(T));
                        if(_instance == null){
                            GameObject singleton = new GameObject();
                            _instance = singleton.AddComponent<T>();
                            singleton.name = "Singleton" + typeof(T).ToString();
                        }
                    }
                }
            }
            return _instance;
        }
    }
    /// <summary>
    /// 主要用于DontDestroyOnLoad的单例判断是否已经存在
    /// </summary>
    /// <returns></returns>
    protected bool IsInstanceExist(){
        return !(_instance == null);
    }
}
