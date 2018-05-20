using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameMode : Photon.PunBehaviour{
	public Enemy[] enemies;

	public float m_minTimeSpawn = 1f;
	public float m_maxTimeSpawn = 10f;
	public float m_maxTimeDifficulty = 300.0f; //60sec * 5min
	public delegate void gameStateChange();

	protected Timer difficultyTimer,spawnTimer;
	protected GameMode.State state = GameMode.State.SLEEP;
	private event gameStateChange OnStartGameMode;
	private event gameStateChange OnEndGameMode;


	public virtual void Launch(){
		if (OnStartGameMode != null)
			OnStartGameMode ();
		state = GameMode.State.PLAYING;
		if (spawnTimer == null)
			spawnTimer = new Timer (m_maxTimeSpawn, Spawn);
		else
			spawnTimer.Reset ();
		spawnTimer.Play ();
		if (difficultyTimer == null)
			difficultyTimer = new Timer (m_maxTimeDifficulty);
		else
			difficultyTimer.Reset ();
		difficultyTimer.Play ();
	}

	public virtual void End (){
		if (OnEndGameMode != null)
			OnEndGameMode ();
		state = GameMode.State.END;
		spawnTimer.Reset ();
		difficultyTimer.Reset ();
	}

	public abstract void PlayerDied (Player p);

	public void AddStartEvent (gameStateChange function){
		OnStartGameMode += function;
	}

	public void AddEndEvent (gameStateChange function){
		OnEndGameMode += function;
	}

	public void RemoveStartEvent (gameStateChange function){
		OnStartGameMode -= function;
	}

	public void RemoveEndEvent (gameStateChange function){
		OnEndGameMode -= function;
	}

	protected virtual void Spawn(){
		//Difficulty is not handeled for the moment. So, it's not very usefull to put it.
		Spawner.SpawnEnemy (enemies [0].GetEnemy(), difficultyTimer.GetPercentageOftime());
		spawnTimer.m_endTime = Mathf.Lerp (m_maxTimeSpawn, m_minTimeSpawn, difficultyTimer.GetPercentageOftime());
		spawnTimer.ResetPlay ();
	}

	public override void OnJoinedRoom (){
		base.OnJoinedRoom ();
		foreach (Enemy e in enemies)
			e.ClearList ();
	}

	public override void OnLeftRoom (){
		base.OnJoinedRoom ();
		foreach (Enemy e in enemies)
			e.ClearList ();
	}

	public enum State {
		SLEEP,
		PLAYING,
		END,
	};

	public enum Type{
		MENU = 0,
		SURVIVAL = 1,
		MELEE = 2,
	};
}