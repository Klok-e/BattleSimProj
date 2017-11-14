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

        public void Initialize(Vector2 start, Vector2 direction, float damage, WarriorController shooter)
        {
            this.transform.position = start;
            transform.up = direction;
            this.damage = damage;
            this.whoShoots = shooter;
            speed = Vector3.up * HelperConstants.projectileSpeed;
        }

        public void Tick()
        {
            speed *= 0.97f;
            transform.Translate(speed);
            if (speed.magnitude < 0.1)
            {
                Die();
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.tag.Equals("Warrior") && collision.gameObject.activeSelf)
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
            else if (collision.gameObject.tag.Equals("Obstacle"))
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
