using UnityEngine;

public class Rintangan : MonoBehaviour
{
    public Transform titikAwal;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.position = titikAwal.position;
            Debug.Log("Kena rintangan! Kembali ke awal.");
        }
    }
}
