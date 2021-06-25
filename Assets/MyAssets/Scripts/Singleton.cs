//*** Script by Federico Gobbato ***

using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;


//THIS SINGLETON ALWAYS WORK AT RUNTIME, BUT ON EDITOR YOU STILL CAN ADD TWO EQUAL COMPONENTS
public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    [SerializeField]
    protected bool m_DontDestroyOnLoad = false;
    //[SerializeField]
    //protected bool m_CreateIfNotFound = false;

    static object m_Lock = new object();
    static T m_Instance = null;
    static bool m_ShuttingDown = false;

    public static T Instance
    {
        get
        {
            lock (m_Lock)
            {
                if (m_Instance == null)
                {
                    m_Instance = (T)FindObjectOfType(typeof(T));

                    //if (m_Instance == null && !m_ShuttingDown)
                    //{
                    //    GameObject singleton = new GameObject(typeof(T).ToString() + "Singleton", typeof(T));
                    //}
                }
            }

            if (m_ShuttingDown && !m_Instance)
            {
                Debug.LogWarning("[Singleton] Instance '" + typeof(T) +
                    "' already destroyed. Returning null.");
                return null;
            }

            return m_Instance;
        }
    }


    protected virtual void Awake()
    {
        m_ShuttingDown = false;

        T newIstance = GetComponent<T>();

        //Check if instance already exists
        if (m_Instance == null)
        {
            //if not, set instance to this
            m_Instance = newIstance;

            //Sets this to not be destroyed when reloading scene
            //The Singleton is going to lose every reference of objects destroyed when a new scene, or the same, is loaded
            if (m_DontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }

        }
        else if (m_Instance != newIstance)
        {
            //Then destroy this. There can only ever be one instance of a Singleton<T>.
            Debug.LogWarning(newIstance + " component was destroyed, because is a singleton and already existing");
            Destroy(newIstance);
        }
    }

    protected void OnApplicationQuit()
    {
        m_ShuttingDown = true;
        m_Instance = null;
    }

    protected void OnDestroy()
    {
        m_ShuttingDown = true;
        m_Instance = null;
    }
}
