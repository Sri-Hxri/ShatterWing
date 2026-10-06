using UnityEngine;
public class PlayerCollision : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<Asteroid>() || collision.gameObject.GetComponent<EnemyShip>())
            GetComponent<PlayerHealth>().TakeHit();
    }
}
