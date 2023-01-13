using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerNames : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        object[] obj = GameObject.FindObjectsOfType(typeof(TextMeshProUGUI));
        foreach (object o in obj)
        {
            TextMeshProUGUI t = (TextMeshProUGUI)o;
            Debug.Log(t.name);
            if(t.name == "PlayerName")
            {
                t.text = "penis";
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
