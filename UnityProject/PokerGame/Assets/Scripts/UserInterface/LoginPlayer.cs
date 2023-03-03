using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginPlayer : MonoBehaviour
{
    private string playerLogin;
    private string playerPassword;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button backToMenuButton;

    public void OnBackToMenuButton()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void OnLoginButton()
    {
        SceneManager.LoadScene("PlayMenu");
    }

    public void ReadLogin(string login)
    {
        if (login.Length == 0)
        {
            this.playerLogin = null;
            return;
        }

        this.playerLogin = login;
        Debug.Log(this.playerLogin);
    }

    public void ReadPassword(string password)
    {
        if (password.Length == 0)
        {
            this.playerPassword = null;
            return;
        }

        this.playerPassword = password;
        Debug.Log(this.playerPassword);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
