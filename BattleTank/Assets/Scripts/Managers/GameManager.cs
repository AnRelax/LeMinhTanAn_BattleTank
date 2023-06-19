using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.Netcode;

public class GameManager : NetworkBehaviour
{
    public int m_NumRoundsToWin = 5;
    public float m_StartDelay = 3f; 
    public float m_EndDelay = 3f;
    public float m_TimeSpawnTransport=15f;
    public float m_timer;
    public float positionX;
    public float positionZ; 
    public CameraControl m_CameraControl;
    public Text m_MessageText;              
    public GameObject[] m_CountPlayers;        
    public TankManager[] m_Tanks; 
    public Vector3 m_SpawnPoint;
    public Quaternion m_SpawnRotaion;
    private int m_RoundNumber;  
    private WaitForSeconds m_StartWait; 
    private WaitForSeconds m_EndWait;
    private TankManager m_RoundWinner;
    private TankManager m_GameWinner; 
    public bool StartGame;

    const float k_MaxDepenetrationVelocity = float.PositiveInfinity;
 
    private void Start()
    {
        m_StartWait = new WaitForSeconds (m_StartDelay);
        m_EndWait = new WaitForSeconds (m_EndDelay);
        if(!IsServer)
            return;
        positionX=Random.Range(-58,-35);
        positionZ=Random.Range(-58,-35);    
    }

    private void Update() {
        m_CountPlayers=GameObject.FindGameObjectsWithTag("Player");
        if(m_CountPlayers.Length>0 && !StartGame)
        {
            SetAllTankClientRpc();
            SetCameraTargetsClientRpc();
        }
        if(!IsServer)
            return;
        if(StartGame)
        {
            m_SpawnPoint=new Vector3(positionX,25,positionZ);
            m_timer+=Time.deltaTime;
            if(m_timer>=m_TimeSpawnTransport)
            {
                m_timer=0;
            }    
        }
    }

    public void StartGameBtn()
    {
        if(!IsServer)
            return;
        StartGameBtnClientRpc();
    }

    [ClientRpc]
    private void StartGameBtnClientRpc()
    {
        if(m_CountPlayers.Length>1)
        {
            StartGame=true;
            StartCoroutine(GameLoop());
        }
    }

    // [ClientRpc]
    // private void SpawnTransportClientRpc()
    // {
    //     Instantiate(m_Transport,m_SpawnPoint,m_SpawnRotaion);
    //     positionX=Random.Range(-58,-35);
    //     positionZ=Random.Range(-58,-35);
    // }

    [ClientRpc]
    private void SetAllTankClientRpc()
    {
        SpawnAllTanks();
    }

    [ClientRpc]
    private void SetCameraTargetsClientRpc()
    {
        SetCameraTargets();
    }
    
    private void SpawnAllTanks()
    {
        for (int i = 0; i < m_CountPlayers.Length; i++)
        {
            m_Tanks[i].m_Instance = m_CountPlayers[i];
            m_Tanks[i].m_PlayerNumber = i + 1;
            m_Tanks[i].Setup();
        }
    }

    private void SetCameraTargets()
    {
        Transform[] targets = new Transform[m_CountPlayers.Length];

        for (int i = 0; i < targets.Length; i++)
        {
            targets[i] = m_Tanks[i].m_Instance.transform;
        }

        m_CameraControl.m_Targets = targets;
    }

    [ClientRpc]
    private void LoadSceneClientRpc()
    {
        NetworkManager.Singleton.Shutdown();
        SceneManager.LoadScene(0);
    }

    private IEnumerator GameLoop ()
    {
        yield return StartCoroutine (RoundStarting());
        yield return StartCoroutine (RoundPlaying());
        yield return StartCoroutine (RoundEnding());

        if (m_GameWinner != null)
        {
            NetworkManager.Singleton.StopAllCoroutines();
            LoadSceneClientRpc();
        }
        else
        {
            StartCoroutine (GameLoop());
        }
    }

    private IEnumerator RoundStarting ()
    {
        ResetAllTanks();
        DisableTankControl();
        m_CameraControl.SetStartPositionAndSize();
        m_RoundNumber++;
        m_MessageText.text = "ROUND " + m_RoundNumber;

        yield return m_StartWait;
    }

    private IEnumerator RoundPlaying ()
    {
        EnableTankControl();
        m_MessageText.text = string.Empty;
        while (!OneTankLeft())
        {
            yield return null;
        }
    }

    private IEnumerator RoundEnding ()
    {
        DisableTankControl();
        m_RoundWinner = null;
        m_RoundWinner = GetRoundWinner();
        if (m_RoundWinner != null){
            m_RoundWinner.m_Wins++;
        }  

        m_GameWinner = GetGameWinner();
        string message = EndMessage();
        m_MessageText.text = message;

        yield return m_EndWait;
    }


    private bool OneTankLeft()
    {
        int numTanksLeft = 0;
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            if (m_Tanks[i].m_Instance.activeSelf){
                 numTanksLeft++;
            }
        }

         return numTanksLeft <= 1;
    }
        
        
    private TankManager GetRoundWinner()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            if (m_Tanks[i].m_Instance.activeSelf){
                return m_Tanks[i];
            }
        }

        return null;
    }


    private TankManager GetGameWinner()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            if (m_Tanks[i].m_Wins == m_NumRoundsToWin){
                return m_Tanks[i];
            }
        }

         return null;
    }

    private string EndMessage()
    {
        string message = "DRAW!";
        if (m_RoundWinner != null){
            message = m_RoundWinner.m_ColoredPlayerText + " WINS THE ROUND!";
        }
        message += "\n\n\n\n";
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            message += m_Tanks[i].m_ColoredPlayerText + ": " + m_Tanks[i].m_Wins + " WINS\n";
        }
        if (m_GameWinner != null)
            message = m_GameWinner.m_ColoredPlayerText + " WINS THE GAME!";
        return message;
    }

    private void ResetAllTanks()
    {
        for (int i = 0; i < m_Tanks.Length; i++){
            m_Tanks[i].Reset();
        }
    }

    private void EnableTankControl()
    {
        for (int i = 0; i < m_Tanks.Length; i++){
            m_Tanks[i].EnableControl();
        }
    }

    private void DisableTankControl()
    {
        for (int i = 0; i < m_Tanks.Length; i++){
            m_Tanks[i].DisableControl();
        }
    }
}