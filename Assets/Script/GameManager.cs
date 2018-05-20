using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
	
	public int m_numberOfPlayers = 4;
	public Text m_debugText;
	public GameObject[] walls;
	public PlayerManager playerManager;
	public Menu menu;
	public EnemyManager enemyManager;
	[Space(20)]
	private MenuMode menuMode;
	private SurvivalMode survivalMode;
	private MeleeMode meleeMode;


	public static GameManager instance;
	private List<Player> players;
	private GameMode currentMode;

    void Start(){
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
		instance = this;
		menuMode = GetComponent<MenuMode> ();
		survivalMode = GetComponent<SurvivalMode> ();
		meleeMode = GetComponent<MeleeMode> ();
		players = new List<Player>();
		CreateSoloInstance ();
		currentMode = menuMode;
		currentMode.Launch ();

    }

	public void SwapGameMode (GameMode.Type gameMode){
		if (currentMode != null && currentMode == menuMode)
			currentMode.End ();
		switch (gameMode) {
		case GameMode.Type.MELEE:
			currentMode = meleeMode;
			break;
		case GameMode.Type.SURVIVAL:
			if (PhotonNetwork.inRoom)
				PhotonNetwork.LeaveRoom ();
			currentMode = survivalMode;
			break;
		case GameMode.Type.MENU:
		default:
			currentMode = menuMode;
			break;			
		}

		currentMode.Launch ();
	}

	public void LaunchMeleeMode () { //That is necessary, even though we have already a method for this..
		if (players.Count > 1) {
			foreach (Player p in players)
				p.photonView.RPC ("LaunchMeleeMode", PhotonTargets.All);
		}
	}

	public void SwapGameMode(int gameModeIndex){
		SwapGameMode((GameMode.Type) gameModeIndex);
	}

	public static List<Player> GetPlayers(){
		return instance.players;
	}

	public static void PlayerDie(Player p){
		instance.currentMode.PlayerDied (p);
	}

	public void CreateSoloInstance (){
		Player old = GetMyPlayer ();
		players.Clear ();
		if (old == null)
			Instantiate (Resources.Load("Player") as GameObject, Vector3.zero, Quaternion.identity);
		else {
			Instantiate (Resources.Load("Player") as GameObject, old.transform.position, Quaternion.identity);
		}
	}

	public void AddPlayer (Player p){
		players.Add (p);
	}

	public void RemovePlayer (Player p){
		players.Remove (p);
	}

	public Player GetMyPlayer (){
		foreach (Player p in players) {
			if (!PhotonNetwork.inRoom || p.photonView.isMine)
				return p;
		}
		return null;
	}

	public void CreateMultiplayerInstance(){
		ExitGames.Client.Photon.Hashtable table = new ExitGames.Client.Photon.Hashtable ();
		table.Add ("Nickname", GameManager.instance.playerManager.GetNickname());// <--- Here put the right nickname
		table.Add ("Skin", GameManager.instance.playerManager.GetSkinNumber ().ToString ());
		PhotonNetwork.SetPlayerCustomProperties (table);
		Player old = players[0];
		RemovePlayer (old);
		old.isBeingDestroyed = true;
		Destroy (old.gameObject);
		PhotonNetwork.Instantiate ("Player", Vector3.zero, Quaternion.identity, 0);

	}
}
