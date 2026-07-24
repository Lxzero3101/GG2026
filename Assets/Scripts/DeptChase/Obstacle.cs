using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public float speed = 6f;
    private float despawnX = -10f;

    void Update()
    {
        transform.Translate(Vector3.left * speed * Time.deltaTime);

        if (transform.position.x < despawnX)
            Destroy(gameObject);
    }
}