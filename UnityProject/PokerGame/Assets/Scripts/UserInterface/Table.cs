using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class Table : MonoBehaviour
{
    [SerializeField] private Button checkButton;
    [SerializeField] private Button allInButton;
    [SerializeField] private Button passButton;
    [SerializeField] private Button bidButton;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void OnCheckButton()
    {
        Debug.Log("Check");
    }
    public void OnAllInButton()
    {
        Debug.Log("All on");
    }
    public void OnPassButton()
    {
        Debug.Log("Pass");
    }
    public void OnBidButton()
    {
        Debug.Log("Bid");
    }
}
