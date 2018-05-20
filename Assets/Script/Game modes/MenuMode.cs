using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuMode : GameMode {

	public Menu menu;
	private Queue queue = new Queue();
	public override void Launch(){
		base.Launch ();
		menu.Enter ();
	}

	public override void End(){
		base.End ();

		Debug.Log ("end");
	}

	public override void PlayerDied(Player p){
		queue.Enqueue (p);
		Invoke ("Resurrect", 2.0f);
	}

	protected override void Spawn (){
		Debug.Log ("Menu");
		if (!PhotonNetwork.isNonMasterClientInRoom) {
			base.Spawn ();
		}
	}

	private void Resurrect (){
		((Player)queue.Dequeue ()).Resurrect();
	}

}
