using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimulationX;
using System;


public class Chromosome {
	//a list of the traffic signal mask and the duration
	//8 signal masks and their durations
	//public List<float> cr = new List<float> {0f,1f,2f,3f,4f,5f,6f,7f};
	public List<float> cr = new List<float> ();
	//string c;

	public List<float> getGeneList(){
		return cr;
	}

	public void addAllele(float t){
		cr.Add (t);
	}

	public void shuffle (List<float> array)
	{
		System.Random rng = new System.Random();   // i.e., java.util.Random.
		int n = array.Count;        // The number of items left to shuffle (loop invariant).
		while (n > 1)
		{
			int k = rng.Next(n);  // 0 <= k < n.
			n--;                     // n is now the last pertinent index;
			float temp = array[n];     // swap array[n] with array[k] (does nothing if k == n).
			array[n] = array[k];
			array[k] = temp;
		}
	}
	
	//figure out if this can be replaced by a constructor!
	public void makeChromosome(){
		//shuffle the list and append a random time duration for the list
		//for (int i=0; i<8; i++) {
		//	Debug.Log ("array before shuffling : " + cr[i].ToString());
		//}
		//shuffle (cr);
		//use this for the one junction scene
		//for (int i=0; i<8; i++) {
		//	Debug.Log ("array after shuffling : " + cr[i].ToString());
		//}
		//change for 4 junctions! to 8*4
		for (int i=0; i<8*4; i++) {
			//for every signalMask
			cr.Add (UnityEngine.Random.Range (5f, 12f));
			//cr.Add (10f);
		}
		/*
		for (int i=0; i<8; i++) {
			//for every signalMask
			cr.Add (UnityEngine.Random.Range (5f, 15f));
		}
		*/

}

public void setGene(float newVal, int index){
	cr [index] = newVal;
}

public float getGene(int i){
	return cr[i];
}
//mutate the chromosome
public void mutate(){
	System.Random rnd = new System.Random ();
	int index1 = rnd.Next (0,cr.Count );
	int index2 = rnd.Next (0,cr.Count );
	int index3 = rnd.Next (0,cr.Count );

	float minVal = 0f;
	float maxVal = 5f;

	double val1 = rnd.NextDouble () * (maxVal - minVal) + minVal;
	double val2 = rnd.NextDouble () * (maxVal - minVal) + minVal;
	double val3 = rnd.NextDouble () * (maxVal - minVal) + minVal;

	cr [index1] += (float)val1;
	cr [index2] += (float)val2;
	cr [index3] += (float)val3;

}

public float calculateFitness(float avgWaitTime, float avgTimePenalty , float simTime){
		//0.3 * avgWaitTime + 0.7* avgTimePenalty after one chromosome runs
		//Debug.Log ("avgWaitTime : " + avgWaitTime.ToString() + " sim time : " + simTime.ToString());
		//I have changed the 
		//float fitness = 0.3f * (avgWaitTime) + (0.5f) * (avgTimePenalty) + 0.2f * (simTime);
		float fitness = (avgWaitTime) + (avgTimePenalty) + (simTime);
		//float fitness = (simTime);
		//float fitness = 1/ (numMovingars);	
		Debug.Log ("fitness value" + fitness.ToString() );
		return fitness;

}
}

public class Population {
	public List<Chromosome> chromosomeList = new List<Chromosome> ();
	public int populationSize;

	//give birth to  individuals
	public void populate(){
		for (int i=0; i<populationSize; i++) {
			Chromosome c = new Chromosome ();
			c.makeChromosome ();
			chromosomeList.Add (c);
			Console.WriteLine ("x:{0}", c.cr.Count);
		}

		Debug.Log ("first Generation wait times");
		for (int i=0; i<chromosomeList.Count; i++) {
			Debug.Log ("generation number : " + i.ToString());
			for (int j=0; j<8; j++) {
				Debug.Log ("value at " + i + " : " + chromosomeList [i].getGene(j).ToString ());
			}
		}

	}

	public void createNextGeneration(List<Chromosome> newGen){
		Debug.Log ("Next Generation wait times");
		for (int i=0; i<newGen.Count; i++) {
			Debug.Log ("generation number : " + i.ToString());
			for (int j=0; j<8; j++) {
				Debug.Log ("value at " + i + " : " + newGen [i].getGene(j).ToString ());
			}
		}
		chromosomeList = newGen;
	}

	public List<Chromosome> getChromosomes(){	
		return chromosomeList;
	}
	//get the 2 fittest chromosomes
	//Elitism or steady state selection
	//randomly select two chromosomes which are not firstFittest or seconFittest, they are parents for gen next
	public void getParentsFromElitism(List<float> fitnessVals, int fittestIndex, int secondFittestIndex, int badIndex1 , int badIndex2, int numRuns){
		System.Random rnd = new System.Random ();
		int p1 = rnd.Next (0,numRuns);
		int p2 = rnd.Next (0,numRuns);
		while(p1 == fittestIndex || p1 == secondFittestIndex){
			p1 = rnd.Next (0, numRuns);
		}
		while(p2 == fittestIndex || p2 == secondFittestIndex){
			p2 = rnd.Next (0, numRuns);
		}
		//Debug.Log ("parent indicies : " + fittestIndex  + " : "+ secondFittestIndex + " : " + p1 + " : " + p2);

		//List<Chromosome> children = crossover (chromosomeList[p1], chromosomeList[p2], chromosomeList[fittestIndex], chromosomeList[secondFittestIndex]);
		List<Chromosome> children = new List<Chromosome> ();
		Chromosome child1 = crossover1 (chromosomeList[p1], chromosomeList[fittestIndex]);
		Chromosome child2 = crossover1 (chromosomeList[p2], chromosomeList[secondFittestIndex]);

		//Chromosome child1 = crossover2 (chromosomeList[secondFittestIndex], chromosomeList[fittestIndex]);
		//Chromosome child2 = crossover2 (chromosomeList[fittestIndex], chromosomeList[secondFittestIndex]);

		children.Add (child1); children.Add (child2);
		//replace exising bad solutions with
		chromosomeList [badIndex1] = children [0];
		chromosomeList [badIndex2] = children [1];
	}

	//remove the bad chromosomes
	void removeUnfit(){

	}

	//Add new fitter chromosomes
	void addChildren(){

	}

	//return a child, after crossover of two chromosomes
	//single point crossover
	//might need to test this!
	List<Chromosome> crossover(Chromosome c1, Chromosome c2 , Chromosome fit1, Chromosome fit2){
		List<Chromosome> test = new List<Chromosome> ();
		System.Random rnd = new System.Random ();
		int p = rnd.Next (0,7);
		int p1 = rnd.Next (0,7);
		//c1[0:p] + c2[p:7]
		//c2[0:p1] + c1[p1:7]
		Chromosome temp1 = new Chromosome();
		Chromosome temp2 = new Chromosome();
		for (int i=0; i<8; i++) {
			temp1.addAllele (c1.getGene (i));
			temp2.addAllele (c2.getGene (i));
		}
		for (int i=0; i<8; i++) {
			if (i > p) {
				temp1.setGene (c2.getGene(i), i);
			}
			if (i > p1) {
				temp2.setGene (c1.getGene(i), i);
			}

		}
		test.Add (temp1); test.Add (temp2);
		return test;
	}

	Chromosome crossover1(Chromosome c1, Chromosome fit1){
		System.Random rnd = new System.Random ();
		int p = rnd.Next (0,4);
		//int p1 = rnd.Next (0,7);
		//c1[0:p] + c2[p:7]
		//c2[0:p1] + c1[p1:7]
		Chromosome temp1 = new Chromosome ();

		for (int i=0; i<8; i++) {
			temp1.addAllele (c1.getGene (i));
		}
		for (int i=0; i<8; i++) {
			if (i >= p) {
				temp1.setGene (fit1.getGene(i), i);
			}

		}
		return temp1;
	}

	Chromosome crossover2(Chromosome fit1, Chromosome fit2){
		System.Random rnd = new System.Random ();
		int p = rnd.Next (0,7);
		//int p1 = rnd.Next (0,7);
		//c1[0:p] + c2[p:7]
		//c2[0:p1] + c1[p1:7]
		Chromosome temp1 = new Chromosome ();

		for (int i=0; i<8; i++) {
			temp1.addAllele (fit1.getGene (i));
		}
		for (int i=0; i<8; i++) {
			if (i >= p) {
				temp1.setGene (fit2.getGene(i), i);
			}

		}
		return temp1;
	}

	//randomly select a chromosome and mutate it
	public void applyMutation(){
		System.Random rnd = new System.Random ();
		//randomly select 3 chromosomes to mutate
		int p1 = rnd.Next (0, chromosomeList.Count);
		int p2 = rnd.Next (0, chromosomeList.Count);
		int p3 = rnd.Next (0, chromosomeList.Count);
		chromosomeList [p1].mutate ();
		chromosomeList [p2].mutate ();
		chromosomeList [p3].mutate ();

	}
}

public class GeneticAlgorithm{

	List<string> SigMasks = new List<string>{
		"1101110001100111",
		"1011111001100011",
		"1001110101110011",
		"1001110011101011",
		"1011110011100011",
		"1001110101100111",
		"1101110001110011",
		"1001111001101011"};

	//I am making everything public!
	public Population p;

	public void initialize(int numRuns){
		p = new Population ();
		p.populationSize = numRuns;
		p.populate ();

	}

	public Population getPopulation(){
			return p;
		}
	}
