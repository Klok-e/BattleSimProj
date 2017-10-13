﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Battle_sim_imp
{
    public static class Actions
    {
        public abstract class Action
        {
            public int number;
            public int ticksToFinish;
            public bool isFinished;
            public void Tick()
            {
                if (!isFinished)
                {
                    ticksToFinish--;
                    if (ticksToFinish <= 0) isFinished = true;
                    ActionStuff();
                }
            }
            public abstract void ActionStuff();
        }

        public class AttackAction : Action
        {
            Limb dealDamageTo;
            float damage;

            public AttackAction(Limb dealDamageTo, float dmg, int ticksLasts)
            {
                number = 0;

                ticksToFinish = ticksLasts;
                this.dealDamageTo = dealDamageTo;
                damage = dmg;
            }

            public override void ActionStuff()
            {
                if (isFinished)
                    dealDamageTo.GetDamage(damage);
            }
        }

        public class DodgeAction : Action
        {
            public override void ActionStuff()
            {
                throw new NotImplementedException();
            }
        }

        public class BlockAction : Action
        {
            public override void ActionStuff()
            {
                throw new NotImplementedException();
            }
        }

        public class MoveAction : Action
        {
            Warrior moves;
            /// <summary>
            /// World tiles per tick
            /// </summary>
            float speed;
            Block direction;

            public MoveAction(float spd, Block drctn, Warrior w)
            {
                number = 3;

                speed = spd;
                direction = drctn;
                moves = w;

                var temp = speed;
                ticksToFinish = (int)Math.Min(1 / temp, 1 / temp);
            }

            public override void ActionStuff()
            {
                if (isFinished && direction.warriorsAtThis.Count == 0)
                {
                    moves.BlockThisAt.warriorsAtThis.Remove(moves);
                    moves.BlockThisAt = direction;
                }
            }
        }

        public class IdleAction : Action
        {
            public IdleAction()
            {
                number = 4;
                ticksToFinish = 1;
            }

            public override void ActionStuff()
            {

            }
        }
    }


    public class Limb
    {
        public string name;
        public bool isVital;
        public bool severed;
        public float hp;
        public float armorPercent;

        Warrior warrior;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="hp"></param>
        /// <param name="armor">percent of damage reflected</param>
        /// <param name="isVital"></param>
        /// <param name="wa"></param>
        public Limb(string name, float hp, float armor, bool isVital, Warrior wa = null)
        {
            warrior = wa;
            this.name = name;
            this.hp = hp;
            armorPercent = armor;
            this.isVital = isVital;
        }

        public void GetDamage(float dmg)
        {
            hp -= dmg;
            warrior.bloodRemaining -= dmg;
            if (hp < 0) severed = true;
        }

        public Limb Copy(Warrior warrior)
        {
            return new Limb(name, hp, armorPercent, isVital, warrior);
        }
    }

    public class Warrior
    {
        public float speed;

        public AAi ai;

        public int team;

        public Block BlockThisAt { get { return blockThisAt; } set { if (!value.warriorsAtThis.Contains(this)) { value.warriorsAtThis.Add(this); blockThisAt = value; } } }
        private Block blockThisAt;
        public Block[,] mapThisAt;

        public List<Limb> limbs;
        public float bloodRemaining;

        public Actions.Action currentAction;

        public Warrior(StructureOfWarrior warStrct, Block[,] map, Vector2 pos, int tm, AAi ai)
        {
            this.ai = ai;
            blockThisAt = map[(int)(pos.x), (int)(pos.y)];
            team = tm;

            mapThisAt = map;
            bloodRemaining = warStrct.blood;

            limbs = new List<Limb>();
            foreach (var l in warStrct.str)
            {
                limbs.Add(l.Copy(this));
            }
            speed = 0.01f;
        }

        public void Tick(Actions.Action action)
        {
            Tick();
            currentAction = action;
        }

        public void Tick()
        {
            currentAction.Tick();
        }

        public int GetActionNum()
        {
            if (currentAction.number == 0) throw new Exception("Wrong");
            return currentAction.number;
        }
    }

    public class StructureOfWarrior
    {
        public float blood;
        public List<Limb> str;
    }
}
