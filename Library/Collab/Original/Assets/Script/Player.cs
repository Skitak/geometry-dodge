using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Photon.PunBehaviour , IPunObservable{

    private Rigidbody2D rigid;
    [Range(2, 20)]
    public float speed_multiplicator;
	[Range(1, 20)]
	public float m_inertia;
	public PhotonView photonView;
	public GameObject deathParticle;
	public float m_respawnDelay;

	private SpriteRenderer sprite;
	private Light spotLight;
	private TrailRenderer trail;
	private int skinNumber;

    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
		sprite = GetComponent<SpriteRenderer> ();
		spotLight = GetComponentInChildren<Light> ();
		trail = GetComponent<TrailRenderer> ();
		skinNumber = GameManager.instance.playerManager.GetSkinNumber ();
		SetupSkin ();
    }
	[PunRPC]
    public void Die(){
		Instantiate (deathParticle, this.transform.position, this.transform.rotation);
		gameObject.SetActive (false);
		GameManager.PlayerDie (this);
	}

    public void Move(Vector3 direction) // ici, la gyro du téléphone est récupéré.
    {
		rigid.velocity = Vector3.Lerp(rigid.velocity, direction * speed_multiplicator,Time.deltaTime * m_inertia);
    }

	public void Resurect(){
		gameObject.SetActive (true);
		this.enabled = false;
		this.GetComponent<Renderer> ().enabled = false;
		this.GetComponent<CircleCollider2D> ().enabled = false;
		for( int i = 0; i < transform.childCount; ++i ){
			transform.GetChild(i).gameObject.SetActiveRecursively(false);
		}
		StartCoroutine ("RespawnDelay");
	}

	public IEnumerator RespawnDelay(){
		yield return new WaitForSeconds (m_respawnDelay);

		transform.position = Vector3.zero;
		this.enabled = true;
		this.GetComponent<Renderer> ().enabled = true;
		this.GetComponent<CircleCollider2D> ().enabled = true;
		for( int i = 0; i < transform.childCount; ++i ){
			transform.GetChild(i).gameObject.SetActiveRecursively(true);
		}
		SetupSkin ();
	}

	public bool isDead(){
		return !this.gameObject.GetActive ();	//Might change later
	}
	[PunRPC]
	public void SwapSkin(int skinNumber){
		this.skinNumber = skinNumber;
		this.
		SetupSkin ();
	}

	private void SetupSkin (){
		sprite.sprite = GameManager.instance.playerManager.sprites[skinNumber];
		trail.colorGradient= GameManager.instance.playerManager.trails[skinNumber];
		spotLight.color = GameManager.instance.playerManager.colors[skinNumber];

		//Particle modifications here

		ParticleSystem particles = GameManager.instance.playerManager.particles[skinNumber] ;
		Destroy(transform.GetComponentInChildren<ParticleSystem>().gameObject);
		GameObject particlesObject = null;
		if (PhotonNetwork.inRoom) {
			particlesObject = PhotonNetwork.Instantiate (particles.gameObject.name, Vector3.zero, Quaternion.identity, 0) ;
		} else {
			particlesObject = Instantiate (particles.gameObject, Vector3.zero, Quaternion.identity) as GameObject ;
		}
		particlesObject.transform.parent = transform;
		particlesObject.transform.localPosition = Vector3.zero;
		particlesObject.transform.localScale = Vector3.one;
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info){}
}
