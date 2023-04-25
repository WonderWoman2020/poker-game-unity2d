using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Klasa, która zapewnia nie niszczenie GameObject'u,
// do którego zosta³a do³¹czona
public class PersistentManagers : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
}
