using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class TankHealth : NetworkBehaviour
{
    public float m_StartingHealth = 100f;
    public Slider m_Slider; 
    public Image m_FillImage; 
    public Color m_FullHealthColor = Color.green;
    public Color m_ZeroHealthColor = Color.red;
    public GameObject m_ExplosionPrefab;
        
    private AudioSource m_ExplosionAudio;  
    private ParticleSystem m_ExplosionParticles; 
    public float m_CurrentHealth; 
    private bool m_Dead; 

    private void Awake (){
        m_ExplosionParticles = Instantiate (m_ExplosionPrefab).GetComponent<ParticleSystem> ();
        m_ExplosionAudio = m_ExplosionParticles.GetComponent<AudioSource> ();
        m_ExplosionParticles.gameObject.SetActive (false);
    }

    private void OnEnable(){
        m_CurrentHealth = m_StartingHealth;
        m_Dead = false;
        if(IsServer){
            SetHealthUIClientRpc(m_CurrentHealth);
        } 
    }

    public void TakeDamage (float amount){
        m_CurrentHealth -= amount;

        if(IsServer){
            SetHealthUIServerRpc(m_CurrentHealth);
            //SetHealthUIClientRpc();
        }else{
            SetHealthUIServerRpc(m_CurrentHealth);
            // SetHealthUIClientRpc();
            // SetHealthUI();
        }

        if (m_CurrentHealth <= 0f && !m_Dead){
            if(IsServer){
                //OnDeathClientRpc();
                OnDeathServerRpc(m_CurrentHealth);
            }else{
                OnDeathServerRpc(m_CurrentHealth);
                //OnDeathClientRpc();
                //OnDeath();
            }
        }  
    }

    private void SetHealthUI (float CurrentHealth){
        m_Slider.value = CurrentHealth;
        m_FillImage.color = Color.Lerp (m_ZeroHealthColor, m_FullHealthColor, CurrentHealth / m_StartingHealth);
    }

    [ServerRpc]
    private void SetHealthUIServerRpc(float CurrentHealth){
        SetHealthUI(CurrentHealth);
        SetHealthUIClientRpc(CurrentHealth);
    }

    [ClientRpc]
    private void SetHealthUIClientRpc(float CurrentHealth){
        if(!IsServer){
            SetHealthUI(CurrentHealth);
        }
    }

    private void OnDeath (float CurrentHealth){
        if(CurrentHealth <= 0f){
            m_Dead = true;
            m_ExplosionParticles.transform.position = transform.position;
            m_ExplosionParticles.gameObject.SetActive (true);
            m_ExplosionParticles.Play ();
            m_ExplosionAudio.Play();
            gameObject.SetActive (false);
        }
        
    }

    [ServerRpc]
    private void OnDeathServerRpc(float CurrentHealth){
        OnDeath(CurrentHealth);
        OnDeathClientRpc(CurrentHealth);
    }

    [ClientRpc]
    private void OnDeathClientRpc(float CurrentHealth){
        if(!IsServer){
            OnDeath(CurrentHealth);
        }
        
    }
}