using SharpNeat.Decoders.Neat;
using SharpNeat.Genomes.Neat;
using SharpNeat.Phenomes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    public int AmoutOfWarriorsOwns { get { return amoutOfWarriorsOwns; } }
    private int amoutOfWarriorsOwns;

    BattleExperiment experiment;

    NeatGenomeDecoder decoder;

    public IList<NeatGenome> GenomeList { get { return experiment._ea.GenomeList; } }

    public int Team { get { return team; } }
    private int team;

    public List<Vector3> pointsToVisitDuringTraining;
    public bool IsEvolutionFreezed { get { return isEvolutionFreezed; } }
    private bool isEvolutionFreezed;

    LineRenderer lineRenderer;

    private void Update()
    {
        if (initialized)
        {
            lineRenderer.positionCount = pointsToVisitDuringTraining.Count;
            lineRenderer.SetPositions(pointsToVisitDuringTraining.ToArray());
        }
    }

    bool initialized;
    public void Initialize(Vector2 pos, int amoutOfWarriorsOwns, int team,bool freeze)
    {
        Debug.Assert(amoutOfWarriorsOwns > 5);
        transform.position = pos;
        experiment = CreateExperiment(amoutOfWarriorsOwns);
        decoder = experiment.CreateDecoder();
        this.team = team;

        this.amoutOfWarriorsOwns = amoutOfWarriorsOwns;

        pointsToVisitDuringTraining = new List<Vector3>();
        pointsToVisitDuringTraining.Add(pos);

        initialized = true;

        lineRenderer = GetComponent<LineRenderer>();
        isEvolutionFreezed = freeze;
    }

    private float CalculateSpeed(List<Vector3> list)
    {
        float totalDistance = 0;
        for (int i = 0; i < pointsToVisitDuringTraining.Count - 1; i++)
        {
            totalDistance += Vector3.Distance(pointsToVisitDuringTraining[i], pointsToVisitDuringTraining[i + 1]);
        }
        float moveSpeed = totalDistance / HelperConstants.ticksPerEvaluation;

        return moveSpeed;
    }

    public IEnumerator TickTrain()
    {
        transform.position = pointsToVisitDuringTraining[0];
        float moveSpeed = CalculateSpeed(pointsToVisitDuringTraining);

        for (int goingToPointWithIndex = 0; goingToPointWithIndex < pointsToVisitDuringTraining.Count - 1; goingToPointWithIndex++)
        {
            Vector2 directionCurrentlyGoing = (pointsToVisitDuringTraining[goingToPointWithIndex + 1] - transform.position);
            int timeLeft = (int)Mathf.Round(directionCurrentlyGoing.magnitude / moveSpeed);
            for (; timeLeft >= 0; timeLeft--)
            {
                transform.Translate(directionCurrentlyGoing.normalized * moveSpeed);
                yield return new WaitForFixedUpdate();
            }
        }
        transform.position = pointsToVisitDuringTraining[0];
    }

    public void UserInputTick()
    {

    }

    public IEnumerator BeginDoingOneGeneration()
    {
        if (IsEvolutionFreezed)
        {
            var eval = new BattleEvaluator<NeatGenome>(this);
            yield return eval.Evaluate(GenomeList);
        }
        else
        {
            yield return StartCoroutine(experiment._ea.DoOneGeneration());
        }
    }

    private BattleExperiment CreateExperiment(int warriors)
    {
        var experiment = new BattleExperiment();

        var battleEvaluator = new BattleEvaluator<NeatGenome>(this);

        experiment._ea = experiment.CreateEvolutionAlgorithm(battleEvaluator, warriors);
        Debug.Log("Experiment created");
        return experiment;
    }

    public IBlackBox Decode(NeatGenome genome)
    {
        return decoder.Decode(genome);
    }

    public void LoadPopulation(string filename)
    {
        var battleEvaluator = new BattleEvaluator<NeatGenome>(this);

        experiment.CreateEvolutionAlgorithm(battleEvaluator, experiment.LoadPopulation(filename));
    }

    public void SavePopulation(string filename)
    {
        experiment.SavePopulation(filename);
    }
}
