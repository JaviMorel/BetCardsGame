using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player
{
    public int IDPlayer = -1;
    public int Lives = -1;
    public List<Card> Cards = new List<Card>();

    public Player(int nIDPlayers, int nLives)
    {
        IDPlayer = nIDPlayers;
        Lives = nLives;
    }
}
