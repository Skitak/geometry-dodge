using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Photon.PunBehaviour , IPunObservable{

	private Rigidbody2D rigid;
	[Range(2, 20)]
	public float speed_multiplicator;
	[Range(0, 1)]
	public float m_minInertia;
	public PhotonView photonView;
	[Range(1,50)]
	public float maxSpeed;
	//public AudioSource deathSound;

	private ParticleSystem m_dyingBurst;
	private SpriteRenderer sprite;
	private Light spotLight;
	private TrailRenderer trail;
	[HideInInspector]
	public bool isBeingDestroyed = false;
	private int skinNumber = 0;

	void Start() {
		rigid = GetComponent<Rigidbody2D>();
		sprite = GetComponent<SpriteRenderer> ();
		spotLight = GetComponentInChildren<Light> ();
		trail = GetComponent<TrailRenderer> ();
		m_dyingBurst = GameManager.instance.playerManager.dyingParticles;
		if (!PhotonNetwork.inRoom || photonView.isMine) {
			skinNumber = GameManager.instance.playerManager.GetSkinNumber ();
		}
		else 
			skinNumber = int.Parse((string) photonView.owner.CustomProperties["Skin"]);
		GameManager.instance.AddPlayer (this);
		SetupSkin ();
	}

	[PunRPC]
	public void Die(){
		if (isBeingDestroyed)
			return;
		//AudioSource.PlayClipAtPoint(deathSound.clip, transform.position);
		m_dyingBurst.transform.position = transform.position;
		ParticleSystem.MainModule main = m_dyingBurst.main;
		main.startColor = GameManager.instance.playerManager.colors[skinNumber];
		m_dyingBurst.Play ();
		gameObject.SetActive (false);
		GameManager.PlayerDie (this);
	}

	public void Move(Vector3 direction) {
		if (isBeingDestroyed)
			return;
		if (rigid == null)
			return;
		rigid.velocity = Vector3.Lerp(rigid.velocity, direction * speed_multiplicator, m_minInertia);
		if(maxSpeed < rigid.velocity.magnitude)
			rigid.velocity = rigid.velocity.normalized * maxSpeed;
	}

	public void Resurrect(){
		gameObject.SetActive (true);
		SetupSkin ();
	}

	public bool isDead(){
		return !this.gameObject.GetActive ();	//Might change later
	}

	[PunRPC]
	public void SwapSkin(int skinNumber){
		this.skinNumber = skinNumber;
		SetupSkin ();
	}

	private void SetupSkin (){
		sprite.sprite = GameManager.instance.playerManager.sprites[skinNumber];
		ParticleSystem particles = GameManager.instance.playerManager.particles[skinNumber] ;
		trail.colorGradient= GameManager.instance.playerManager.trails[skinNumber];
		spotLight.color = GameManager.instance.playerManager.colors[skinNumber];

		//Particle modifications here
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

	[PunRPC]
	public void LaunchMeleeMode () {
		if (photonView.isMine)
			GameManager.instance.SwapGameMode (GameMode.Type.MELEE);
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info){}

}
