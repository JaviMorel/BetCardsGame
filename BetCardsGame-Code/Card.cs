using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum CardType
{
    Oros, Bastos, Espadas, Copas, NoAsignado
}

public class Card : MonoBehaviour
{
    public int ID = -1;
    public int Num = 0;
    public CardType Type = CardType.NoAsignado;
    public int Owner = -1;
    public Sprite FrontImage = null;
    public Sprite BackImage = null;


    public Card(Card c)
    {
        ID = c.ID;
        Num = c.Num;
        Type = c.Type;
        Owner = c.Owner;
        FrontImage = c.FrontImage;
        BackImage = c.BackImage;
    }

}
