using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupText : MonoBehaviour
{
    public float DestroyTime = 3f;
    public Vector3 Offset = new Vector3(0, -300, 0);

    void Start()
    {
        Destroy(gameObject, DestroyTime);

        transform.localPosition += Offset;
    }
}
