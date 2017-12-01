using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour {

public void NewGame() {
    SceneManager.LoadScene("WorldScreen", LoadSceneMode.Single);
}

public void QuitGame() {
    Application.Quit();
}

}
