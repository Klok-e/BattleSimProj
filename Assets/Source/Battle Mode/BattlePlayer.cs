using SharpNeat.Phenomes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


class BattlePlayer : APlayerContoller
{
    public int Team { get { return team; } }
    private int team;

    private List<IBlackBox> phenomes;

    private void Update()
    {

    }

    bool initialized;
    public void Initialize(Vector2 pos, int team, List<IBlackBox> pheno)
    {
        Debug.Assert(team > 1);
        transform.position = pos;
        this.team = team;
        initialized = true;
        phenomes = pheno;
    }

    public void SpawnWarriors()
    {
        throw new NotImplementedException();
    }

    public void UserInputTick()
    {

    }

    
}

