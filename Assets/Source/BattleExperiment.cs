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

namespace Assets.Source
{
    class BattleExperiment
    {
        public int InputCount = HelperConstants.totalAmountOfSensors;

        public int OutputCount = HelperConstants.totalAmountOfOutputsOfNet;

        public int _complexityThreshold = HelperConstants.complexityThreshold;

        public NeatEvolutionAlgorithmParameters _eaParams;
        public NeatGenomeParameters _neatGenomeParams;

        public BattleExperiment()
        {
            _eaParams = new NeatEvolutionAlgorithmParameters();
            _eaParams.SpecieCount = 10;

            _neatGenomeParams = new NeatGenomeParameters();
            _neatGenomeParams.ActivationFn = new SharpNeat.Network.Linear();
            _neatGenomeParams.AddConnectionMutationProbability = 0.5;
            _neatGenomeParams.AddNodeMutationProbability = 0.3;
            _neatGenomeParams.DeleteConnectionMutationProbability = 0.4;
            _neatGenomeParams.ConnectionWeightMutationProbability = 0.94;
            _neatGenomeParams.InitialInterconnectionsProportion = 1;
        }

        public IGenomeDecoder<NeatGenome, IBlackBox> genomeDecoder;
        public NeatEvolutionAlgorithm<NeatGenome> CreateEvolutionAlgorithm(BattleEvaluator<NeatGenome> evaluator, int populationSize)
        {
            IDistanceMetric distanceMetric = new ManhattanDistanceMetric(1.0, 0.0, 10.0);
            ISpeciationStrategy<NeatGenome> speciationStrategy = new KMeansClusteringStrategy<NeatGenome>(distanceMetric);
            IComplexityRegulationStrategy complexityRegulationStrategy = new DefaultComplexityRegulationStrategy(ComplexityCeilingType.Absolute, _complexityThreshold);

            NeatEvolutionAlgorithm<NeatGenome> neatEvolutionAlgorithm = new NeatEvolutionAlgorithm<NeatGenome>(_eaParams, speciationStrategy, complexityRegulationStrategy);

            genomeDecoder = new SharpNeat.Decoders.Neat.NeatGenomeDecoder(SharpNeat.Decoders.NetworkActivationScheme.CreateAcyclicScheme());

            var genomeFactory = new NeatGenomeFactory(InputCount, OutputCount, _neatGenomeParams);
            var genomeList = genomeFactory.CreateGenomeList(populationSize, 0);

            neatEvolutionAlgorithm.Initialize(evaluator, genomeFactory, genomeList);

            return neatEvolutionAlgorithm;
        }
    }

    class BattleEvaluator<TGenome> : IGenomeListEvaluator<TGenome>
            where TGenome : class, IGenome<TGenome>
    {
        public ulong EvaluationCount { get; set; }

        public bool StopConditionSatisfied { get; set; }

        public void Evaluate(IList<TGenome> genomeList)
        {

        }

        public void Reset()
        {

        }
    }
}