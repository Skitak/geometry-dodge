using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PlayerManager : MonoBehaviour {

	public ParticleSystem dyingParticles;
	public Sprite[] sprites;
	public ParticleSystem[] particles;
	public Gradient[] trails;
	public Color[] colors;

	private JSONObject playerStats;

	private void Start(){
		ReadFromFile ();
		UpdateDB ();
		if (GetId () == 0)
			SetId (CreateNewID ());
	}
	
	public void SwapSkin (int i) {
		if (PhotonNetwork.inRoom)
			GameManager.GetPlayers () [0].photonView.RPC ("SwapSkin", PhotonTargets.All, i);
		else {
			GameManager.GetPlayers () [0].SwapSkin (i);
		}
		playerStats.SetField("skin", i);
		UpdateFile ();
		UpdateDB ();
	}

	public int GetSkinNumber() {
		int value = 0;
		playerStats.GetField(ref value,"skin");
		return value; 
	}

	public int GetScore () {
		int value = 0;
		playerStats.GetField(ref value,"score");
		return value; 
	}

	public string GetNickname () {
		string value = "";
		playerStats.GetField(ref value,"pseudo");
		return value; 
	}

	private int GetId () {
		int value = 0;
		playerStats.GetField (ref value, "id");
		return value;
	}

	public void SetScore(int score) {
		if (GetScore () > score)
			return;
		playerStats.SetField ("score",score);
		UpdateFile ();
		UpdateDB ();
	}

	public void SetNickname(string pseudo) {
		playerStats.SetField ("pseudo",pseudo);
		UpdateFile ();
		UpdateDB ();
	}

	private void SetId(int id) {
		playerStats.SetField ("id",id);
		UpdateFile ();
		UpdateDB ();
	}

	private void ReadFromFile(){ //sur pc, : C:/User/Bastien/AppData/LocalLow/PJS4Company/Geometry dodgePlayerInfos.txt
		StreamReader streamReader = new StreamReader (File.Open (Application.persistentDataPath + "Player Infos.txt", FileMode.OpenOrCreate, FileAccess.Read));
		string file = streamReader.ReadToEnd ();
		playerStats = new JSONObject (file);
		streamReader.Close ();
		if (!playerStats.HasField ("id"))
			FillPlayerStatsInitially ();
	}

	private void UpdateDB(){
		if (GetId () == 0)
			return;
		WWWForm form = new WWWForm();
		form.AddField ("id", GetId());
		form.AddField ("pseudo", GetNickname());
		form.AddField ("score", GetScore());
		new WWW ("http://geometry-dodge.co.nf/updateStats.php", form);
	}

	private void UpdateFile (){
		StreamWriter streamWriter = new StreamWriter (Application.persistentDataPath + "Player Infos.txt", false,System.Text.Encoding.Default);
		streamWriter.Write (playerStats.ToString());
		streamWriter.Close ();

	}

	private int CreateNewID (){
		try{
			WWW connection = new WWW("http://geometry-dodge.co.nf/connection.php");
			while (!connection.isDone);
			return int.Parse(connection.text);
		}catch {
			return 0;
		}
	}

	private void FillPlayerStatsInitially(){
		playerStats = new JSONObject ();
		playerStats.AddField ("id", CreateNewID ());
		playerStats.AddField ("skin", 0);
		playerStats.AddField ("pseudo", "Peon");
		playerStats.AddField ("score", 0);
		UpdateFile ();
	}
}
