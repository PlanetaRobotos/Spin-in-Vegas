using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mechanics
{
    public class Arrow : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Number") && SceneManager.GetActiveScene().buildIndex == 1)
            {
                TextMesh mesh = other.GetComponent<TextMesh>();
                GameManager.Numbers[GameManager.CurrentNumber] = int.Parse(mesh.text);
            }
        }
    }
}