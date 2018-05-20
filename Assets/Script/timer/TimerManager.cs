using UnityEngine;
using System.Collections;

public class TimerManager : MonoBehaviour {
	private static ArrayList timers = new ArrayList ();
	private static ArrayList timersToAdd = new ArrayList ();

	void Update () {
		foreach (Timer t in timers)
			t.m_time += Time.deltaTime;
		if (timersToAdd.Count != 0) {
			foreach (Timer t in timersToAdd)
				timers.Add (t);
			timersToAdd.Clear ();
		}
	}
		
	public static void SetupTimer(Timer t){
		timersToAdd.Add (t);
	}
}
