using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    private void Update() {
        if(Input.GetKeyDown(KeyCode.Escape)){
            SceneManager.LoadScene(0);
        }
    }
    public void StartMenu(){
        SceneManager.LoadScene(0);
    }
    public void Start2Player(){
        SceneManager.LoadScene(1);
    }
    public void Multiplayer(){
        SceneManager.LoadScene(2);
    }
    public void QuitGame(){
        Application.Quit();
    }
}
