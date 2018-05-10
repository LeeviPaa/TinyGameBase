using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Toolbox : Singleton<Toolbox>
{
    protected Toolbox() { } // guarantee this will be always a singleton only - can't use the constructor!

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if(GameObject.FindObjectsOfType<Toolbox>().Length > 1)
        {
            GameObject.Destroy(gameObject);
        }
        //E = RegisterComponent<EventManager>();
        // Your initialization code here
    }

    static public T RegisterComponent<T>() where T : Component
    {
        return Instance.GetOrAddComponent<T>();
    }
}

[System.Serializable]
public class Language
{
    public string current;
    public string lastLang;
}