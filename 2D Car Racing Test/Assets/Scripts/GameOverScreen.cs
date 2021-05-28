using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverScreen : MonoBehaviour
{
    [SerializeField] GameObject gameOverScreen;
    LapCounter lapCounter;

    // Start is called before the first frame update
    void Start()
    {
        gameOverScreen.SetActive(false);
        lapCounter = GetComponent<LapCounter>();
    }

    // Update is called once per frame
    void Update()
    {
        if (lapCounter.GetIsCompleted())
        {
            DisplayGameOverScreen();

            if (Input.GetKeyDown(KeyCode.Space))
                SceneManager.LoadScene("SampleScene");
        }
    }

    private void DisplayGameOverScreen()
    {
        gameOverScreen.SetActive(true);
    }
}
