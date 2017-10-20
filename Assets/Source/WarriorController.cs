using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarriorController : MonoBehaviour
{
    public Sprite[] sprites;

    public string toShowOnGui;

    private void Start()
    {
        currentAction = new Actions.IdleAction();
        GetComponent<SpriteRenderer>().sprite = sprites[0];
    }

    void Update()
    {

    }

    #region Warrior


    public const float distanceOfAttack = 1f;
    public float speed;
    public AAi ai;
    public int team;
    public List<Limb> limbs;

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
        toShowOnGui = "";
        var data = GatherDataOfEnvironment();
        var pred = ai.Predict(data);
        var action = PredictionToAction(pred);
        return action;
    }

    public readonly int sensorRange = 4;
    public readonly int raycastSensors = 5;
    public readonly int diffBetwSensorsInDeg = 15;
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
                eyeData[j] = 1 / hit.distance;
                if (hit.collider.tag.Equals("Obstacle"))
                {
                    //Debug.Log(hit.point + "obst" + hit.distance);
                    eyeData[j + 1] = 1;
                }

                else if (hit.collider.tag.Equals("Warrior"))
                {
                    //Debug.Log(hit.point + "warr" + hit.distance);
                    eyeData[j + 1] = 2;
                }
                eyeData[j] = hit.distance;
            }
            j += 2; //every eye must see distance and what is it
        }
        #endregion

        dataData.AddRange(eyeData);
        dataData.Add(1 - (1 / bloodRemaining));
        dataData.Add(team);

        #region string to show
        var temp = dataData.ToArray();
        toShowOnGui += Helpers.ArrayToString(temp) + "\n";
        #endregion

        return temp;//12 sensors total
    }

    private Actions.Action PredictionToAction(double[] pred)
    {
        Debug.Assert(pred.Length == HelperConstants.totalAmountOfOutputsOfNet);
        /* [0] - angle
         * [1] - speed
         * [2] - whether to attack
         * [3] - whether to dodge
         * [4] - whether to block
        */

        #region to show
        toShowOnGui += Helpers.ArrayToString(pred);
        #endregion

        if (pred[2] > pred[3] && pred[2] > pred[4])//attack
        {
            return new Actions.AttackAction(transform.TransformPoint(transform.up * distanceOfAttack), 100, 20, this);
        }
        /*
        else if (pred[3] > pred[2] && pred[3] > pred[4])//dodge
        {
            return new Actions.DodgeAction();
        }
        else if (pred[4] > pred[3] && pred[4] > pred[2])//block
        {
            return new Actions.BlockAction();
        }*/
        return new Actions.MoveAction((float)pred[1] * speed, new Vector2(Mathf.Cos((float)pred[0] * 360 * Mathf.Deg2Rad), Mathf.Sin((float)pred[0] * 360 * Mathf.Deg2Rad)).normalized, this);
    }

    #region Statistics
    public float fitness;
    #endregion

    public float bloodRemaining;
    public bool LoseBlood(float toLose)
    {
        transform.GetChild(0).GetComponent<ParticleSystem>().Play();
        bloodRemaining -= toLose;
        if (bloodRemaining <= 0)
        {
            gameObject.SetActive(false);
            return true;
        }
        return false;
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
        public const float radius = 0.1f;
        Vector2 dealDamageAt;
        float damage;
        WarriorController warr;

        public AttackAction(Vector2 dealDamageAt, float dmg, int ticksLasts, WarriorController whoDeals)
        {
            number = 0;

            warr = whoDeals;

            ticksToFinish = ticksLasts;
            this.dealDamageAt = dealDamageAt;
            damage = dmg;
        }

        public override void ActionStuff()
        {
            DebugExtension.DebugWireSphere(dealDamageAt, radius);
            if (isFinished)
            {

                var coll = Physics2D.OverlapCircle(dealDamageAt, radius, LayerMask.GetMask("Warrior"));
                if (coll)
                {
                    if (coll.GetComponent<WarriorController>().LoseBlood(damage))//TODO: random limbs must be
                        warr.fitness += 1;
                }
            }
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
            {
                moves.transform.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
                moves.transform.Translate(direction * speed);
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
        warrior.LoseBlood(dmg);
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
