using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Enemy : Photon.PunBehaviour{

	public float m_minSpeed = 8f;
	public float m_maxSpeed = 12f;
	[Space(10)]
	public float m_maxSize = 1.5f;
	[Space(20)]
	public Color[] m_colors;
	public Sprite[] m_bodies;
	public Sprite[] m_outlines;
	public ParticleSystem m_movingParticles;
	public GameObject m_warning;

	public PhotonView photonView;

	private Rigidbody2D rigid;
	private Timer securityDeathTimer;
	private Light spotLight = null;
	private SpriteRenderer spriteBody, spriteOutline, spriteWarning;
	private float speed = 1f;
	private bool canDie = false,isBeingDestroyed = false;
	private static Queue<Enemy> enemies = new Queue<Enemy>();
	protected virtual void Awake(){
		lock (this) {
			securityDeathTimer = new Timer (10f, Die);
			rigid = GetComponent<Rigidbody2D> ();
			spotLight = GetComponentInChildren<Light> ();
			spriteBody = transform.GetChild (0).GetComponent<SpriteRenderer> ();
			spriteOutline = GetComponent<SpriteRenderer> ();
			spriteWarning = m_warning.GetComponent<SpriteRenderer> ();
		}
	}

	[PunRPC]
	public virtual void Initialize(Vector3 pos, float rot, float speedDifficulty, float sizeDifficulty, float warning) {
		lock (this) {
			if (isBeingDestroyed)
				return;
			gameObject.SetActive (true);
			//Color initialization
			int color = Random.Range (0, m_colors.Length);
			int spriteNumber = Random.Range (0, m_bodies.Length);
			spriteBody.color = m_colors [color];
			spriteBody.sprite = m_bodies [spriteNumber];
			spriteOutline.sprite = m_outlines [spriteNumber];
			spotLight.color = m_colors [color];
			spriteWarning.color = m_colors [color];
			ParticleSystem.MainModule main = m_movingParticles.main;
			main.startColor = m_colors [color];

			speed = Mathf.Lerp (m_minSpeed, m_maxSpeed, speedDifficulty);
			gameObject.transform.position = pos;
			float size = Mathf.Lerp (1f, m_maxSize, sizeDifficulty);
			gameObject.transform.localScale = Vector3.one * size;
			gameObject.SetActive (true);
			rigid.rotation = rot;
			m_warning.SetActive (true);
			securityDeathTimer.ResetPlay ();
			Invoke ("Launch", warning);
		}
	}
		
	protected virtual void OnTriggerEnter2D(Collider2D other){
		if (other.tag.Equals ("Wall")) {
			if (canDie)

				Die ();
		}
	}

	protected virtual void OnTriggerExit2D (Collider2D other){
		if (other.tag.Equals ("Wall")) {
			canDie = true;

		}
	}

	[PunRPC]
	protected virtual void Die () {
		if (isBeingDestroyed)
			return;
		//initialize again

		securityDeathTimer.Pause();
		ParticleSystem dyingBurst = GetDyingBurst ();
		dyingBurst.gameObject.transform.position = transform.position;
		ParticleSystem.MainModule emission = dyingBurst.main;
		emission.startColor = spriteBody.color;
		dyingBurst.Play ();
		canDie = false;
		transform.position = new Vector3 (100, 100, 0);
		rigid.velocity = Vector3.zero;
		transform.rotation = Quaternion.identity;
		transform.localScale = Vector3.one;
		gameObject.SetActive (false);
		Enqueue (this);
	}

	protected virtual ParticleSystem GetDyingBurst(){
		return GameManager.instance.enemyManager.m_enemyDieBurst;
	}

	protected virtual void OnCollisionEnter2D (Collision2D other) {
		if (other.gameObject.tag == "Player") {
			if (PhotonNetwork.inRoom) {
				other.gameObject.GetComponent<Player> ().photonView.RPC("Die",PhotonTargets.All);
				photonView.RPC ("Die", PhotonTargets.All);
			} else {
				other.gameObject.GetComponent<Player> ().Die ();
				Die ();
			}
		} else if (other.gameObject.tag.Equals("Enemy") && canDie) {
			if (PhotonNetwork.inRoom) {
				photonView.RPC ("Die", PhotonTargets.All);
			} else {
				Die ();
			}
		}
	}

	[PunRPC]
	public virtual Enemy GetEnemy(){
		Enemy enemy = null;
		try {
			enemy = Dequeue ();

		} catch {
			if (!PhotonNetwork.inRoom) {
				enemy = Instantiate (Resources.Load (this.name) as GameObject,
					new Vector3 (100, 100, 0), Quaternion.identity).GetComponent<Enemy> ();
			} else {
				enemy = PhotonNetwork.Instantiate (this.name,
					new Vector3 (100, 100, 0), Quaternion.identity, 0).GetComponent<Enemy> ();
			}
		}

		return enemy;
	}

	protected virtual void Enqueue(Enemy e){
		enemies.Enqueue (e);
	}

	protected virtual Enemy Dequeue(){
		return enemies.Dequeue ();
	}

	private void Launch(){
		m_warning.SetActive (false);
		rigid.velocity = gameObject.transform.up * speed;
	}

	public override void OnLeftRoom(){
		base.OnLeftRoom ();
		isBeingDestroyed = true;
		Destroy (this.gameObject);
	}

	public override void OnJoinedRoom(){
		base.OnJoinedRoom ();
		isBeingDestroyed = true;
		Destroy (this.gameObject);
	}

	
	public bool getPassed(){
		return canDie;
	}

	public void setPassed(bool pass){
		this.canDie = pass;
	}

	public Rigidbody2D getRigid(){
		return rigid;
	}

	public void setRigid(Rigidbody2D r2D){
		this.rigid = r2D;
	}

	public static Queue<Enemy> getListEnemy(){
		return enemies;
	}

	public void ClearList(){
		lock (enemies) {
			foreach (Enemy e in getListEnemy())
				if (!e.isBeingDestroyed) {
					e.isBeingDestroyed = true;
					Destroy (e.gameObject);
				}
			getListEnemy().Clear ();
		}
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info){}
}
