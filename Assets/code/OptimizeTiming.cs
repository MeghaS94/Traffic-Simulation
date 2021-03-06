using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimulationX;

public class OptimizeTiming : MonoBehaviour 
{
	SimManager simMgr;
	public int numGenerations = 3;
	public int maxRuns=5;
	public float[] bestTrafficTiming;
	public float minCost = 10000000f;
	public float minScheduleTime, minPenalty, minWait;
	int numTotalTrafficLightStates = 0;

	public float temp;

	List<string> SigMasks = new List<string>{
		"1101110001100111",
		"1011111001100011",
		"1001110101110011",
		"1001110011101011",
		"1011110011100011",
		"1001110101100111",
		"1101110001110011",
		"1001111001101011"};
	
	// List<float> bestSimTime = new List<float> {9.45f, 8.70f, 10.96f, 10.67f, 6.87f, 6.30f, 10.66f, 8.29f};
	//**********************************
	GeneticAlgorithm myGeneticAlgorithm;
	Population p;
	List <Chromosome> chromosomes;
	List<float> fitnessValues = new List<float>();

	//**********************************
	
	// Use this for initialization
	IEnumerator Start () 
	{
		//wait for a sec to allow the full scene to awake and init
		yield return new WaitForSeconds (1f);

		//************************************************************
		myGeneticAlgorithm = new GeneticAlgorithm ();
		myGeneticAlgorithm.initialize (maxRuns);
		p = myGeneticAlgorithm.getPopulation ();
		chromosomes = new List<Chromosome> ();
		chromosomes = p.getChromosomes ();
		int numChromosomes = chromosomes.Count;
		//maxRuns = numChromosomes;
		//*************************************************************

		simMgr = GameObject.Find ("SimManager").GetComponent<SimManager> ();

		//Set up the signal masks first
		int numJunctions = simMgr.junction.Length;
		for (int i = 0; i < numJunctions; i++) {
			SetSignalMask (simMgr.junction[i]);
		}

		numTotalTrafficLightStates = 0;
		for (int i = 0; i < numJunctions; i++) 
			numTotalTrafficLightStates += simMgr.junction[i].signalMask.Length;

		//this is the search parameter
		bestTrafficTiming = new float[numTotalTrafficLightStates];

		//Start the search loop
		StartCoroutine (doSearch ());
	}

	IEnumerator doSearch()
	{
		float avgFitnessValue = 0f;
		int numJunctions = simMgr.junction.Length;

		//try with while(avgFitnessValue > 0.6)
		for (int g=0; g<numGenerations; g++) {

		for (int i = 0; i < maxRuns; i++) {
			for (int j = 0; j < numJunctions; j++) {
					int numTrafficLightStates = simMgr.junction[j].signalMask.Length;
					for (int k = 0; k < numTrafficLightStates; k++)
						simMgr.junction [j].signalMask [k].duration = chromosomes [i].getGene (k);//bestSimTime [k];//10f;//Random.Range (5f, 15f);
			}

			simMgr.SimSpeed = 40f;
			simMgr.StartSim(0f,1f);//this runs it for the full dataset
			//if you want to splice a smaller subsection you can provide
			//a normalized start-end range e.g. simMgr.startSim(0.4f,0.5f)
			//This will run the sim from the 40% to the 50% of the dataset


			//Block till the sim is not finished
			while (simMgr.isSimRunning ())
				yield return null;

			//Alternatively you can also try time thresholding like
			//while (simMgr.SimTime() < threshold). Make sure your sim time
			//normally would overrun this threshold otherwise you will be stuck 
			//here forever. It is better you use the above data splicing method
			//for shorter training runs

			//we get this after one run of the simulation finishes
			Debug.Log ("avg wait time : " + simMgr.AvgWaitTime.ToString ());
			Debug.Log ("avg time penalty : " + simMgr.AvgTimePenalty.ToString ());
			Debug.Log ("sim time" + simMgr.SimTime().ToString());
			minScheduleTime = simMgr.SimTime ();
			fitnessValues.Add (chromosomes [i].calculateFitness (simMgr.AvgWaitTime, simMgr.AvgTimePenalty, minScheduleTime));
/*
			//An example cost function
			float cost = simMgr.AvgWaitTime + simMgr.AvgTimePenalty;
			if (cost < minCost) {
				minCost = cost;
				minScheduleTime = simMgr.SimTime(); 
				minPenalty = simMgr.AvgTimePenalty;
				minWait = simMgr.AvgWaitTime;
				for (int j = 0, l = 0; j < numJunctions; j++) {
					int numTrafficLightStates = simMgr.junction [j].signalMask.Length;
					for (int k = 0; k < numTrafficLightStates; k++,l++)
						bestTrafficTiming[l] = simMgr.junction[j].signalMask[k].duration;
					}
				}
			}


		Debug.Log ("Best Timing found... Setting all the params to best set");
		Debug.Log ("Least cost: " + minCost.ToString()+
			" Best schedule: "+minScheduleTime+"s"+
			" Least Penalty: "+minPenalty+"s "+
		    " Least Wait: "+minWait+"s");
		for (int j = 0, l = 0; j < numJunctions; j++) {
			int numTrafficLightStates = simMgr.junction [j].signalMask.Length;
			for (int k = 0; k < numTrafficLightStates; k++,l++)
				simMgr.junction[j].signalMask[k].duration = bestTrafficTiming[l];
		}
		*/
				//at this point all the configurtions have been run and fitness has been calculated.
			}
				//pick the fittest
				float first = 1000000f; //same as min
				float second = 1000000f;
				float max = 0f;
				int firstIndex = 0;
				int secondIndex = 0;
				for (int m=0; m<maxRuns; m++) {
					//Debug.Log ("fitness value of " + m.ToString () + " : " + fitnessValues [m]);
					if (fitnessValues [m] < first) {
						second = first;
						secondIndex = firstIndex;
						first = fitnessValues [m];
						firstIndex = m; 

					} else if (fitnessValues [m] < second && fitnessValues [m] != first) {
						second = fitnessValues [m];
						secondIndex = m;
					}

					if (fitnessValues [m] > max) {
						max = fitnessValues [m];
					}
				}

				//normalise fitness values
				for (int i=0; i<fitnessValues.Count; i++) {
					fitnessValues [i] = (fitnessValues [i] - first) / (max - first);
				}

				//to pick the ones with the largest wait times
				float badFirst = 0f;
				float badSecond = 0f;
				int badFirstIndex = 0;
				int badSecondIndex = 0;

				for (int i=0; i<maxRuns; i++) {
					//Debug.Log ("fitness value of " + i.ToString () + " : " + fitnessValues [i]);
					if (fitnessValues [i] > badFirst) {
						badSecond = badFirst;
						badSecondIndex = badFirstIndex;
						badFirst = fitnessValues [i];
						badFirstIndex = i; 

					} else if (fitnessValues [i] > badSecond && fitnessValues [i] != badFirst) {
						badSecond = fitnessValues [i];
						badSecondIndex = i;
					}
				}

				//Debug.Log ("the fittest candidates are at indices : " + first + " : " + firstIndex.ToString () + " , " + second + " : " + secondIndex.ToString () + 
				  //         " max num :" + max.ToString ());
				//Debug.Log ("indices of two bad chromosomes : " + badFirstIndex + " : " + badFirst.ToString () + " , " + badSecondIndex + " : " + badSecond.ToString ());
				//give the indices of the two fittest chromosomes

				//find parents and crossover
				//this modifies the chromosome list
				myGeneticAlgorithm.p.getParentsFromElitism (fitnessValues, firstIndex, secondIndex, badFirstIndex, badSecondIndex, maxRuns);
				//mutate
				p.applyMutation ();
				//tell my population to update the list
				p.createNextGeneration (chromosomes);

				//clear the fitness values list!
				fitnessValues.Clear ();
				//progress to next generation
				//for (int i=0; i<chromosomes.Count; i++) {
				//	Debug.Log ("chromosome next gen " + i + " : " + chromosomes [i].getGene (i));
				//}
			}


			Debug.Log ("Best Timing found... Setting all the params to best set");
			Debug.Log ("Least cost: " + minCost.ToString()+
			           " Best schedule: "+minScheduleTime+"s"+
			           " Least Penalty: "+minPenalty+"s "+
			           " Least Wait: "+minWait+"s");
		//}
	}

	void SetSignalMask(Junction jn)
	{
		int numTrafficLights = jn.incoming.Length;
		Junction.SignalMask[] signalMask = new Junction.SignalMask[numTrafficLights*2];
		for (int i = 0; i < 8; i++) {
			signalMask[i].mask = SigMasks[i];
		}
		jn.signalMask = signalMask;
	}
}
