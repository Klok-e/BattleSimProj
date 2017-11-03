using SharpNeat.Genomes.Neat;
using SharpNeat.Phenomes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimEditor
{
    public class CoEvaluator
    {
        int players;
        public Dictionary<PlayerController, Dictionary<IBlackBox, NeatGenome>> playerNetsDict { get; } = new Dictionary<PlayerController, Dictionary<IBlackBox, NeatGenome>>();

        public bool evalFinished;
        public bool EvalRequested { get { return evalRequested; } }
        private bool evalRequested;

        public CoEvaluator(int players)
        {
            this.players = players;
        }

        /// <summary>
        /// Meant to be started by several players.
        /// Evaluation starts only if all of them have submitted genomes.
        /// </summary>
        public IEnumerator SubmitGenomesAndWaitUntilTheyAreEvaluated(List<NeatGenome> list, PlayerController player)
        {
            playerNetsDict.Add(player, new Dictionary<IBlackBox, NeatGenome>());
            for (int i = 0; i < player.AmoutOfWarriorsOwns; i++)
            {
                playerNetsDict[player].Add(player.Decode(list[i]), list[i]);
            }
            evalFinished = false;
            evalRequested = false;
            while (!evalFinished)
            {
                if (playerNetsDict.Count == players)
                {
                    evalRequested = true;
                }
                yield return null;
            }
            playerNetsDict.Clear();
        }
    }
}
