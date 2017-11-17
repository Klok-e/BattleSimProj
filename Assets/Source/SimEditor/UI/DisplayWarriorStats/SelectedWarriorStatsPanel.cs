using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;

namespace SimEditor
{
    public class SelectedWarriorStatsPanel : MonoBehaviour
    {
        [SerializeField] private GameObject cellPref;

        [SerializeField] private GameObject inputsPanel;
        [SerializeField] private GameObject outputsPanel;
        [SerializeField] private Text fitnessText;
        [SerializeField] private Image genomeGraphImg;

        private WarrViewCellScript[] inputCells;
        private WarrViewCellScript[] outputCells;

        public void Initialize()
        {
            var cells = new List<WarrViewCellScript>();
            //setting up inputs
            for (int i = 0; i < HelperConstants.totalAmountOfSensors; i++)
            {
                var obj = Instantiate(cellPref);
                obj.transform.SetParent(inputsPanel.transform, false);
                obj.transform.localScale = Vector3.one;

                var scrpt = obj.GetComponent<WarrViewCellScript>();
                scrpt.Initialize(-1, 1);
                cells.Add(scrpt);
            }
            inputCells = cells.ToArray();
            cells.Clear();

            //setting up outputs
            for (int i = 0; i < HelperConstants.totalAmountOfOutputsOfNet; i++)
            {
                var obj = Instantiate(cellPref, outputsPanel.transform);
                obj.transform.SetParent(outputsPanel.transform, false);
                obj.transform.localScale = Vector3.one;

                var scrpt = obj.GetComponent<WarrViewCellScript>();
                scrpt.Initialize(0, 1);
                cells.Add(scrpt);
            }
            outputCells = cells.ToArray();
            cells.Clear();
        }

        public void DrawGenome(SharpNeat.Genomes.Neat.NeatGenome genome)
        {
            var painter = new GenomeDisplayer();

            genomeGraphImg.sprite = painter.GetImage(genome);
        }

        public void RefreshCells(double[] inps, double[] outps, float fitness)
        {
            Debug.Assert(inps.Length == inputCells.Length);
            for (int i = 0; i < inputCells.Length; i++)
            {
                inputCells[i].SetColour((float)inps[i]);
            }

            Debug.Assert(outps.Length == outputCells.Length);
            for (int i = 0; i < outputCells.Length; i++)
            {
                outputCells[i].SetColour((float)outps[i]);
            }
            fitnessText.text = fitness.ToString();
        }
    }
}
