using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuAnim : MonoBehaviour {

	public Animator[] m_buttonAnimators;

	private bool[] animState;

	public void Enter(){
		gameObject.SetActive (true);
		foreach (Animator a in m_buttonAnimators) {
			if (a.gameObject.activeSelf)
			a.Play("Start");
		}
	}

	public void Exit (){
		gameObject.SetActive (false);
	}
}
