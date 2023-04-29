using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Klasa od napisów informacyjnych wyœwietlanych w grze (ma swój Prefab w Prefabs -> Popup)
// TODO (cz. PGGP-68) dodaæ tutaj metody typu SetPosition, SetText, SetDestroyTime
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
