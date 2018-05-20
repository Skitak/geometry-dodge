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
	public ParticleSystem m_dyingBurst;

	private Rigidbody2D rigid;
	private Light spotLight = null;
	private SpriteRenderer spriteBody;
	private SpriteRenderer spriteOutline;
	private float speed = 1f;
	private bool canDie = false;
	private static Queue<Enemy> enemies = new Queue<Enemy>();

	public virtual void Initialize(Vector3 pos, float rot, float speedDifficulty, float sizeDifficulty, float warning) {
		speed = Mathf.Lerp (m_minSpeed, m_maxSpeed, speedDifficulty);
		gameObject.transform.position = pos;
		float size = Mathf.Lerp (1f, m_maxSize, sizeDifficulty);
		gameObject.transform.localScale = Vector3.one * size;
		gameObject.SetActive (true);
		rigid.rotation = rot;
		Invoke ("Launch", warning);

	}

	public void getReady(){
		//Start
		if (rigid == null) {	
			rigid = GetComponent<Rigidbody2D> ();
			spotLight = GetComponentInChildren<Light> ();
			spriteBody = transform.GetChild (0).GetComponent<SpriteRenderer> ();
			spriteOutline = GetComponent<SpriteRenderer> ();
		}

		rigid.velocity = Vector3.zero;
		transform.rotation = Quaternion.identity;
		transform.localScale = Vector3.one;
		transform.position = new Vector3 (100,100,0);
		gameObject.SetActive (true);

		//Color initialization
		int color = Random.Range (0,m_colors.Length);
		int spriteNumber = Random.Range (0, m_bodies.Length);
		spriteBody.color = m_colors[color];
		spriteBody.sprite = m_bodies [spriteNumber];
		spriteOutline.sprite = m_outlines [spriteNumber];
		spotLight.color = m_colors[color];

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

	protected virtual void Die () {
		//initialize again
		canDie = false;
		gameObject.SetActive (false);
		enemies.Enqueue (this);

		if (m_dyingBurst != null) {
			m_dyingBurst.gameObject.transform.position = transform.position;
			ParticleSystem.EmissionModule emission = m_dyingBurst.emission;
			emission.enabled = true;
		}
	}

	protected virtual void OnCollisionEnter2D (Collision2D other) {
		if (other.gameObject.tag == "Player") {
			other.gameObject.GetComponent<Player> ().Die ();
			Die ();
		} else if (other.gameObject.tag.Equals("Enemy") && canDie) {
			Die ();
		}
	}

	public virtual Enemy GetEnemy(){
		Enemy enemy = null;
		try {
			enemy = enemies.Dequeue ();

		} catch {
			if (!PhotonNetwork.inRoom) {
				enemy = Instantiate (Resources.Load ("Enemy_classic" + Random.Range (1, 3)) as GameObject,
					new Vector3 (100, 100, 0), Quaternion.identity).GetComponent<Enemy> (); 
			} else {
				enemy = PhotonNetwork.Instantiate ("Enemy_classic" + Random.Range (1, 3),
					new Vector3 (100, 100, 0), Quaternion.identity, 0).GetComponent<Enemy> (); 
			}
		}
		enemy.getReady();
		return enemy;
	}

	private void Launch(){
		rigid.velocity = gameObject.transform.up * speed;
	}

	public override void OnLeftRoom(){
		if (enemies.Count != 0)
			ClearList ();
		Destroy (this.gameObject);
	}

	public override void OnJoinedRoom(){
		if (enemies.Count != 0)
			ClearList ();
		if (GetComponent<PhotonView> ().isMine)
			Destroy (this.gameObject);
	}
	
	public bool getPassed(){
		return canDie;
	}

	public void setPassed(bool pass){
		this.canDie = pass;
	}

	public static Queue<Enemy> getListEnemy(){
		return enemies;
	}

	private void ClearList(){
		lock (enemies) {
			foreach (Enemy e in enemies)
				Destroy (e.gameObject);
			enemies.Clear ();
		}
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info){}
}
