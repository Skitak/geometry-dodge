using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SurvivalMode : GameMode{

	public Text m_text;

	private float timeInGame;
	private Timer bouncyTimer, bombTimer;
	private int tick;

	void Update() {
		if (difficultyTimer != null && m_text != null && state == State.PLAYING) {
			timeInGame += Time.deltaTime;
			m_text.text = ((int) timeInGame).ToString();
		}
	}

	public override void Launch(){
		tick = 0;
		timeInGame = 0;
		bouncyTimer = new Timer (m_maxTimeSpawn * 1.5f, SpawnBouncy);
		bombTimer = new Timer (m_maxTimeSpawn * 2f, SpawnBomb);
		base.Launch ();
		Spawn ();
	}

	public override void End(){
		base.End ();
		bouncyTimer.Reset ();
		bombTimer.Reset ();

		Invoke ("SwapLayout",2f);

	}

	protected override void Spawn(){
		Spawner.SpawnEnemy (enemies[0].GetEnemy(), difficultyTimer.GetPercentageOftime ());
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

	public override void PlayerDied(Player p){
		End ();
		Invoke ("Resurrect", 2.0f);
	}

	private void Resurrect(){
		GameManager.instance.GetMyPlayer ().Resurrect ();
	}

	private void SpawnBouncy(){
		bouncyTimer.m_endTime =  Mathf.Lerp (m_maxTimeSpawn, m_minTimeSpawn, difficultyTimer.GetPercentageOftime ()) * 2f;
		bouncyTimer.ResetPlay ();
		Spawner.SpawnEnemy (enemies[1].GetEnemy(), difficultyTimer.GetPercentageOftime());
	}

	private void SpawnBomb () {
		bombTimer.m_endTime =  Mathf.Lerp (m_maxTimeSpawn, m_minTimeSpawn, difficultyTimer.GetPercentageOftime ()) * 2f;
		bombTimer.ResetPlay ();
		Spawner.SpawnEnemy (enemies[2].GetEnemy(), difficultyTimer.GetPercentageOftime());
	}

	private void SwapLayout(){
		GameManager.instance.SwapGameMode (GameMode.Type.MENU);
		GameManager.instance.menu.Enter ();
		GameManager.instance.playerManager.SetScore ((int) timeInGame);
	}
}
