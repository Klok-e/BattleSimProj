using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarriorController : MonoBehaviour
{
    public string toShowOnGui;
    SpriteRenderer sprRend;

    public const float distanceOfAttack = 1f;
    public float speed;
    public float rotationSpeed;
    public NeuralAI ai;
    public int team;

    public Actions.Action currentAction;

    private float bloodRemaining;
    private float bloodMax;

    public PlayerController PlayerOwner { get { return playerOwner; } }
    private PlayerController playerOwner;

    private void Start()
    {
        currentAction = new Actions.IdleAction();
        sprRend = GetComponent<SpriteRenderer>();
        //sprRend.color = team == 1 ? Color.blue : Color.red;
    }

    public void Initialize(Vector2 pos, StructureOfWarrior str, int team, NeuralAI ai, PlayerController pla)
    {
        transform.position = pos;

        playerOwner = pla;

        this.ai = ai;
        this.team = team;
        this.bloodRemaining = str.blood;
        bloodMax = str.blood;
        speed = HelperConstants.speedMultOfWa;
        rotationSpeed = HelperConstants.warriorRotationSpeed;
        isShooter = true;

        //TODO: add limbs
    }

    public void Revive()
    {
        gameObject.SetActive(true);
        transform.GetChild(0).gameObject.SetActive(true);
        transform.position = playerOwner.pointsToVisitDuringTraining[0];
        bloodRemaining = bloodMax;
        ai.network.ResetState();
    }

    #region Warrior
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
        Actions.Action action;

        action = PredictionToAction(pred);
        return action;
    }

    public readonly int sensorRange = 6;
    public readonly int raycastSensors = 7;
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
        var eyeData = new double[raycastSensors * 4];

        var pos = transform.position;
        var mask = LayerMask.GetMask("Obstacle", "Warrior", "Projectile");
        var j = 0;
        foreach (var angle in angles)
        {
            var dir = transform.TransformDirection(new Vector3(Mathf.Cos(Mathf.Deg2Rad * (angle)), Mathf.Sin(Mathf.Deg2Rad * (angle)), 0).normalized);
            RaycastHit2D hit = Physics2D.Raycast(pos, dir, sensorRange, mask);
            if (hit)
            {
                Debug.DrawLine(pos, hit.point);
                eyeData[j] = Math.Min(1 / hit.distance, 1);
                if (hit.collider.tag.Equals("Obstacle"))
                {
                    //Debug.Log(hit.point + "obst" + hit.distance);
                    eyeData[j + 1] = -1;//2nd elem is whether it is warrior or obstacle
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
                else if (hit.collider.tag.Equals("Projectile"))
                {
                    var projectile = hit.collider.GetComponent<ProjectileController>();
                    eyeData[j + 1] = 0;
                    eyeData[j + 2] = -1;//-1 - projectile
                    eyeData[j + 3] = Vector2.Angle(transform.TransformPoint(projectile.Direction), transform.up) * Mathf.Deg2Rad;
                }
            }
            j += 4; //every eye must see distance and what is it
        }
        #endregion

        dataData.AddRange(eyeData);
        dataData.Add(1 - (1 / bloodRemaining));
        var deltaVector = (playerOwner.transform.position - transform.position).normalized;
        dataData.Add(NormalizeAngle(deltaVector));//magic

        #region string to show
        var temp = dataData.ToArray();
        toShowOnGui += Helpers.ArrayToString(temp) + "\n";
        #endregion
        Debug.Assert(HelperConstants.totalAmountOfSensors == temp.Length);
        return temp;//30 sensors total
    }

    private float NormalizeAngle(Vector2 deltaVector)
    {
        float angle = transform.eulerAngles.z % 360f;
        if (angle < 0f)
            angle += 360f;

        float rad = Mathf.Atan2(deltaVector.y, deltaVector.x);
        rad *= Mathf.Rad2Deg;

        rad = rad % 360;
        if (rad < 0)
        {
            rad = 360 + rad;
        }

        rad = 90f - rad;
        if (rad < 0f)
        {
            rad += 360f;
        }
        rad = 360 - rad;
        rad -= angle;
        if (rad < 0)
            rad = 360 + rad;
        if (rad >= 180f)
        {
            rad = 360 - rad;
            rad *= -1f;
        }
        rad *= Mathf.Deg2Rad;

        return rad / (Mathf.PI);
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
                return new Actions.AttackShootAction(transform.up, 80, 20, this);
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
    public float Fitness { get { return fitness; } }
    private float fitness;
    public void AddFitnessToThis(float toAddFit)
    {
        fitness += toAddFit;
    }
    public void ResetFitness()
    {
        fitness = 0;
    }
    #endregion


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
                GameManagerController.inputManagerInstance.simInst.CreateNewProjectile((Vector2)warr.transform.position + direction, direction, damage, warr);
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

            ticksToFinish = 1;
        }

        public override void ActionStuff()
        {
            if (isFinished)
            {
                var posBeforeMove = moves.transform.position;
                moves.transform.Rotate(new Vector3(0, 0, rotateBy));
                moves.transform.Translate(Vector3.up * speed);
                moves.AddFitnessToThis(
                    Mathf.Max(Vector3.Distance(posBeforeMove, moves.PlayerOwner.transform.position) -
                    Vector3.Distance(moves.transform.position, moves.PlayerOwner.transform.position), 0) * 0.007f);//if approached to owner then encourage
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
