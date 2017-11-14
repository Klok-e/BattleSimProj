using System;
using UnityEngine;

namespace Warrior
{
    public static class Actions
    {
        public abstract class Action
        {
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
            private Vector2 dealDamageAt;
            private float damage;
            private WarriorController attackingWarr;

            public AttackMeleeAction(Vector2 dealDamageAt, float dmg, int ticksLasts, WarriorController whoDeals)
            {
                attackingWarr = whoDeals;

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
                        var attackedWarr = coll.GetComponent<WarriorController>();
                        attackedWarr.TakeDamage(damage);//TODO: random limbs must be

                        if (attackedWarr.team != attackingWarr.team)//different teams
                        {
                            attackedWarr.stats.damageFromEnemy += damage;
                            attackingWarr.stats.enemyDamage += damage;
                        }
                        else
                        {
                            attackedWarr.stats.damageFromAlly += damage;
                            attackingWarr.stats.allyDamage += damage;
                        }
                    }
                }
            }
        }

        public class AttackShootAction : Action
        {
            public const float radius = 0.1f;
            private Vector2 direction;
            private float damage;
            private WarriorController warr;

            public AttackShootAction(Vector2 dir, float dmg, int ticksLasts, WarriorController whoDeals)
            {
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
                    if (SimEditor.GameManagerController.inputManagerInstance != null)
                    {
                        SimEditor.GameManagerController.inputManagerInstance.simInst.CreateNewProjectile((Vector2)warr.transform.position + direction, direction, damage, warr);
                    }
                    else
                    {
                        BattleMode.BattleManager.battleManagerInst.CreateNewProjectile((Vector2)warr.transform.position + direction, direction, damage, warr);
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
            private WarriorController moves;

            /// <summary>
            /// World tiles per tick
            /// </summary>
            private float speed;

            private float rotateBy;

            public MoveAction(float spd, float rotateBy, WarriorController w)
            {
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
                    var moveToPos = moves.transform.TransformPoint(Vector3.up * speed);

                    var rigidBody = moves.GetComponent<Rigidbody2D>();
                    rigidBody.MovePosition(moveToPos);
                    rigidBody.MoveRotation(rigidBody.rotation + rotateBy);

                    var plOwnerCurrPos = moves.PlayerOwner.transform.position;
                    var plOwnerNextPos = plOwnerCurrPos + (Vector3)moves.PlayerOwner.nextPosChange;

                    var posWillMostLikelyMove = moveToPos;//TODO: pos will most likely move

                    var distMovedTowPl = Vector3.Distance(posBeforeMove, plOwnerCurrPos) - Vector3.Distance(posWillMostLikelyMove, plOwnerNextPos);
                    var distToPl = Vector3.Distance(moves.transform.position, plOwnerCurrPos);

                    if (distMovedTowPl > 0)
                        moves.stats.passedToFlagDistCoef += distMovedTowPl / (Mathf.Pow(distToPl, 2) + 1);
                    else
                        moves.stats.passedFromFlagDistCoef += distMovedTowPl;
                }
            }
        }

        public class IdleAction : Action
        {
            public IdleAction()
            {
                ticksToFinish = 1;
            }

            public override void ActionStuff()
            {
            }
        }
    }
}
