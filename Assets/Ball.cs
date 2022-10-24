using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    Rigidbody2D Rigidbody;
    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody2D>();
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Rigidbody.velocity = new Vector2(Rigidbody.velocity.x * -1, Rigidbody.velocity.y);
        GameController.Singleton.BallTouches++;
        GameController.Singleton.RandomizeBallVelocity();
    }
}
