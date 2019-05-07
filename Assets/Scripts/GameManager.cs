using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static BasePlayerController[] players;
    public static GameManager instance = null;

    public GameObject winGame;
    public Text winText;

    public GameObject loseGame;
    public Text loseText;
    // Start is called before the first frame update
    void Start()
    {
        players = FindObjectsOfType<BasePlayerController>();
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(this.gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    public void StartGame()
    {
        EnemyManager.numEnemies = 0;
        SceneManager.LoadScene("Level1");

        Debug.Log("audio should play now");
    }

    public void MainMenu()
    {
        //AudioManager audio = FindObjectOfType<AudioManager>();
        //audio.Stop("subleville");

        SceneManager.LoadScene("MainMenu");
        Debug.Log("audio should stop now");
    }

    public void GameWon(string player)
    {
        Time.timeScale = 0;
        winGame.SetActive(true);
        FindObjectOfType<AudioManager>().Stop("BG_Music");
        FindObjectOfType<AudioManager>().Play("Game_Won");
        winText.text = player + " Wins!";
    }

    public void GameLost()
    {
        Time.timeScale = 0;
        loseGame.SetActive(true);
        FindObjectOfType<AudioManager>().Stop("BG_Music");
        FindObjectOfType<AudioManager>().Play("Game_Over");
        loseText.text = "You Lose";
    }
}
