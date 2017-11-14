/* ***************************************************************************
 * This file is part of SharpNEAT - Evolution of Neural Networks.
 *
 * Copyright 2004-2016 Colin Green (sharpneat@gmail.com)
 *
 * SharpNEAT is free software; you can redistribute it and/or modify
 * it under the terms of The MIT License (MIT).
 *
 * You should have received a copy of the MIT License
 * along with SharpNEAT; if not, see https://opensource.org/licenses/MIT.
 */

namespace SharpNeat.EvolutionAlgorithms
{
    /// <summary>
    /// Parameters specific to the NEAT evolution algorithm.
    /// </summary>
    public class NeatEvolutionAlgorithmParameters
    {
        #region Constants

        private const int DefaultSpecieCount = 10;
        private const double DefaultElitismProportion = 0.2;
        private const double DefaultSelectionProportion = 0.2;

        private const double DefaultOffspringAsexualProportion = 0.5;
        private const double DefaultOffspringSexualProportion = 0.5;
        private const double DefaultInterspeciesMatingProportion = 0.01;

        private const int DefaultDestFitnessMovingAverageHistoryLength = 100;
        private const int DefgaultMeanSpecieChampFitnessMovingAverageHistoryLength = 100;
        private const int DefaultComplexityMovingAverageHistoryLength = 100;

        #endregion Constants

        #region Instance Fields

        private int _specieCount;
        private double _elitismProportion;
        private double _selectionProportion;

        private double _offspringAsexualProportion;
        private double _offspringSexualProportion;
        private double _interspeciesMatingProportion;

        private int _bestFitnessMovingAverageHistoryLength;
        private int _meanSpecieChampFitnessMovingAverageHistoryLength;
        private int _complexityMovingAverageHistoryLength;

        #endregion Instance Fields

        #region Constructor

        /// <summary>
        /// Constructs with the default parameters.
        /// </summary>
        public NeatEvolutionAlgorithmParameters()
        {
            _specieCount = DefaultSpecieCount;
            _elitismProportion = DefaultElitismProportion;
            _selectionProportion = DefaultSelectionProportion;

            _offspringAsexualProportion = DefaultOffspringAsexualProportion;
            _offspringSexualProportion = DefaultOffspringSexualProportion;
            _interspeciesMatingProportion = DefaultInterspeciesMatingProportion;

            _bestFitnessMovingAverageHistoryLength = DefaultDestFitnessMovingAverageHistoryLength;
            _meanSpecieChampFitnessMovingAverageHistoryLength = DefgaultMeanSpecieChampFitnessMovingAverageHistoryLength;
            _complexityMovingAverageHistoryLength = DefaultComplexityMovingAverageHistoryLength;

            NormalizeProportions();
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        public NeatEvolutionAlgorithmParameters(NeatEvolutionAlgorithmParameters copyFrom)
        {
            _specieCount = copyFrom._specieCount;
            _elitismProportion = copyFrom._elitismProportion;
            _selectionProportion = copyFrom._selectionProportion;

            _offspringAsexualProportion = copyFrom._offspringAsexualProportion;
            _offspringSexualProportion = copyFrom._offspringSexualProportion;
            _interspeciesMatingProportion = copyFrom._interspeciesMatingProportion;

            _bestFitnessMovingAverageHistoryLength = copyFrom._bestFitnessMovingAverageHistoryLength;
            _meanSpecieChampFitnessMovingAverageHistoryLength = copyFrom._meanSpecieChampFitnessMovingAverageHistoryLength;
            _complexityMovingAverageHistoryLength = copyFrom._complexityMovingAverageHistoryLength;
        }

        #endregion Constructor

        #region Properties

        /// <summary>
        /// Gets or sets the specie count.
        /// </summary>
        public int SpecieCount
        {
            get { return _specieCount; }
            set { _specieCount = value; }
        }

        /// <summary>
        /// Gets or sets the elitism proportion.
        /// We sort specie genomes by fitness and keep the top N%, the other genomes are
        /// removed to make way for the offspring.
        /// </summary>
        public double ElitismProportion
        {
            get { return _elitismProportion; }
            set { _elitismProportion = value; }
        }

        /// <summary>
        /// Gets or sets the selection proportion.
        /// We sort specie genomes by fitness and select parent genomes for producing offspring from
        /// the top N%. Selection is performed prior to elitism being applied, therefore selecting from more
        /// genomes than will be made elite is possible.
        /// </summary>
        public double SelectionProportion
        {
            get { return _selectionProportion; }
            set { _selectionProportion = value; }
        }

        /// <summary>
        /// Gets or sets the proportion of offspring to be produced from asexual reproduction (mutation).
        /// </summary>
        public double OffspringAsexualProportion
        {
            get { return _offspringAsexualProportion; }
            set { _offspringAsexualProportion = value; }
        }

        /// <summary>
        /// Gets or sets the proportion of offspring to be produced from sexual reproduction.
        /// </summary>
        public double OffspringSexualProportion
        {
            get { return _offspringSexualProportion; }
            set { _offspringSexualProportion = value; }
        }

        /// <summary>
        /// Gets or sets the proportion of sexual reproductions that will use genomes from different species.
        /// </summary>
        public double InterspeciesMatingProportion
        {
            get { return _interspeciesMatingProportion; }
            set { _interspeciesMatingProportion = value; }
        }

        /// <summary>
        /// Gets or sets the history buffer length used for calculating the best fitness moving average.
        /// </summary>
        public int BestFitnessMovingAverageHistoryLength
        {
            get { return _bestFitnessMovingAverageHistoryLength; }
            set { _bestFitnessMovingAverageHistoryLength = value; }
        }

        /// <summary>
        /// Gets or sets the history buffer length used for calculating the mean specie champ fitness
        /// moving average.
        /// </summary>
        public int MeanSpecieChampFitnessMovingAverageHistoryLength
        {
            get { return _meanSpecieChampFitnessMovingAverageHistoryLength; }
            set { _meanSpecieChampFitnessMovingAverageHistoryLength = value; }
        }

        /// <summary>
        /// Gets or sets the history buffer length used for calculating the mean genome complexity moving
        /// average.
        /// </summary>
        public int ComplexityMovingAverageHistoryLength
        {
            get { return _complexityMovingAverageHistoryLength; }
            set { _complexityMovingAverageHistoryLength = value; }
        }

        #endregion Properties

        #region Private Methods

        /// <summary>
        /// Normalize the sexual and asexual proportions such that their sum equals 1.
        /// </summary>
        private void NormalizeProportions()
        {
            double total = _offspringAsexualProportion + _offspringSexualProportion;
            _offspringAsexualProportion = _offspringAsexualProportion / total;
            _offspringSexualProportion = _offspringSexualProportion / total;
        }

        #endregion Private Methods

        #region Public Methods

        /// <summary>
        /// Creates a set of parameters based on the current set and that are suitable for the simplifying
        /// phase of the evolution algorithm when running with complexity regulation enabled.
        /// </summary>
        public NeatEvolutionAlgorithmParameters CreateSimplifyingParameters()
        {
            // Make a copy of the current 'complexifying' parameters (as required by complexity regulation)
            // and modify the copy to be suitable for simplification. Basically we disable sexual reproduction
            // while in simplifying mode to prevent proliferation of structure through sexual reproduction.
            NeatEvolutionAlgorithmParameters eaParams = new NeatEvolutionAlgorithmParameters(this);
            eaParams._offspringAsexualProportion = 1.0;
            eaParams._offspringSexualProportion = 0.0;
            return eaParams;
        }

        #endregion Public Methods
    }
}