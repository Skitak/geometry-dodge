using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveEnemy : Enemy {
	public float radius;
	public float explosivePower;

	private static Queue<Enemy> enemies = new Queue<Enemy>();
	private Collider2D[] objectsInRange;

	[PunRPC]
	public override void Initialize(Vector3 pos, float rot, float speedDifficulty, float sizeDifficulty, float warning){
		base.Initialize(pos, rot, speedDifficulty, sizeDifficulty, warning);
		for (int i = 0; i < 4 ; ++i){
			Physics2D.IgnoreCollision (GameManager.instance.walls[i].GetComponent<BoxCollider2D> (), this.GetComponent<Collider2D> (), true);
		}
	}

	protected override void OnCollisionEnter2D (Collision2D other){
		if (other.gameObject.tag == "Player") {
			Explode ();
			Die ();
		}
	}

	protected override void OnTriggerEnter2D(Collider2D other){
		if (other.tag.Equals ("Wall")) {
			if (getPassed ()) {
				Explode ();
				Die ();
			}
		}
	}

	public void Explode (){
		objectsInRange = Physics2D.OverlapCircleAll (transform.position, radius);
		foreach(Collider2D projected in objectsInRange){
			if(projected.gameObject.tag == "Player" || (projected.gameObject.tag == "Enemy" && projected.gameObject.GetComponent<Enemy>().getPassed())) {
				Vector2 forceApplied = projected.transform.position - transform.position;
				float size = radius - forceApplied.magnitude;
				if (size > 0) {
					size /= radius;
					size *= explosivePower;
					forceApplied.Normalize ();
					forceApplied = forceApplied * size;
					projected.GetComponent<Rigidbody2D>().AddForce (forceApplied);
				}
			}
		}
	}

	protected override void Enqueue(Enemy e){
		enemies.Enqueue (this);
	}

	protected override Enemy Dequeue (){
		return enemies.Dequeue ();
	}

	protected override ParticleSystem GetDyingBurst(){
		return GameManager.instance.enemyManager.m_bouncyDieBurst;
	}

}
