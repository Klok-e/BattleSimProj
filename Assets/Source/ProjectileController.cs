using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            var otherWarr = collision.gameObject.GetComponent<WarriorController>();
            Debug.Assert(otherWarr != null);
            if (otherWarr != whoShoots)
            {
                bool wasKilled = otherWarr.LoseBlood(damage);
                if (wasKilled)
                {
                    if (otherWarr.team != whoShoots.team)
                    {
                        otherWarr.AddFitnessToThis(HelperConstants.fitnessBonusForDyingFromEnemy);
                        whoShoots.AddFitnessToThis(HelperConstants.fitnessForKillingAnEnemy);
                    }
                    else
                    {
                        whoShoots.AddFitnessToThis(-HelperConstants.fitnessBonusForDyingFromEnemy);
                    }
                }
                Die();
            }
        }
        else if (collision.gameObject.tag.Equals("Obstacle"))
        {
            Die();
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}