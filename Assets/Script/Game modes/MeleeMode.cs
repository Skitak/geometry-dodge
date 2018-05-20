using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MeleeMode : GameMode{
	public GameObject m_layoutMeleeMode;
	public GameObject m_layoutRoom;
	public Text m_winnerText;


	private Timer bouncyTimer, bombTimer;
	private int tick;

	public override void Launch(){
		tick = 0;
		bouncyTimer = new Timer (m_maxTimeSpawn * 1.5f, SpawnBouncy);
		bombTimer = new Timer (m_maxTimeSpawn * 2f, SpawnBomb);
		base.Launch ();
		GameManager.instance.menu.SwapMenuLayout (m_layoutMeleeMode);
		if (PhotonNetwork.isMasterClient) {
			PhotonNetwork.room.IsOpen = false;
		}
	}

	public override void End(){
		base.End ();
		bouncyTimer.Reset ();
		bombTimer.Reset ();
		foreach (Player p in GameManager.GetPlayers())
			if (!p.isBeingDestroyed && !p.isDead())
			m_winnerText.text = p.photonView.owner.CustomProperties["Nickname"] + " won this game!";
		Invoke ("GoToMenu", 5f);

	}

	public override void PlayerDied (Player p) {
		if (PlayerCount() == 1)
			End ();
	}


		
	protected override void Spawn(){
		if (!PhotonNetwork.isMasterClient) {
			return;
		}
		Spawner.SpawnEnemy (enemies [0].GetEnemy (), difficultyTimer.GetPercentageOftime ());
		spawnTimer.m_endTime = Mathf.Lerp (m_maxTimeSpawn, m_minTimeSpawn, difficultyTimer.GetPercentageOftime ());
		spawnTimer.ResetPlay ();
		++tick;
		if (tick == 3)
			SpawnBouncy ();
		else if (tick == 6)
			SpawnBomb ();
		else if (tick > 15 && tick % 5 == 0)
			Spawn ();

	}

	public override void OnDisconnectedFromPhoton () {
		if (state == State.PLAYING)
			End ();
	}

	public override void OnLeftRoom (){
		base.OnLeftRoom ();
		if (state == State.PLAYING)
			End ();
	}

	public override void OnPhotonPlayerDisconnected (PhotonPlayer otherPlayer){
		base.OnPhotonPlayerDisconnected (otherPlayer);
		if (PlayerCount() == 1 && state == State.PLAYING)
			End ();
	}

	private void GoToMenu(){
		GameManager.instance.menu.SwapMenuLayout (m_layoutRoom);
		foreach (Player p in GameManager.GetPlayers())
			if (!p.isBeingDestroyed && p.isDead())
				p.Resurrect ();
		m_winnerText.text = "";
		if (PhotonNetwork.isMasterClient) {
			PhotonNetwork.room.IsOpen = true;
		}
		GameManager.instance.SwapGameMode (GameMode.Type.MENU);
	}

	private void SpawnBouncy(){
		if (!PhotonNetwork.isMasterClient) {
			return;
		}
		bouncyTimer.ResetPlay ();
		Spawner.SpawnEnemy (enemies[1].GetEnemy(), 0.3f);
	}

	private void SpawnBomb () {
		if (!PhotonNetwork.isMasterClient) {
			return;
		}
		bombTimer.ResetPlay ();
		Spawner.SpawnEnemy (enemies[2].GetEnemy(), 0.8f);
	}

	private int PlayerCount (){
		int i = 0;
		foreach (Player p in GameManager.GetPlayers()) {
			if (!p.isBeingDestroyed && !p.isDead ())
				++i;
		}
		return i;
	}
}	
