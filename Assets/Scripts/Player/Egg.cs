﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Minego;

public class Egg : MonoBehaviour
{
    public float ExplosionDelay = 2f;
    public float ExplosionRadius = 1f;
    float createdTime;
    bool exploded = false;
    public float ThrowForce = 100f;
    public float ThrowForceVertical = 100f;

    Rigidbody2D rb2d;
    bool instantBomb = false;

    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        createdTime = Time.timeSinceLevelLoad;
    }

    void Update()
    {
        if (!exploded && Time.timeSinceLevelLoad - createdTime >= ExplosionDelay)
        {
            Explode();
        }
    }

    public void Explode()
    {
        if (exploded)
        {
            return;
        }
        var hits = Physics2D.CircleCastAll(transform.position, ExplosionRadius, Vector2.zero, 0f,
                (1 << LayerMask.NameToLayer("Player")) + (1 << LayerMask.NameToLayer("Obstacle")));
        foreach (var hit in hits)
        {
            if (hit.collider.gameObject.CompareTag("Player"))
            {
                bool notReallyHit = Physics2D.Linecast(transform.position, hit.collider.gameObject.transform.position,
                    (1 << LayerMask.NameToLayer("Ground")) + (1 << LayerMask.NameToLayer("Obstacle")));
                if (notReallyHit)
                {
                    continue;
                }
                var player = hit.collider.GetComponent<Player>();
                player.BeHitted();
            }
            else if (hit.collider.gameObject.CompareTag("Destroyable"))
            {
                var reallyHit = Physics2D.Linecast(transform.position, hit.collider.gameObject.transform.position,
                    (1 << LayerMask.NameToLayer("Ground")) + (1 << LayerMask.NameToLayer("Obstacle")));
                if (reallyHit.rigidbody == hit.rigidbody)
                {
                    var destroyable = hit.collider.GetComponent<IDestroyable>();
                    // The destroy always has a delay so it's safe to call
                    destroyable.BeDestroyed();
                }
            }
        }
        GetComponentInChildren<SpriteRenderer>().color = new Color(1, 0, 0);
        exploded = true;
        rb2d.isKinematic = true;
        rb2d.angularVelocity = 0f;
        rb2d.velocity = Vector2.zero;
        Destroy(gameObject, 0.5f);
    }

    public void BeThrown(Vector2 velocity, bool toRight)
    {
        rb2d.velocity = new Vector2(velocity.x / 4, 0f);
        rb2d.AddForce(new Vector2(ThrowForce * (toRight ? 1 : -1), ThrowForceVertical));
        instantBomb = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (instantBomb && (collision.gameObject.layer == LayerMask.NameToLayer("Ground")
            || collision.gameObject.layer == LayerMask.NameToLayer("Obstacle")))
        {
            Explode();
        }
    }
}