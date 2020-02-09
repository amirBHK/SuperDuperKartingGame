using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour {

    public void PlayGame() {
        SceneManager.LoadScene(1);
    }

    public void GoToMainMenu() {
        SceneManager.LoadScene(0);
    }

    // Start is called before the first frame update
    public void QuitGame() {
        Application.Quit();
    }
}