using SharpNeat.Phenomes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BattleMode
{
    internal class BattlePlayer : APlayerController
    {
        public int Team { get { return team; } }
        private int team;

        public List<IBlackBox> phenomes;

        private void Update()
        {
        }

        private bool initialized;

        public void Initialize(Vector2 pos, int team, List<IBlackBox> pheno)
        {
            Debug.Assert(team >= 1);
            transform.position = pos;
            this.team = team;
            initialized = true;
            phenomes = pheno;
        }
    }
}