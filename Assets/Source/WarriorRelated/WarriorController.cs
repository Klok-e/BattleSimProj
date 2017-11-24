using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Warrior
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class WarriorController : MonoBehaviour
    {
        private SpriteRenderer sprRend;

        public const float distanceOfAttack = 1f;
        public float speed;
        public float rotationSpeed;
        public NeuralAI ai;
        public int team;

        public Actions.Action currentAction;

        private float bloodRemaining;
        private float bloodMax;

        public APlayerController PlayerOwner { get { return playerOwner; } }
        private APlayerController playerOwner;

        public void Initialize(Vector2 pos, StructureOfWarrior str, int team, NeuralAI ai, APlayerController pla)
        {
            currentAction = new Actions.IdleAction();
            sprRend = GetComponent<SpriteRenderer>();

            transform.position = pos;

            playerOwner = pla;

            this.ai = ai;
            this.team = team;
            bloodMax = str.blood;
            speed = HelperConstants.speedMultOfWa;
            rotationSpeed = HelperConstants.warriorRotationSpeed;
            isShooter = true;

            positionsDuringSomeTime = new Helpers.Deque<Vector2>(3);//TODO: magic number

            stats = new Statistics();
            //TODO: add limbs

            rigidbody2d = GetComponent<Rigidbody2D>();

            angles = new List<int>();

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

            #endregion Calculate angles

            Revive(pos);
        }

        public void Revive(Vector2 pos)
        {
            transform.position = pos;
            bloodRemaining = bloodMax;
            ai.network.ResetState();
            tickCount = 0;

            gameObject.SetActive(true);
            transform.GetChild(0).gameObject.SetActive(true);
        }

        private int tickCount;

        public void Tick(Actions.Action action)
        {
            Tick();
            currentAction = action;
        }

        public Rigidbody2D rigidbody2d { get; private set; }

        public void Tick()
        {
            Collider2D[] overlap = new Collider2D[7];
            var myFilter = new ContactFilter2D();
            myFilter.NoFilter();
            myFilter.SetLayerMask(LayerMask.GetMask(HelperConstants.warriorTag));
            int a = GetComponent<CircleCollider2D>().OverlapCollider(myFilter, overlap);
            if (a > 0)
            {
                foreach (var item in overlap)
                {
                    if (item)
                        rigidbody2d.AddRelativeForce((rigidbody2d.position - item.GetComponent<Rigidbody2D>().position).normalized * HelperConstants.warrPushForce);
                }
            }

            currentAction.Tick();
            tickCount++;
        }

        public double[] data { get; private set; }
        public double[] prediction { get; private set; }

        public Actions.Action ChooseAction()
        {
            if (tickCount % HelperConstants.dataRefreshFrequency == 0 || tickCount == 1)
            {
                data = GatherDataOfEnvironment();
            }
            Debug.Assert(data != null);
            prediction = ai.Predict(data);
            Actions.Action action;

            action = PredictionToAction(prediction);
            return action;
        }

        private List<int> angles;

        public readonly int sensorRange = 10;
        public readonly int raycastSensors = 5;
        public readonly int diffBetwSensorsInDeg = 24;

        private double[] GatherDataOfEnvironment()
        {
            var dataData = new List<double>(30);

            #region Raycasts

            var eyesData = new double[raycastSensors, 3];

            var pos = transform.position;

            var mask = LayerMask.GetMask(HelperConstants.warriorTag, HelperConstants.obstacleTag);

            var count = 0;
            foreach (var angle in angles)
            {
                var dir = transform.TransformDirection(new Vector3(Mathf.Cos(Mathf.Deg2Rad * (angle)), Mathf.Sin(Mathf.Deg2Rad * (angle)), 0).normalized);
                RaycastHit2D hit;

                hit = Physics2D.Raycast(pos, dir, sensorRange, mask);
                if (hit)
                {
                    Debug.DrawLine(pos, hit.point);
                    eyesData[count, 0] = 1 - (hit.distance / sensorRange); //was: eyeData[j] = Math.Min(1 / hit.distance, 1);
                    if (hit.collider.tag.Equals(HelperConstants.warriorTag))
                    {
                        var other = hit.collider.GetComponent<WarriorController>();

                        eyesData[count, 1] = other.team == team ? 0 : 1;//0 if same team else 1
                        eyesData[count, 2] = Helpers.NormalizeNumber(Vector2.Angle(other.transform.up, transform.up), 0, 180);//4th element is angle between this warrior and other
                                                                                                                              //eyeData[j + 3] = NormalizeAngle((other.transform.up - transform.up).normalized);
                    }
                    else if (hit.collider.tag.Equals(HelperConstants.obstacleTag))
                    {
                        eyesData[count, 1] = -1;
                        eyesData[count, 2] = -1;
                    }
                }
                else
                {
                    eyesData[count, 0] = -1;
                    eyesData[count, 1] = -1;
                    eyesData[count, 2] = -1;
                }
                count += 1; //every eye must see distance and what is it
            }

            #endregion Raycasts

            foreach (var item in eyesData)
            {
                dataData.Add(item);
            }

            dataData.Add(1 - (bloodRemaining / bloodMax));
            var deltaVector = (playerOwner.transform.position - (Vector3)rigidbody2d.position).normalized;
            dataData.Add(NormalizeAngle(deltaVector));//magic

            var temp = dataData.ToArray();
            Debug.Assert(HelperConstants.totalAmountOfSensors == temp.Length, $"wrong array length: {HelperConstants.totalAmountOfSensors} != {temp.Length}");
            return temp;
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
        public Helpers.Deque<Vector2> positionsDuringSomeTime;

        private Actions.Action PredictionToAction(double[] pred)
        {
            Debug.Assert(pred.Length == HelperConstants.totalAmountOfOutputsOfNet);
            /* [0] - angle to rotate
             * [1] - speed
             * [2] - whether to attack
             * [3] - whether to idle
            */

            if (pred[2] >= 1)//attack
            {
                if (!isShooter)
                    return new Actions.AttackMeleeAction(transform.TransformPoint(Vector3.up), 100, 20, this);
                else
                    return new Actions.AttackShootAction(transform.up, 80, 20, this);
            }
            else if (pred[3] >= 1)//idle
            {
                return new Actions.IdleAction();
            }

            var angle = (float)(pred[0] * rotationSpeed);
            var moveBy = (float)(pred[1]);
            moveBy = moveBy > 0 ? moveBy : moveBy * 0.5f;//backwards is 2 times slower

            return new Actions.MoveAction(moveBy * speed, angle, this);
        }

        #region Statistics

        public Statistics stats { get; private set; }

        public double GetFitness()
        {
            double fitness = 0;

            fitness += (1 - 1 / ((stats.damageFromEnemy) + 1)) * HelperConstants.fitnessBonusForDyingFromEnemy;

            fitness += (1 - 1 / ((stats.enemyDamage) + 1)) * HelperConstants.fitnessForKillingAnEnemy;

            //fitness += Helpers.NormalizeNumber(((1 - 1 / ((stats.passedToFlagDistCoef) + 1)) + (1 - 1 / ((stats.distToFlagCoefBonus) + 1))), 0, 2) * HelperConstants.fitnessMultiplierForApproachingToFlag;//should work
            fitness += (1 - 1 / ((stats.passedToFlagDistCoef) + 1)) * HelperConstants.fitnessMultiplierForApproachingToFlag;

            fitness -= (1 - 1 / ((stats.allyDamage) + 1)) * HelperConstants.fitnessPenaltyForKillingAlly;//more penalty, more substracted from fitness

            return fitness;
        }

        #endregion Statistics

        public bool TakeDamage(float toLose)
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
    }
}
