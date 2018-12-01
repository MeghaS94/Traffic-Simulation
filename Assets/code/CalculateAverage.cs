using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimulationX;

public class CalculateAverage : MonoBehaviour 
{
	SimManager simMgr;
	public int maxRuns=5;
	public float[] bestTrafficTiming;
	float minScheduleTime, minPenalty, minWait;
	public float avgScheduleTime, avgPenalty, avgWait;
	int numTotalTrafficLights = 0;

	// Use this for initialization
	IEnumerator Start () 
	{
		yield return new WaitForSeconds (1f);

		simMgr = GameObject.Find ("SimManager").GetComponent<SimManager> ();
		int numJunctions = simMgr.junction.Length;
		for (int i = 0; i < numJunctions; i++) {
			SetSignalMask (simMgr.junction[i]);
		}

		for (int i = 0; i < numJunctions; i++) 
			numTotalTrafficLights += simMgr.junction[i].incoming.Length;

		bestTrafficTiming = new float[numTotalTrafficLights];

		StartCoroutine (doSearch ());
	}

	IEnumerator doSearch()
	{
		minPenalty = 0f;
		minScheduleTime = 0f;
		minWait = 0f;
		int numJunctions = simMgr.junction.Length;
		
		for (int i = 0; i < maxRuns; i++) {
			for (int j = 0; j < numJunctions; j++) {
				int numTrafficLights = simMgr.junction[j].incoming.Length;
				for (int k = 0; k < numTrafficLights; k++)
					simMgr.junction[j].signalMask[k].duration = Random.Range (5f, 15f);
			}

			simMgr.StartSim();
			while (simMgr.isSimRunning ())
				yield return null;
			minPenalty += simMgr.AvgTimePenalty;
			minWait += simMgr.AvgWaitTime;
			minScheduleTime += simMgr.SimTime ();

		}

		avgWait = minWait / (float)maxRuns;
		avgPenalty = minPenalty / (float)maxRuns;
		avgScheduleTime = minScheduleTime / (float)maxRuns;

	}

	void SetSignalMask(Junction jn)
	{
		int numTrafficLights = jn.incoming.Length;
		Junction.SignalMask[] signalMask = new Junction.SignalMask[numTrafficLights];
		for (int i = 0; i < numTrafficLights; i++) {
			string mask = "";
			for (int j = 0; j < numTrafficLights * 4; j += 4) {
				mask += (j/4 == i ? "1111" : "0000");
			}
			signalMask[i].mask = mask;
		}

		jn.signalMask = signalMask;
	}
}
