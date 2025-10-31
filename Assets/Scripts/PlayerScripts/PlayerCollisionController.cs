using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollisionController : MonoBehaviour
{
    public GameObject collisionParticles;
    private BoxCollider2D collider;
    PacStudentController playerController;

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
        else if (collision.CompareTag("Teleporter"))
        {
            TeleportToOtherSide(collision.transform);
        }
        else if (collision.CompareTag("Pellet"))
        {
            AudioManager.instance.PlayAudioRandom("eat");
            Destroy(collision.gameObject);
            GameManager.instance.AddScore(10);
        }
        else if (collision.CompareTag("Cherry"))
        {
            AudioManager.instance.PlayAudioRandom("burp");
            Destroy(collision.gameObject);
            GameManager.instance.AddScore(100);
        }
        else if (collision.CompareTag("PowerPellet"))
        {
            AudioManager.instance.PlayAudioRandom("evil");
            GameManager.instance.ActivateScaredMode(true);
            Destroy(collision.gameObject);
            GameManager.instance.AddScore(50);
        }
        else if (collision.CompareTag("Ghost"))
        {
            GhostController ghost = collision.GetComponent<GhostController>();
            if (!GameManager.instance.CanHitGhost(ghost))
            {
                playerController.Die();
                AudioManager.instance.PlayAudioRandom("die");
            }
        }
            
    }

    void TeleportToOtherSide(Transform currTeleporter)
    {
        playerController.Teleport(currTeleporter);
    }

    void CollideWithWall(Vector2 collisionPos)
    {
        Instantiate(collisionParticles, collisionPos, Quaternion.identity, transform);
        AudioManager.instance.PlayAudioRandom("hit");
    }

    public void SetPlayerController(PacStudentController pacStudentController)
    {
        playerController = pacStudentController;
    }
}
