using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersistentManagers : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
}
