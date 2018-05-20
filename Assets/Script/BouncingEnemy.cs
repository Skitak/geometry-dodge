using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BouncingEnemy : Enemy {
	public int m_bounceNumber;
	private int bounceMax;
	private static Queue<Enemy> enemies = new Queue<Enemy>();
	[PunRPC]
	public override void Initialize(Vector3 pos, float rot, float speedDifficulty, float sizeDifficulty, float warning){
		base.Initialize(pos, rot, speedDifficulty, sizeDifficulty, warning);
		for (int i = 0; i < 4 ; ++i){
			Physics2D.IgnoreCollision (GameObject.Find("Manager").GetComponent<GameManager>().walls[i].GetComponent<BoxCollider2D> (), this.GetComponent<Collider2D> (), true);
		}
	}

	protected override void OnCollisionEnter2D (Collision2D other){
		base.OnCollisionEnter2D (other);
		if (other.gameObject.tag.Equals ("Wall") || other.gameObject.tag.Equals ("Enemy")) {
			if (m_bounceNumber > 0) {
				m_bounceNumber--;
			} else if (m_bounceNumber == 0) {
				Die ();
			}
		}
	}

	protected override void OnTriggerExit2D (Collider2D other){
		if (other.tag.Equals ("Wall")) {
			for (int i = 0; i < 4; ++i) {
				Physics2D.IgnoreCollision (GameObject.Find("Manager").GetComponent<GameManager>().walls[i].GetComponent<BoxCollider2D> (), this.GetComponent<Collider2D> (), false);
			}
		}
	}

	private void OnEnable(){
		m_bounceNumber = bounceMax;
		for (int i = 0; i < 4; ++i) {
			Physics2D.IgnoreCollision (GameManager.instance.walls[i].GetComponent<BoxCollider2D>(), this.gameObject.GetComponent<Collider2D>(), true);
		}
	}

	protected override void Awake ()
	{
		base.Awake ();
		bounceMax = this.m_bounceNumber;
	}

	protected override ParticleSystem GetDyingBurst(){
		return GameManager.instance.enemyManager.m_bouncyDieBurst;
	}

	protected override void Enqueue(Enemy e){
		enemies.Enqueue (this);
	}

	protected override Enemy Dequeue(){
		return enemies.Dequeue ();
	}
}
