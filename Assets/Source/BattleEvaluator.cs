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
        //public static object locker = new object();

        public BattleEvaluator()
        {

        }

        public ulong EvaluationCount { get; set; }

        public bool StopConditionSatisfied { get; set; }

        public IEnumerator Evaluate(IList<TGenome> genomeList)
        {
            yield return Coroutiner.StartCoroutine(SimController.simInstance.StartEvaluatingGenomes((List<NeatGenome>)genomeList));
            Debug.Log(String.Format("Evaluation {0} finished", EvaluationCount));

            EvaluationCount++;
        }

        public void Reset()
        {

        }
    }
}
