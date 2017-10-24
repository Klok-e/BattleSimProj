using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarriorController : MonoBehaviour
{
    public string toShowOnGui;
    SpriteRenderer sprRend;

    private void Start()
    {
        currentAction = new Actions.IdleAction();
        sprRend = GetComponent<SpriteRenderer>();
        sprRend.color = team == 1 ? Color.blue : Color.red;
    }

    public void Revive()
    {
        gameObject.SetActive(true);
        transform.GetChild(0).gameObject.SetActive(true);
    }

    #region Warrior


    public const float distanceOfAttack = 1f;
    public float speed;
    public float rotationSpeed;
    public NeuralAI ai;
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

    public bool userControlled;
    public Actions.Action ChooseAction()
    {
        toShowOnGui = "";
        var data = GatherDataOfEnvironment();
        var pred = ai.Predict(data);
        Actions.Action action;
        if (userControlled)
        {
            var playerPred = new double[HelperConstants.totalAmountOfOutputsOfNet];

            float move_x = Input.GetAxis("Horizontal");
            float move_y = Input.GetAxis("Vertical");

            playerPred[1] = Math.Min(Input.GetAxis("Horizontal") + 1, 1);//angle
            playerPred[0] = Math.Min(Input.GetAxis("Vertical") + 1, 1);//speed
            playerPred[2] = Math.Min(Input.GetAxis("Jump"), 1);

            action = PredictionToAction(playerPred);
        }
        else
        {
            action = PredictionToAction(pred);
        }
        return action;
    }

    public readonly int sensorRange = 6;
    public readonly int raycastSensors = 7;
    public readonly int diffBetwSensorsInDeg = 22;
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
        var eyeData = new double[raycastSensors * 4];

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
                eyeData[j] = Math.Min(1 / hit.distance, 1);
                if (Debug.isDebugBuild)
                {

                    if (eyeData[j] > 1)
                        Debug.Log(hit.distance);

                }
                if (hit.collider.tag.Equals("Obstacle"))
                {
                    //Debug.Log(hit.point + "obst" + hit.distance);
                    eyeData[j + 1] = -1;
                    eyeData[j + 2] = 0;//3rd element is team
                    eyeData[j + 3] = 0;//4th element is angle between this warrior and other
                }

                else if (hit.collider.tag.Equals("Warrior"))
                {
                    var other = hit.collider.GetComponent<WarriorController>();
                    //Debug.Log(hit.point + "warr" + hit.distance);
                    eyeData[j + 1] = 1;
                    eyeData[j + 2] = other.team == team ? 0 : 1;//0 if same team else 1
                    eyeData[j + 3] = Vector2.Angle(other.transform.up, transform.up) * Mathf.Deg2Rad;//4th element is angle between this warrior and other
                }
                eyeData[j] = hit.distance;
            }
            j += 4; //every eye must see distance and what is it
        }
        #endregion

        dataData.AddRange(eyeData);
        dataData.Add(1 - (1 / bloodRemaining));

        #region string to show
        var temp = dataData.ToArray();
        toShowOnGui += Helpers.ArrayToString(temp) + "\n";
        #endregion
        Debug.Assert(HelperConstants.totalAmountOfSensors == temp.Length);
        return temp;//29 sensors total
    }

    public bool isShooter;
    private Actions.Action PredictionToAction(double[] pred)
    {
        Debug.Assert(pred.Length == HelperConstants.totalAmountOfOutputsOfNet);
        /* [0] - angle to rotate
         * [1] - speed
         * [2] - whether to attack
         * [3] - whether to dodge
         * [4] - whether to block
        */

        #region to show
        toShowOnGui += Helpers.ArrayToString(pred);
        #endregion

        var angle = (float)(pred[0] > 0.5 ? 0.5 * rotationSpeed : -0.5 * rotationSpeed);

        if (pred[2] == 1)//attack
        {
            if (!isShooter)
                return new Actions.AttackMeleeAction(transform.TransformPoint(Vector3.up), 100, 20, this);
            else
                return new Actions.AttackShootAction(Vector3.up, 80, 20, this);
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
        //new Vector2(Mathf.Cos((float)pred[0] * 360 * Mathf.Deg2Rad), Mathf.Sin((float)pred[0] * 360 * Mathf.Deg2Rad)).normalized

        //Debug.Log(transform.localEulerAngles+" angle");
        return new Actions.MoveAction((float)pred[1] * speed, angle, this);
    }

    #region Statistics
    public float fitness;
    public void AddFitnessToThis(float toAddFit)
    {
        fitness += toAddFit;

        ai.genome.EvaluationInfo.SetFitness(Math.Max(fitness, 0));
    }
    public float GetFitness()
    {
        return fitness;
    }
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

    public class AttackMeleeAction : Action
    {
        public const float radius = 0.1f;
        Vector2 dealDamageAt;
        float damage;
        WarriorController warr;

        public AttackMeleeAction(Vector2 dealDamageAt, float dmg, int ticksLasts, WarriorController whoDeals)
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
                    var collWarr = coll.GetComponent<WarriorController>();
                    if (collWarr.LoseBlood(damage))//TODO: random limbs must be
                    {
                        collWarr.AddFitnessToThis(collWarr.team == warr.team ? 0 : 0.5f);//encourage death from enemies
                        warr.AddFitnessToThis(collWarr.team == warr.team ? -0.4f : 1);//if team the same then subtract 0.4
                    }
                }
            }
        }
    }

    public class AttackShootAction : Action
    {
        public const float radius = 0.1f;
        Vector2 direction;
        float damage;
        WarriorController warr;

        public AttackShootAction(Vector2 dir, float dmg, int ticksLasts, WarriorController whoDeals)
        {
            number = 0;

            warr = whoDeals;

            ticksToFinish = ticksLasts;
            direction = dir;
            damage = dmg;
        }

        public override void ActionStuff()
        {
            Debug.DrawRay(warr.transform.position, direction);
            if (isFinished)
            {
                throw new NotImplementedException();
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
        float rotateBy;

        public MoveAction(float spd, float rotateBy, WarriorController w)
        {
            number = 3;

            speed = spd;
            this.rotateBy = rotateBy;
            moves = w;

            var temp = speed;
            ticksToFinish = 1;
        }

        public override void ActionStuff()
        {
            if (isFinished)
            {
                moves.transform.Rotate(new Vector3(0, 0, rotateBy));
                moves.transform.Translate(Vector3.up * speed);
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
