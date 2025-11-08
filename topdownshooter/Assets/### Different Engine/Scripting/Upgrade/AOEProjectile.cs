using UnityEngine;

public class AOEProjectile : MonoBehaviour
{
    private AOEAttack aoe;
    public void Setup(AOEAttack source)
    {
        aoe = source;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            aoe.DoBlast(transform.position);
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        // failsafe: falls nach 5 sekunden nichts getroffen
        Destroy(gameObject, 5f);
    }
}
