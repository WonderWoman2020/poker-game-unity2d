using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SignInMenu : MonoBehaviour
{
    [SerializeField] private Button exitToMenuButton;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button registerButton;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void OnExitToMenuButton()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void OnLoginButton()
    {
        SceneManager.LoadScene("Login"); ;
    }

    public void OnRegisterButton()
    {
        if (MyGameManager.Instance.MainPlayer == null)
            SceneManager.LoadScene("CreatePlayer");
        else
            SceneManager.LoadScene("PlayMenu");
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
