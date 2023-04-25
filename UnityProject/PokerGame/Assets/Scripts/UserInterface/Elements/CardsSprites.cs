using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/* Klasa trzymająca w sobie listę sprite'ów kart
 * Kolejność sprite'ów (można też sprawdzić w Prefabs -> Cards_Collection):
 * 1. Serca: 2-13, As
 * 2. Piki: 2-13, As
 * 3. Karo: 2-13, As
 * 4. Trefl: 2-13, As
 * 5. Sprite odwrotnej strony karty
 */
// TODO przenieść tutaj metody do wyświetlania kart na stole?
// albo dać tu jakąś metodę przyjmującą Card i zwracającą odpowiedni sprite
public class CardsSprites : MonoBehaviour
{

    [SerializeField]
    public Sprite[] cardsSpriteSerialization;

    public CardsSprites()
    {

    }
}

