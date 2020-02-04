using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager sInstance;
    public float timeLeft = 301.0f;
    public Text scoreText;
    public Text timerText;
    public Text highScoreText;
    public GameObject gameOverMenu;
    public bool isTicking; 


  
    private int previousHighScore;
    private int playerScore=0;

    private void Awake()
    {
        if (sInstance == null)
            sInstance = this;

        else if (sInstance != this)
            Destroy(gameObject);
    }

    private void Start()
    {
        EnableText();
        Reset();
        isTicking = true;
    }

    private void Update()
    {
        DecreaseTimer();
    }

    void DecreaseTimer()
    {
        if(isTicking)
        {
            timeLeft -= Time.deltaTime;
            var timeLeftInMins = TimeSpan.FromSeconds(timeLeft);

            timerText.text = "Time Left: " + timeLeftInMins.ToString(@"mm\:ss");

            if (timeLeft <= 0f)
            {
                ShowGameOverMenu();
                Reset();
            }
                

        }
       
    }

    void EnableText()
    {
        previousHighScore = PlayerPrefs.GetInt("High Score");
        scoreText.enabled = true;
        timerText.enabled = true;
    }
    
  
    public void IncrementScore(int score)
    {
        playerScore += score;
        Debug.Log(playerScore);
        scoreText.text = "Score: "+playerScore.ToString();
        timeLeft += 5;
    }

    public void ShowGameOverMenu()
    {
        gameOverMenu.SetActive(true);
        if(previousHighScore<=playerScore)
        {
            previousHighScore = playerScore;
            highScoreText.text = "New High Score: " + playerScore.ToString();
            PlayerPrefs.SetInt("High Score", playerScore);
            PlayerPrefs.Save();
        }

        else
        {
            highScoreText.text = "High Score: " + previousHighScore.ToString();
            PlayerPrefs.SetInt("High Score", previousHighScore);
            PlayerPrefs.Save();
        }
            


        scoreText.enabled = false;
        timerText.enabled = false;

    }

    public void Reset()
    {
        isTicking = false;
        
        scoreText.text = "Score: 0";
       
        playerScore = 0;
        timeLeft = 301.0f;
    }
}
