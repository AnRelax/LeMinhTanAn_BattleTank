using UnityEngine;
using Photon.Pun;
public class UIDirectionControl : MonoBehaviourPunCallbacks
{
     public bool m_UseRelativeRotation = true;

    private Quaternion m_RelativeRotation;

    private void Start ()
    {
        m_RelativeRotation = transform.parent.localRotation;
    }

    private void Update ()
    {
        if(photonView.IsMine){
            if (m_UseRelativeRotation){
                transform.rotation = m_RelativeRotation;
            }
        } 
    }
}
