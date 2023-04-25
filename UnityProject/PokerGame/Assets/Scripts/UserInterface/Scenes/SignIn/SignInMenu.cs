using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/* Ekran wyboru opcji wejœcia na konto
 * - rejestracja (stwórz nowe konto)
 * - logowanie
 */
public class SignInMenu : MonoBehaviour
{
    [SerializeField] private Button exitToMenuButton;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button registerButton;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnExitToMenuButton()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void OnLoginButton()
    {
        SceneManager.LoadScene("Login");
    }

    public void OnRegisterButton()
    {
        SceneManager.LoadScene("CreatePlayer");
    }
}
