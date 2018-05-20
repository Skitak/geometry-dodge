using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Lobby : Photon.PunBehaviour
{

    public GameObject m_roomModel;
    public float m_offset;
    public Menu m_menu;

    [Space(10)]
    public GameObject m_roomContainer;
    public GameObject m_noRoomFoundText;
    public GameObject m_createRoom;
    [Tooltip("Groupe of UI elements when currently in a room")]
    public GameObject m_inRoom;
    public GameObject m_playButton;
	public GameObject m_loadingCreateRoom;
	public GameObject m_loadingConnection;
    private float currentOffset;
    private List<GameObject> roomButtons = new List<GameObject>();

    private void Start()
    {
        if (!PhotonNetwork.connected)
        {
            PhotonNetwork.JoinLobby();
        }
    }

    public void JoinLobby()
    {
		if (PhotonNetwork.inRoom)
			m_menu.SwapMenuLayout (m_inRoom);
		else {
			m_loadingConnection.SetActive (true);
			LoadRooms ();
		}
    }

    public void LoadRooms()
    {
        m_noRoomFoundText.SetActive(false);
		Rect rectRoomModel = m_roomModel.GetComponent<RectTransform>().rect;
		RectTransform rectContainer = m_roomContainer.GetComponent<RectTransform> ();
        foreach (GameObject button in roomButtons)
            Destroy(button);
        roomButtons.Clear();
		foreach (RoomInfo roomInfo in PhotonNetwork.GetRoomList()) {
			if (roomInfo.IsOpen) {
				rectContainer.sizeDelta += new Vector2 (0, rectRoomModel.height);
				Setup (roomInfo);
			}
		}
        if (roomButtons.Count == 0){
            m_noRoomFoundText.SetActive(true);
        }
        currentOffset = 0;
    }

    private void Setup (RoomInfo roomInfo){
        GameObject room = Instantiate(m_roomModel, m_roomContainer.transform, false);
		ExitGames.Client.Photon.Hashtable table = roomInfo.CustomProperties;
		room.GetComponentInChildren<Text> ().text =(string) table ["nickname"];
		room.transform.localScale = Vector3.one;
        room.GetComponent<RectTransform>().localPosition = new Vector3(0, 0 - currentOffset, 0);
        currentOffset += m_offset;
        room.GetComponent<RoomButton>().SetRoom(roomInfo);
        //TODO Add players info, ping, current room color and stuff.
        roomButtons.Add(room);
    }

    public void CreateRoom() {
		m_loadingCreateRoom.SetActive (true);
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.EmptyRoomTtl = 0;
		roomOptions.MaxPlayers = 4;
        roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable();
		roomOptions.CustomRoomProperties.Add("nickname", GameManager.instance.playerManager.GetNickname());
		roomOptions.CustomRoomPropertiesForLobby = new string[] {"nickname"};
        PhotonNetwork.CreateRoom(null, roomOptions, null ) ;
    }

    public void QuitRoom(){
		PhotonNetwork.LeaveRoom ();
	}

    //PUN callbacks

    public override void OnJoinedLobby() {
        Invoke("LoadRooms", 0.5f);
    }

	public override void OnJoinedRoom() {
		base.OnJoinedRoom();

		m_menu.SwapMenuLayout(m_inRoom);
		m_loadingCreateRoom.SetActive (false);
		if (PhotonNetwork.isMasterClient)
			m_playButton.SetActive(true);
		else m_playButton.SetActive(false);

		GameManager.instance.CreateMultiplayerInstance ();
    }

    public override void OnMasterClientSwitched(PhotonPlayer newMasterClient){
		if (PhotonNetwork.inRoom) {
			PhotonNetwork.LeaveRoom ();
			GameManager.instance.menu.Enter ();
		}
    }

    public override void OnLeftRoom() {
        base.OnLeftRoom();
		GameManager.instance.CreateSoloInstance ();
    }

	public override void OnConnectedToPhoton (){
		base.OnConnectedToPhoton ();
		m_loadingConnection.SetActive (false);
	}

	public override void OnDisconnectedFromPhoton (){
		base.OnDisconnectedFromPhoton ();
		m_loadingConnection.SetActive (true);
	}

	public override void OnConnectionFail (DisconnectCause cause){
		m_loadingConnection.SetActive (false);
	}

	public override void OnReceivedRoomListUpdate (){
		base.OnReceivedRoomListUpdate ();
		LoadRooms ();
	}

	public override void OnCreatedRoom (){
		base.OnCreatedRoom ();
		m_loadingCreateRoom.SetActive (false);
	}

	public override void OnPhotonCreateRoomFailed (object[] codeAndMsg){
		base.OnPhotonCreateRoomFailed (codeAndMsg);
		m_loadingCreateRoom.SetActive (false);
	}
}

