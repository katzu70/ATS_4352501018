using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGerak : MonoBehaviour
{
    public float kecepatan = 5f;
    private Rigidbody2D rb;
    private Vector2 arah;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        arah = Vector2.zero;
        if (Keyboard.current.wKey.isPressed) arah.y = 1;
        if (Keyboard.current.sKey.isPressed) arah.y = -1;
        if (Keyboard.current.aKey.isPressed) arah.x = -1;
        if (Keyboard.current.dKey.isPressed) arah.x = 1;
    }

    void FixedUpdate()
    {
        rb.linearVelocity = arah.normalized * kecepatan;
    }
}
