using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour {

    private string gameVersion = "0.1";
    private static NetworkManager networkManager;

	void Awake () {
        PhotonNetwork.autoJoinLobby = false;
        PhotonNetwork.automaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings(gameVersion);
	}

    private void Start(){
        networkManager = this;
    }

    public static void Connect()
    {
		if (!PhotonNetwork.connected)
        	PhotonNetwork.ConnectUsingSettings(networkManager.gameVersion);
    }

	public static void Disconnect(){
		if (PhotonNetwork.inRoom)
			PhotonNetwork.LeaveRoom();
		if (PhotonNetwork.connected ||PhotonNetwork.connecting)
			PhotonNetwork.Disconnect();
	}

}
