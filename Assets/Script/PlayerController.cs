using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : Photon.PunBehaviour, IPunObservable
{
	[Range(0.0f,0.5f)]
	public float m_deadZone = 0.1f;
	private Vector2 deviceRotation;
    private Player player;
//    private PhotonView photonView;

    private void Start()
    {
        player = GetComponent<Player>();
//        photonView = GetComponent<PhotonView>();
    }

    void FixedUpdate ()
	{
		if ((PhotonNetwork.inRoom && !player.photonView.isMine))
			return;
		deviceRotation = new Vector2 (Input.acceleration.x, Input.acceleration.y) * 4;
		if (deviceRotation.magnitude > 1)
			deviceRotation.Normalize ();
		if (deviceRotation.magnitude < m_deadZone)
			deviceRotation = Vector2.zero;
		player.Move (deviceRotation);
        
	}

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }
}