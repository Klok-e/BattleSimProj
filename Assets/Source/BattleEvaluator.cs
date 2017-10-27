using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpNeat;
using SharpNeat.Core;
using SharpNeat.Genomes.Neat;
using UnityEngine;
using System.Threading;
using System.Collections;

namespace Assets.Source
{
    class BattleEvaluator<TGenome> : IGenomeListEvaluator<TGenome>
            where TGenome : class, IGenome<TGenome>
    {
        PlayerController player;
        public BattleEvaluator(PlayerController player)
        {
            this.player = player;
        }

        public ulong EvaluationCount { get; set; }

        public bool StopConditionSatisfied { get; set; }

        public IEnumerator Evaluate(IList<TGenome> genomeList)
        {
            yield return Coroutiner.StartCoroutine(GameManagerController.inputManagerInstance.simInst.coEvaluator.SubmitGenomesAndWaitUntilTheyAreEvaluated((List<NeatGenome>)genomeList, player));

            #region Log statistics and delete fitness
            float totalFitness = 0;
            float mxFitness = (float)genomeList[0].EvaluationInfo.Fitness;
            float totalComplexity = 0;
            float mxComplexity = (float)genomeList[0].Complexity;
            foreach (var genome in genomeList)
            {
                totalFitness += (float)genome.EvaluationInfo.Fitness;
                mxFitness = Math.Max(mxFitness, (float)genome.EvaluationInfo.Fitness);
                totalComplexity += (float)genome.Complexity;
                mxComplexity = Math.Max(mxComplexity, (float)genome.Complexity);
            }
            Debug.Log(String.Format("Max fitness is {0}; Mean fitnes is {1}; Max complexity is {2}; Mean complexity is {3}",
                mxFitness,
                totalFitness / genomeList.Count,
                mxComplexity,
                totalComplexity / genomeList.Count));
            #endregion
            Debug.Log($"Evaluation {++EvaluationCount} of team {player.Team} has finished");
        }

        public void Reset()
        {

        }
    }
}
