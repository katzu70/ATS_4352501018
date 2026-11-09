using UnityEngine;
using UnityEngine.UI;

public class Tujuan : MonoBehaviour
{
    public Text teksPesan;

    void Start()
    {
        teksPesan.text = "";
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            teksPesan.text = "MENANG!";
            Debug.Log("Player sampai di tujuan");
        }
    }
}
