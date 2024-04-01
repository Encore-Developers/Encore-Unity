using System;
using UnityEngine;

public class Singleton<T>
{
    public static T Instance
    {
        get
        {
            if (_instance == null)
                _instance = (T)((object)Activator.CreateInstance(typeof(T)));

            return _instance;
        }
    }


    private static T _instance;
}

public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public virtual bool DontDestroy() => true;

    public static T Instance
    {
        get
        {
            _instance = FindFirstObjectByType<T>();
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null || _instance == this)
        {
            _instance = GetComponent<T>();

            if (DontDestroy())
                DontDestroyOnLoad(transform.root.gameObject);

            OnAwake();
            return;
        }

        if (_instance != this && gameObject.activeInHierarchy)
            Destroy(gameObject);
    }

    public virtual void OnAwake() { }

    private static T _instance;
}
