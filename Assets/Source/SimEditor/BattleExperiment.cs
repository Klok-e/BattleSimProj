using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using SharpNeat.Core;
using SharpNeat.EvolutionAlgorithms;
using SharpNeat.Genomes.Neat;
using SharpNeat.Phenomes;
using SharpNeat.DistanceMetrics;
using SharpNeat.SpeciationStrategies;
using SharpNeat.EvolutionAlgorithms.ComplexityRegulation;
using SharpNeat.Decoders.Neat;
using SharpNeat.Decoders;
using UnityEngine;
using System.IO;

namespace SimEditor
{
    internal class BattleExperiment
    {
        public int InputCount = HelperConstants.totalAmountOfSensors;

        public int OutputCount = HelperConstants.totalAmountOfOutputsOfNet;

        public int _complexityThreshold = HelperConstants.complexityThreshold;

        public NeatEvolutionAlgorithmParameters _eaParams;
        public NeatGenomeParameters _neatGenomeParams;
        private NetworkActivationScheme _activationScheme;

        public NeatEvolutionAlgorithm<NeatGenome> _ea;

        public BattleExperiment()
        {
            _activationScheme = NetworkActivationScheme.CreateCyclicFixedTimestepsScheme(2, true);

            _eaParams = new NeatEvolutionAlgorithmParameters();

            _neatGenomeParams = new NeatGenomeParameters();

            SharpNeat.Network.SReLU func = (SharpNeat.Network.SReLU)new SharpNeat.Network.SReLU();
            func.SetParameters(-0.8, 0.8, 0.05);

            _neatGenomeParams.ActivationFn = func;
            _neatGenomeParams.AddConnectionMutationProbability = 0.7;
            _neatGenomeParams.AddNodeMutationProbability = 0.2;
            _neatGenomeParams.DeleteConnectionMutationProbability = 0.4;
            _neatGenomeParams.ConnectionWeightMutationProbability = 0.94;
            _neatGenomeParams.InitialInterconnectionsProportion = 0;

            _neatGenomeParams.FeedforwardOnly = _activationScheme.AcyclicNetwork;
        }

        public NeatGenomeDecoder CreateDecoder()
        {
            return new NeatGenomeDecoder(_activationScheme);
        }

        public NeatEvolutionAlgorithm<NeatGenome> CreateEvolutionAlgorithm(BattleEvaluator<NeatGenome> evaluator, int populationSize)
        {
            Debug.Assert(populationSize > 5);
            _eaParams.SpecieCount = populationSize / 5;

            var genomeFactory = new NeatGenomeFactory(InputCount, OutputCount, _neatGenomeParams);
            var genomeList = genomeFactory.CreateGenomeList(populationSize, 0);
            return CreateEvolutionAlgorithm(evaluator, genomeList);
        }

        public NeatEvolutionAlgorithm<NeatGenome> CreateEvolutionAlgorithm(BattleEvaluator<NeatGenome> evaluator, List<NeatGenome> list)
        {
            IDistanceMetric distanceMetric = new ManhattanDistanceMetric(1.0, 0.0, 10.0);
            ISpeciationStrategy<NeatGenome> speciationStrategy = new KMeansClusteringStrategy<NeatGenome>(distanceMetric);
            IComplexityRegulationStrategy complexityRegulationStrategy = new DefaultComplexityRegulationStrategy(ComplexityCeilingType.Absolute, _complexityThreshold);

            NeatEvolutionAlgorithm<NeatGenome> neatEvolutionAlgorithm = new NeatEvolutionAlgorithm<NeatGenome>(_eaParams, speciationStrategy, complexityRegulationStrategy);

            var genomeFactory = new NeatGenomeFactory(InputCount, OutputCount, _neatGenomeParams);

            neatEvolutionAlgorithm.Initialize(evaluator, genomeFactory, list);

            _ea = neatEvolutionAlgorithm;
            return neatEvolutionAlgorithm;
        }

        public IGenomeFactory<NeatGenome> CreateGenomeFactory()
        {
            return new NeatGenomeFactory(InputCount, OutputCount, _neatGenomeParams);
        }

        public void SavePopulation(string filename)
        {
            XmlWriterSettings _xwSettings = new XmlWriterSettings();
            _xwSettings.Indent = true;

            Directory.CreateDirectory(Application.dataPath + HelperConstants.saveDirectory);

            using (XmlWriter xw = XmlWriter.Create(Application.dataPath + HelperConstants.saveDirectory + filename, _xwSettings))
            {
                NeatGenomeXmlIO.WriteComplete(xw, _ea.GenomeList, false);
            }
            SaveLoad.Load();
        }

        public List<NeatGenome> LoadPopulation(string filename, int amountToLoad)
        {
            if (File.Exists(Application.dataPath + HelperConstants.saveDirectory + filename))
            {
                List<NeatGenome> genomes;
                using (XmlReader xr = XmlReader.Create(Application.dataPath + HelperConstants.saveDirectory + filename))
                {
                    NeatGenomeFactory genomeFactory = (NeatGenomeFactory)CreateGenomeFactory();
                    genomes = NeatGenomeXmlIO.ReadCompleteGenomeList(xr, false, genomeFactory);
                }

                var newGenomes = new List<NeatGenome>(amountToLoad);
                while (newGenomes.Count < amountToLoad)
                {
                    for (int i = 0; i < genomes.Count; i++)
                    {
                        if (newGenomes.Count >= amountToLoad)
                        {
                            break;
                        }
                        newGenomes.Add(genomes[i]);
                    }
                }
                Debug.Assert(newGenomes.Count == amountToLoad);
                return newGenomes;
            }
            else
            {
                Debug.Log("Could not load pop from " + Application.dataPath + HelperConstants.saveDirectory + filename);
                return null;
            }
        }
    }
}
