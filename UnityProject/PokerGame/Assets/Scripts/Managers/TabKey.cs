using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TabKey : MonoBehaviour
{

    [SerializeField] List<TMP_InputField> Fields;
    int InputSelected;
    // Start is called before the first frame update
    void Start()
    {
        Fields[0].Select();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            InputSelected++;
            if (InputSelected == Fields.Count)
                InputSelected = 0;
            Fields[InputSelected].Select();
            
        }
    }
}
