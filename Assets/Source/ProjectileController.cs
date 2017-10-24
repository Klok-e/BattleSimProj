using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    public Vector2 direction;
    public WarriorController whoShoots;
    public float damage;

    public void Tick()
    {
        transform.Translate(direction * HelperConstants.projectileSpeed);
        direction *= 0.9f;
        if (direction.magnitude < 0.1)
        {
            Die();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag.Equals("Warrior") && collision.gameObject.activeSelf)
        {
            var otherWarr = collision.gameObject.GetComponent<WarriorController>();
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
        gameObject.SetActive(false);
    }
}