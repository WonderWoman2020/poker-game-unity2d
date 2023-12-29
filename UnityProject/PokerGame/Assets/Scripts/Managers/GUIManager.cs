using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Klasa, kt�ra ma dodawa� nowe zachowania dla wszystkich ekran�w GUI
public class GUIManager : MonoBehaviour
{
    // Dzia�a wed�ug wzorca singleton
    public static GUIManager Instance
    { get; set; }

    void Awake()
    {
        // Wzorzec singleton (ma zawsze istnie� tylko 1 instancja tej klasy)
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;
    }
    // Update is called once per frame
    void Update()
    {
        // Je�li w kt�rym� ekranie zostanie naci�ni�ty 'Esc', wy��czy aplikacj�
        //if (Input.GetKeyDown(KeyCode.Escape))
        //{
        //    // �eby dzia�a�o te� w edytorze Unity (dla dewelopera)
        //    #if UNITY_EDITOR
        //            UnityEditor.EditorApplication.isPlaying = false;
        //    #endif

        //    // �eby dzia�a�o w zbudowanej wersji gry
        //    Application.Quit();
        //}
    }
}
