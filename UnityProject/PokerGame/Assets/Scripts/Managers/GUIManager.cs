using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Klasa, która ma dodawaæ nowe zachowania dla wszystkich ekranów GUI
public class GUIManager : MonoBehaviour
{
    // Dzia³a wed³ug wzorca singleton
    public static GUIManager Instance
    { get; set; }

    void Awake()
    {
        // Wzorzec singleton (ma zawsze istnieæ tylko 1 instancja tej klasy)
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;
    }
    // Update is called once per frame
    void Update()
    {
        // Jeœli w którymœ ekranie zostanie naciœniêty 'Esc', wy³¹czy aplikacjê
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // ¯eby dzia³a³o te¿ w edytorze Unity (dla dewelopera)
            #if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
            #endif

            // ¯eby dzia³a³o w zbudowanej wersji gry
            Application.Quit();
        }
    }
}
