using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Singletone<T>: MonoBehaviour where T : MonoBehaviour
{
    private  static T m_instance;

    public static T Instance
    {
        get
        {
            if (m_instance == null) // to find in scene if empty
            {
                m_instance = FindObjectOfType<T>();
                // if it is still empty lets make a new one
                if (m_instance == null)
                {
                    GameObject singletone = new GameObject(typeof(T).Name);
                    m_instance = singletone.AddComponent<T>();
                }
            }
            return m_instance;
        }
    }

    public virtual void Awake()
    {
        if (m_instance == null)
        {
            m_instance = this as T;
            // DontDestroyOnLoad will only work for objects at the root level.
            //transform.parent = null;
           // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
