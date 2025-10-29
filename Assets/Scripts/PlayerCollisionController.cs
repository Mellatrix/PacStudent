using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollisionController : MonoBehaviour
{
    public GameObject collisionParticles;
    private BoxCollider2D collider;

    private void Awake()
    {
        collider = GetComponent<BoxCollider2D>();
    }

    public void OffsetCollider(Vector2 offset)
    {
        collider.offset = offset * 0.1f;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Tile"))
        {
            CollideWithWall(collision.ClosestPoint(transform.position));
        }
    }

    void CollideWithWall(Vector2 collisionPos)
    {
        Instantiate(collisionParticles, collisionPos, Quaternion.identity, transform);
        AudioManager.instance.PlayAudioRandom("hit");
    }
}
