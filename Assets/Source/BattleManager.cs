using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public SimController simController;

    PlayerController[] battlingPlayers = new PlayerController[2];

    public void Initialize()
    {
        simController.Initialize();
    }


    
}
