using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NetWorkUI : MonoBehaviour
{
    public Button startHost;
    public Button startClient;
    public Button startGame;
    private void Update() {
        if(Input.GetKeyDown(KeyCode.Escape)){
            SceneManager.LoadScene(0);
        }
    }
    public void StartHost(){
        NetworkManager.Singleton.StartHost();
        startHost.gameObject.SetActive(false);
        startClient.gameObject.SetActive(false);
        startGame.gameObject.SetActive(true);

    }
    public void StartGame(){
        GameObject.Find("GameManager").GetComponent<GameManager>().StartGameBtn();
        startGame.gameObject.SetActive(false);
    }
    public void StartClient(){
        NetworkManager.Singleton.StartClient();
        startHost.gameObject.SetActive(false);
        startClient.gameObject.SetActive(false);
    }
    public void Exit(){
        SceneManager.LoadScene(0);
    }
}
