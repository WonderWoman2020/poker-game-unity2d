using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Net.NetworkInformation;

// Ekran g��wny gry, pierwsza scena widoczna po w��czeniu aplikacji
public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button exitButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button playButton;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnExitButton()
    {
        Application.Quit();
    }

    // TODO (cz. PGGP-107) ekran Settings do zrobienia i zaimplementowania
    public void OnSettingsButton()
    {
        SceneManager.LoadScene("MainSettingsMenu");
    }

    // W zale�no�ci, czy gracz jest zalogowany, prosi o logowanie lub
    // przepuszcza do kolejnych ekran�w gry
    public void OnPlayButton()
    {
        if(MyGameManager.Instance.MainPlayer == null)
            SceneManager.LoadScene("SignInMenu");
        else
            SceneManager.LoadScene("PlayMenu");
    }
}
