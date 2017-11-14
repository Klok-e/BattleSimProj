using SharpNeat.Decoders.Neat;
using SharpNeat.Genomes.Neat;
using SharpNeat.Phenomes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SimEditor
{
    public class PlayerController : APlayerController
    {
        public int AmoutOfWarriorsOwns { get { return amoutOfWarriorsOwns; } }
        private int amoutOfWarriorsOwns;

        private BattleExperiment experiment;

        private NeatGenomeDecoder decoder;

        public IList<NeatGenome> GenomeList { get { return experiment._ea.GenomeList; } }

        public int Team { get { return team; } }
        private int team;

        //public List<Vector3> pointsToVisitDuringTraining;
        public bool IsEvolutionFreezed { get { return isEvolutionFreezed; } }

        private bool isEvolutionFreezed;

        private LineRenderer lineRenderer;

        private void Update()
        {
            if (initialized)
            {
                lineRenderer.positionCount = pointsToVisitDuringTraining.Count + 1;
                var temp = new List<Vector3>(pointsToVisitDuringTraining);
                temp.Insert(0, new Vector3(defaultPos.x, defaultPos.y, -1));
                lineRenderer.SetPositions(temp.ToArray());
            }
        }

        private bool initialized;

        public void Initialize(Vector2 pos, int amoutOfWarriorsOwns, int team, bool freeze)
        {
            Debug.Assert(amoutOfWarriorsOwns > 5);
            transform.position = pos;
            defaultPos = pos;
            experiment = CreateExperiment(amoutOfWarriorsOwns);
            decoder = experiment.CreateDecoder();
            this.team = team;

            this.amoutOfWarriorsOwns = amoutOfWarriorsOwns;

            pointsToVisitDuringTraining = new List<Vector3>();

            initialized = true;

            lineRenderer = GetComponent<LineRenderer>();
            isEvolutionFreezed = freeze;

            nextPosChange = new Vector2();
        }

        private float CalculateSpeed(List<Vector3> list)
        {
            float totalDistance = 0;
            if (pointsToVisitDuringTraining.Count != 0) totalDistance = Vector3.Distance(defaultPos, pointsToVisitDuringTraining[0]);
            for (int i = 0; i < pointsToVisitDuringTraining.Count - 1; i++)
            {
                totalDistance += Vector3.Distance(pointsToVisitDuringTraining[i], pointsToVisitDuringTraining[i + 1]);
            }
            float moveSpeed = totalDistance / HelperConstants.ticksPerEvaluation;

            return moveSpeed;
        }

        private IEnumerator tickTrain;

        public void ResetToDefault()
        {
            tickTrain = TickTrain();
            transform.position = defaultPos;
        }

        public void Tick()
        {
            tickTrain.MoveNext();
        }

        public Vector2 defaultPos;

        private IEnumerator TickTrain()
        {
            float moveSpeed = CalculateSpeed(pointsToVisitDuringTraining);

            int ticksPassed = 0;
            for (int goingToPointWithIndex = 0; goingToPointWithIndex < pointsToVisitDuringTraining.Count; goingToPointWithIndex++)
            {
                Vector2 directionCurrentlyGoing = (pointsToVisitDuringTraining[goingToPointWithIndex] - transform.position);
                int timeLeft = moveSpeed != 0 ? (int)Mathf.Round(directionCurrentlyGoing.magnitude / moveSpeed) : HelperConstants.ticksPerEvaluation;
                for (; timeLeft >= 0; timeLeft--)
                {
                    transform.Translate(nextPosChange);
                    nextPosChange = (Vector3)(directionCurrentlyGoing.normalized * moveSpeed);

                    ticksPassed++;
                    yield return false;
                }
            }
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

            experiment.CreateEvolutionAlgorithm(battleEvaluator, experiment.LoadPopulation(filename, AmoutOfWarriorsOwns));
        }

        public void SavePopulation(string filename)
        {
            experiment.SavePopulation(filename);
        }
    }
}
