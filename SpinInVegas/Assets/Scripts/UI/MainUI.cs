using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class MainUI : MonoBehaviour
    {
        [SerializeField] private Text scoreText;
        [SerializeField] private Text[] numbers;

        private void OnEnable()
        {
            GameManager.OnSetNumber += SetNumber;
        }

        private void OnDisable()
        {
            GameManager.OnSetNumber -= SetNumber;
        }

        private void Start()
        {
            if (SceneManager.GetActiveScene().buildIndex == 1)
                UpdateScoreText();
        }

        public void StartGame() => SceneManager.LoadScene(1);

        public void Quit() => Application.Quit();

        public void UpdateScoreText() => scoreText.text = $"Score: {PlayerPrefs.GetInt("CurrentScore")}";

        private void SetNumber(int curNumber) =>
            numbers[curNumber].text =
                GameManager.Numbers[curNumber] != 0 ? GameManager.Numbers[curNumber].ToString() : "";
    }
}