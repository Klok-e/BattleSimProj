using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarriorController : MonoBehaviour
{
    public Sprite[] sprites;

    private void Start()
    {
        GatherDataOfEnvironment();
    }

    void Update()
    {
        GetComponent<SpriteRenderer>().sprite = sprites[0];
        GatherDataOfEnvironment();
    }

    #region Warrior code
    public float fitness;

    public float speed;
    public AAi ai;
    public int team;
    public List<Limb> limbs;
    public float bloodRemaining;
    public Actions.Action currentAction;

    public void Tick(Actions.Action action)
    {
        Tick();
        currentAction = action;
    }

    public void Tick()
    {
        currentAction.Tick();
    }

    public Actions.Action ChooseAction()
    {
        var data = GatherDataOfEnvironment();
        var pred = ai.Predict(data);
        var action = PredictionToAction(pred);
        return action;
    }

    public readonly int sensorRange = 4;
    public readonly int raycastSensors = 5;
    public readonly int diffBetwSensorsInDeg = 15;
    public int totalAmountOfSensors;
    private double[] GatherDataOfEnvironment()
    {
        var angles = new List<int>();
        #region Calculate angles
        Debug.Assert(raycastSensors % 2 == 1, "amount of sensors is too even");
        var mul = 0;
        for (int i = 0; i < raycastSensors; i++)
        {
            if (i % 2 == 0)
            {
                angles.Add(90 - diffBetwSensorsInDeg * mul);
            }
            if (i % 2 == 1)
            {
                mul += 1;
                angles.Add(90 + diffBetwSensorsInDeg * mul);
            }
        }
        #endregion

        var dataData = new List<double>();
        #region Raycasts
        var eyeData = new double[raycastSensors * 2];

        var pos = transform.position;
        var mask = LayerMask.GetMask("Obstacle", "Warrior");
        var j = 0;
        foreach (var angle in angles)
        {
            var dir = transform.TransformDirection(new Vector3(Mathf.Cos(Mathf.Deg2Rad * (angle)), Mathf.Sin(Mathf.Deg2Rad * (angle)), 0).normalized);
            RaycastHit2D hit = Physics2D.Raycast(pos, dir, sensorRange, mask);
            if (hit)
            {
                Debug.DrawLine(pos, hit.point);
                if (hit.collider.tag.Equals("Obstacle"))
                {
                    Debug.Log(hit.point + "obst" + hit.distance);
                    eyeData[j + 1] = 1;
                }

                else if (hit.collider.tag.Equals("Warrior"))
                {
                    Debug.Log(hit.point + "warr" + hit.distance);
                    eyeData[j + 1] = 2;
                }
                eyeData[j] = hit.distance;
            }
            j += 2; //every eye must see distance and what is it
        }
        #endregion

        dataData.AddRange(eyeData);
        dataData.Add(bloodRemaining);
        dataData.Add(team);

        totalAmountOfSensors = dataData.Count;
        return dataData.ToArray();
    }

    private Actions.Action PredictionToAction(double[] pred)
    {
        throw new NotImplementedException();
    }

    public int GetActionNum()
    {
        if (currentAction.number == 0) throw new Exception("Wrong");
        return currentAction.number;
    }
    #endregion
}
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
        WarriorController moves;
        /// <summary>
        /// World tiles per tick
        /// </summary>
        float speed;
        Vector2 direction;

        public MoveAction(float spd, Vector2 drctn, WarriorController w)
        {
            number = 3;

            speed = spd;
            direction = drctn;
            moves = w;

            var temp = speed;
            ticksToFinish = 1;
        }

        public override void ActionStuff()
        {
            if (isFinished)
                moves.transform.position += new Vector3(direction.x, direction.y, 0);
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

    WarriorController warrior;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="hp"></param>
    /// <param name="armor">percent of damage reflected</param>
    /// <param name="isVital"></param>
    /// <param name="wa"></param>
    public Limb(string name, float hp, float armor, bool isVital, WarriorController wa = null)
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

    public Limb Copy(WarriorController warrior)
    {
        return new Limb(name, hp, armorPercent, isVital, warrior);
    }
}



public class StructureOfWarrior
{
    public float blood;
    public List<Limb> str;
}
