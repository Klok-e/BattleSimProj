using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    public Vector2 Direction { get { return direction; } }
    private Vector2 direction;

    private WarriorController whoShoots;
    private float damage;

    public void Initialize(Vector2 start, Vector2 direction, float damage, WarriorController shooter)
    {
        this.transform.position = start;
        this.direction = direction;
        this.damage = damage;
        this.whoShoots = shooter;
    }

    public void Tick()
    {
        var speed = Direction * HelperConstants.projectileSpeed;
        transform.Translate(speed);
        direction *= 0.97f;
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
                        otherWarr.AddFitnessToThis(0.4f);
                        whoShoots.AddFitnessToThis(1);
                    }
                    else
                    {
                        whoShoots.AddFitnessToThis(-0.4f);
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