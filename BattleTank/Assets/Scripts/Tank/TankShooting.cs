using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class TankShooting : NetworkBehaviour
{
    public int m_PlayerNumber = 1;
    public Rigidbody m_Shell; 
    public Transform m_FireTransform; 
    public Slider m_AimSlider; 
    public AudioSource m_ShootingAudio;
    public AudioClip m_ChargingClip; 
    public AudioClip m_FireClip; 
    public float m_MinLaunchForce = 15f;  
    public float m_MaxLaunchForce = 30f;  
    public float m_MaxChargeTime = 0.75f;  
    private string m_FireButton;     
    private float m_CurrentLaunchForce;  
    private float m_ChargeSpeed;   
    private bool m_Fired;      
    private void OnEnable(){
        m_CurrentLaunchForce = m_MinLaunchForce;
        m_AimSlider.value = m_MinLaunchForce;
    }

    private void Start (){ 
        m_FireButton = "Fire" + m_PlayerNumber;
        m_ChargeSpeed = (m_MaxLaunchForce - m_MinLaunchForce) / m_MaxChargeTime;
        m_Fired = true;
    }

    private void Update (){
        if(!IsOwner)
            return;   
        m_AimSlider.value = m_MinLaunchForce;
        if(m_CurrentLaunchForce >= m_MaxLaunchForce && !m_Fired){
            m_CurrentLaunchForce = m_MaxLaunchForce;
            m_Fired = true;
           if(IsServer){
                FireServerRpc(m_CurrentLaunchForce);
            }  
            else{
                FireServerRpc(m_CurrentLaunchForce);
            }
        }
        else if (Input.GetButtonDown(m_FireButton)){
            m_Fired = false;
            m_CurrentLaunchForce = m_MinLaunchForce;  
            m_ShootingAudio.clip = m_ChargingClip;
            m_ShootingAudio.Play ();
        }
        else if (Input.GetButton (m_FireButton) && !m_Fired){
            m_CurrentLaunchForce += m_ChargeSpeed * Time.deltaTime;
            m_AimSlider.value = m_CurrentLaunchForce;
        }
        else if (Input.GetButtonUp (m_FireButton) && !m_Fired){
            if(IsServer){
                FireServerRpc(m_CurrentLaunchForce);
            }    
            else{
                FireServerRpc(m_CurrentLaunchForce);
            }
        }
    }

    private void Fire(float launchForce)
    {
        m_Fired = true;
        Rigidbody shellRigidbody = Instantiate(m_Shell, m_FireTransform.position, m_FireTransform.rotation) as Rigidbody;
        shellRigidbody.velocity = launchForce * m_FireTransform.forward;
        m_ShootingAudio.clip = m_FireClip;
        m_ShootingAudio.Play();
        m_CurrentLaunchForce = m_MinLaunchForce;
        Debug.Log("luc ban: " + m_CurrentLaunchForce);
    }

    [ServerRpc]
    private void FireServerRpc(float launchForce)
    {
        Fire(launchForce);
        FireClientRpc(launchForce);
    }

    [ClientRpc]
    private void FireClientRpc(float launchForce)
    {
        if(!IsServer){
            Fire(launchForce);
        }
    }
}