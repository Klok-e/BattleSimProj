using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Warrior
{
    public class ProjectileController : MonoBehaviour
    {
        private WarriorController whoShoots;
        private float damage;
        private Vector3 speed;
        private Rigidbody2D body;

        public void Initialize(Vector2 start, Vector2 direction, float damage, WarriorController shooter)
        {
            body = GetComponent<Rigidbody2D>();
            body.position = start;

            body.AddRelativeForce(direction * HelperConstants.projectileAccel, ForceMode2D.Impulse);

            this.damage = damage;
            this.whoShoots = shooter;
        }

        public void Tick()
        {
            if (body.velocity.magnitude < 1)
            {
                Die();
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.tag.Equals(HelperConstants.warriorTag) && collision.gameObject.activeSelf)
            {
                var attackedWarr = collision.gameObject.GetComponent<WarriorController>();
                Debug.Assert(attackedWarr != null);
                Debug.Assert(attackedWarr.stats != null);
                if (attackedWarr != whoShoots)
                {
                    attackedWarr.TakeDamage(damage);

                    if (attackedWarr.team != whoShoots.team)//different teams
                    {
                        attackedWarr.stats.damageFromEnemy += damage;
                        whoShoots.stats.enemyDamage += damage;
                    }
                    else
                    {
                        attackedWarr.stats.damageFromAlly += damage;
                        whoShoots.stats.allyDamage += damage;
                    }

                    Die();
                }
            }
            else if (collision.gameObject.tag.Equals(HelperConstants.obstacleTag))
            {
                Die();
            }
        }

        public void Die()
        {
            Destroy(gameObject);
        }
    }
}
