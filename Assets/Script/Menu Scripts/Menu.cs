using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : Photon.PunBehaviour {
    public GameObject m_activeLayoutAtStart;
    public GameObject m_connectedLayout;
    public GameObject m_offlineLayout;
    public GameObject m_multiplayerLayout;
	public GameObject m_container;
	public GameObject m_prefabTextScore;
	public InputField m_nicknameField;
	public Text m_errorText, m_playerScore;
    public Lobby m_lobby;
    private GameObject activeLayout;


	private void Start(){
		Invoke ("SetNicknameField", 1f);
	}

    public void SwapMenuLayout(GameObject to){
		activeLayout.GetComponent<MenuAnim>().Exit();
		to.GetComponent<MenuAnim>().Enter();
        activeLayout = to;
    }

    public void JoinLobby(GameObject lobbyLayout){
		NetworkManager.Connect ();
		if (PhotonNetwork.inRoom)
			SwapMenuLayout (m_multiplayerLayout);
		else {
			PhotonNetwork.JoinLobby ();
			SwapMenuLayout (lobbyLayout);
		}
	}

    public void Quit(){
        Application.Quit();
    }

	private void SetNicknameField (){
		((Text)m_nicknameField.placeholder).text = GameManager.instance.playerManager.GetNickname();
	}

    public override void OnConnectedToPhoton() {
		base.OnConnectedToPhoton ();
		m_connectedLayout.SetActive(true);
		m_offlineLayout.SetActive(false);
    }

    public override void OnDisconnectedFromPhoton(){
        m_connectedLayout.SetActive(false);
        m_offlineLayout.SetActive(true);
    }

	public void Enter (){
		if (activeLayout != null)
			activeLayout.SetActive(false);
		activeLayout = m_activeLayoutAtStart;
		activeLayout.SetActive(true);
		activeLayout.GetComponent<MenuAnim> ().Enter ();
	}

	public void ApplyNicknameModifs(){
		if (m_nicknameField.text.Length < 3) {
			m_errorText.text = "Nickname too short";
			m_errorText.color = Color.red;
		} else {
			m_errorText.text = "New Nickname Saved";
			m_errorText.color = Color.green;
		}
		Invoke("EndError",2f);
		GameManager.instance.playerManager.SetNickname(m_nicknameField.text);
		m_nicknameField.text = "";
		((Text) m_nicknameField.placeholder).text = GameManager.instance.playerManager.GetNickname();
	}

	private void EndError(){
		m_errorText.text = "";
	}

	public void GetScores(){
		string space = "          ";
		m_errorText.text = ""; 
		m_playerScore.text = GameManager.instance.playerManager.GetNickname () + space + GameManager.instance.playerManager.GetScore ();
		foreach (Transform t in m_container.transform.GetComponentsInChildren<Transform>())
			if (t.gameObject != m_container)
				Destroy (t.gameObject);
		Rect rectText = m_prefabTextScore.GetComponent<RectTransform> ().rect;
		RectTransform rectContainer = m_container.GetComponent<RectTransform> ();
		rectContainer.sizeDelta = new Vector2 (rectContainer.sizeDelta.x, 0);
		if (PhotonNetwork.connected) {
			WWW website = new WWW ("http://geometry-dodge.co.nf/stats.php");
			while (!website.isDone)
				;
			string file = website.text;
			JSONObject entries = new JSONObject (file);
			string nickname = "";
			string score = "";
			float futurHeight = 0f;
			for (int i = 0; i < entries.Count; ++i) {
				futurHeight += rectText.height;
				GameObject textScore = 	Instantiate (m_prefabTextScore, m_container.transform) as GameObject;
				textScore.transform.localScale = Vector3.one;
				entries[i].GetField (ref nickname, "Pseudo");
				entries[i].GetField (ref score, "Score");
				textScore.GetComponent<Text> ().text = nickname + space + score;
			}
			rectContainer.sizeDelta = new Vector2 (rectContainer.sizeDelta.x, futurHeight);
		} else {
			m_errorText.color = Color.red;
			m_errorText.text = "Internet connection needed.";
			Invoke("EndError",2f);
		}
	}
}
