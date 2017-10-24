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

namespace Assets.Source
{
    class BattleExperiment
    {
        public int InputCount = HelperConstants.totalAmountOfSensors;

        public int OutputCount = HelperConstants.totalAmountOfOutputsOfNet;

        public int _complexityThreshold = HelperConstants.complexityThreshold;

        public NeatEvolutionAlgorithmParameters _eaParams;
        public NeatGenomeParameters _neatGenomeParams;
        NetworkActivationScheme _activationScheme;

        public NeatEvolutionAlgorithm<NeatGenome> _ea;

        public BattleExperiment()
        {
            _activationScheme = NetworkActivationScheme.CreateCyclicFixedTimestepsScheme(2);

            _eaParams = new NeatEvolutionAlgorithmParameters();
            _eaParams.SpecieCount = 5;

            _neatGenomeParams = new NeatGenomeParameters();
            //_neatGenomeParams.ActivationFn = new SharpNeat.Network.Linear();
            _neatGenomeParams.AddConnectionMutationProbability = 0.5;
            _neatGenomeParams.AddNodeMutationProbability = 0.3;
            _neatGenomeParams.DeleteConnectionMutationProbability = 0.4;
            _neatGenomeParams.ConnectionWeightMutationProbability = 0.94;
            _neatGenomeParams.InitialInterconnectionsProportion = 0.2;

            _neatGenomeParams.FeedforwardOnly = _activationScheme.AcyclicNetwork;
        }

        public NeatGenomeDecoder CreateDecoder()
        {
            return new NeatGenomeDecoder(_activationScheme);
        }

        public NeatEvolutionAlgorithm<NeatGenome> CreateEvolutionAlgorithm(BattleEvaluator<NeatGenome> evaluator, IGenomeDecoder<NeatGenome, IBlackBox> genomeDecoder, int populationSize)
        {
            var genomeFactory = new NeatGenomeFactory(InputCount, OutputCount, _neatGenomeParams);
            var genomeList = genomeFactory.CreateGenomeList(populationSize, 0);
            return CreateEvolutionAlgorithm(evaluator, genomeList);
        }

        public NeatEvolutionAlgorithm<NeatGenome> CreateEvolutionAlgorithm(BattleEvaluator<NeatGenome> evaluator,List<NeatGenome> list)
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

            
            using (XmlWriter xw = XmlWriter.Create(Application.dataPath + "/" + filename, _xwSettings))
            {
                NeatGenomeXmlIO.WriteComplete(xw, _ea.GenomeList, false);
            }
        }

        public List<NeatGenome> LoadPopulation(string filename)
        {
            using (XmlReader xr = XmlReader.Create(Application.dataPath + "/" + filename))
            {
                NeatGenomeFactory genomeFactory = (NeatGenomeFactory)CreateGenomeFactory();
                return NeatGenomeXmlIO.ReadCompleteGenomeList(xr, false, genomeFactory);
            }
        }
    }
}