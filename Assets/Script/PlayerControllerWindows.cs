using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControllerWindows : MonoBehaviour {

	private Player player;

	private void Start()
	{
		player = GetComponent<Player>();
	}

	void FixedUpdate(){
		if ((PhotonNetwork.inRoom && !player.photonView.isMine))
			return;
		Vector3 mousePosition = Camera.main.ScreenToWorldPoint (Input.mousePosition);
		mousePosition.z = 0;
		Vector3 truePosition = mousePosition - transform.position;
		if (truePosition.magnitude > 1)
			truePosition.Normalize ();
		player.Move (truePosition);
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info){}
}
