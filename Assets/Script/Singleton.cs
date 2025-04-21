using UnityEngine;

/// <summary>
/// Generic singleton class that can be inherited by any MonoBehaviour
/// </summary>
/// <typeparam name="T">Type of the singleton</typeparam>
public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static readonly object _lock = new object();
    private static bool _applicationIsQuitting = false;
    
    public static T Instance
    {
        get
        {
            if (_applicationIsQuitting)
            {
                Debug.LogWarning($"[Singleton] Instance '{typeof(T)}' already destroyed on application quit. Returning null.");
                return null;
            }
            
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = (T)FindAnyObjectByType(typeof(T));
                    
                    if (FindObjectsByType(typeof(T), FindObjectsSortMode.None).Length > 1)
                    {
                        Debug.LogError($"[Singleton] Something went wrong - there should never be more than 1 singleton! Reopening the scene might fix it.");
                        return _instance;
                    }
                    
                    if (_instance == null)
                    {
                        GameObject singletonObject = new GameObject();
                        _instance = singletonObject.AddComponent<T>();
                        singletonObject.name = $"{typeof(T).Name} (Singleton)";
                        
                        DontDestroyOnLoad(singletonObject);
                        
                        Debug.Log($"[Singleton] An instance of {typeof(T)} was created.");
                    }
                }
                
                return _instance;
            }
        }
    }
    
    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Debug.LogWarning($"[Singleton] Another instance of {typeof(T)} already exists! Destroying this duplicate.");
            Destroy(gameObject);
        }
    }
    
    protected virtual void OnApplicationQuit()
    {
        _applicationIsQuitting = true;
    }
}