using UnityEngine;

/// <summary>
/// Generic singleton class that can be inherited by any MonoBehaviour.
/// 어떤 MonoBehaviour에서도 상속받아 사용할 수 있는 제네릭 싱글톤 클래스입니다.
/// </summary>
/// <typeparam name="T">The type of the singleton component. 싱글톤 컴포넌트의 타입입니다.</typeparam>
public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    /// <summary>
    /// Private instance of the singleton
    /// 싱글톤의 비공개 인스턴스
    /// </summary>
    private static T _instance;
    
    /// <summary>
    /// Thread safety lock object
    /// 스레드 안전성을 위한 락 객체
    /// </summary>
    private static readonly object _lock = new object();
    
    /// <summary>
    /// Flag to check if application is quitting
    /// 애플리케이션 종료 여부를 확인하는 플래그
    /// </summary>
    private static bool _applicationIsQuitting = false;
    
    /// <summary>
    /// Public accessor for the singleton instance.
    /// 싱글톤 인스턴스에 대한 공개 접근자.
    /// If no instance exists, one will be created.
    /// 인스턴스가 없는 경우 새로 생성합니다.
    /// </summary>
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
    
    /// <summary>
    /// Called when the MonoBehaviour is initialized. Ensures only one instance exists.
    /// MonoBehaviour가 초기화될 때 호출됩니다. 하나의 인스턴스만 존재하도록 보장합니다.
    /// </summary>
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
    
    /// <summary>
    /// Called when the application is about to quit.
    /// Sets the application quitting flag to prevent access after quitting.
    /// 애플리케이션이 종료될 때 호출됩니다.
    /// 종료 후 접근을 방지하기 위해 애플리케이션 종료 플래그를 설정합니다.
    /// </summary>
    protected virtual void OnApplicationQuit()
    {
        _applicationIsQuitting = true;
    }
}