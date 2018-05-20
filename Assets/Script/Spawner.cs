using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class Spawner : MonoBehaviour {

	[HideInInspector]
	public SelectionMethod m_aimingSystem;

	[Space(30)]
	public float m_minWarningTime = 0.2f;
	public float m_maxWarningTime = 2.0f;

	private static Spawner spawner; // This should not be static in the futur

	void Start(){
		spawner = this;
	}

	public static void SpawnEnemy(Enemy enemy, float difficulty){

		//Génération des variables de difficulté

		float difficultyAngle = 0f;
		float difficultySpeed = 0f;
		float difficultySize = 0f;
		float difficultyWarning = 0f;

		difficulty *= Random.Range(1.5f,2f);
		difficultyAngle = Random.Range (Mathf.Max(difficulty - 1, 0),Mathf.Min(difficulty, 1));
		difficultySpeed = difficulty - difficultyAngle;
		difficultySize = Random.Range (Mathf.Max(difficulty - 1, 0),Mathf.Min(difficulty, 1));
		difficultyWarning = difficulty - difficultySize;

		//Génération de l'angle de tir

		Vector3 spawnPoint = Vector3.zero;
		Vector3 target = GetTarget ();
		Vector2 angle = new Vector2 (1-difficultyAngle, difficultyAngle);

		if (target.x < 0)
			angle.x *= -1;
		if (target.y < 0)
			angle.y *= -1;
		
		float correctedAngle = Vector2.Angle (Vector2.up, angle) * (angle.x > 0 ? -1 : 1); //Vector2.Angle return an absolute value, so we make it normal

		spawnPoint = Physics2D.Raycast (target, -1 * angle, 100f, 1 << LayerMask.NameToLayer("SpawnerWall")).point;

		if (PhotonNetwork.inRoom) {
			enemy.photonView.RPC ("Initialize", PhotonTargets.All,
				spawnPoint,
				correctedAngle,
				difficultySpeed,
				difficultySize,
				Mathf.Lerp (spawner.m_maxWarningTime, spawner.m_minWarningTime, difficultyWarning)
			);
		} else
			enemy.Initialize (
				spawnPoint,
				correctedAngle,
				difficultySpeed,
				difficultySize,
				Mathf.Lerp (spawner.m_maxWarningTime, spawner.m_minWarningTime, difficultyWarning));
	}

	private static Vector3 GetCenterPoint(){
		List<Player> players = GameManager.GetPlayers ();
		int nbPlayers = 0;
		Vector3 playersCenterPosition = Vector3.zero;

		foreach (Player p in players) {
			if (p == null)
				continue;
			if (p.isDead ())
				continue;
			++nbPlayers;
			playersCenterPosition += p.gameObject.transform.position;
		}
		playersCenterPosition /= Mathf.Max(nbPlayers, 1);
		return playersCenterPosition;
	}

	private static Vector3 GetTarget(){
		//Switch sur les méthodes
		return GetCenterPoint();
	}




	public enum SelectionMethod{
		RANDOM,
		CENTER,
	}
}
