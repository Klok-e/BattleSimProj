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

using Redzen.Numerics;

namespace SharpNeat.Network
{
    /// <summary>
    /// S-shaped rectified linear activation unit (SReLU).
    /// From:
    ///    https://en.wikipedia.org/wiki/Activation_function
    ///    https://arxiv.org/abs/1512.07030 [Deep Learning with S-shaped Rectified Linear Activation Units]
    ///
    /// </summary>
    public class SReLU : IActivationFunction
    {
        /// <summary>
        /// Default instance provided as a public static field.
        /// </summary>
        public static readonly IActivationFunction __DefaultInstance = new SReLU();

        public string FunctionId => this.GetType().Name;

        public string FunctionString => "";

        public string FunctionDescription => "Leaky Rectified Linear Unit (ReLU)";

        public bool AcceptsAuxArgs => false;

        private double _tl = 0.001;
        private double _tr = 0.999;
        private double _a = 0.00001;

        /// <param name="tl">left threshold</param>
        /// <param name="tr">right threshold</param>
        /// <param name="a">multiplier if threshold is reached</param>
        public void SetParameters(double tl, double tr, double a)
        {
            _tl = tl;
            _tr = tr;
            _a = a;
        }

        public double Calculate(double x, double[] auxArgs)
        {
            double y;
            if (x > _tl && x < _tr)
            {
                y = x;
            }
            else if (x <= _tl)
            {
                y = _tl + (x - _tl) * _a;
            }
            else
            {
                y = _tr + (x - _tr) * _a;
            }

            return y;
        }

        public float Calculate(float x, float[] auxArgs)
        {
            float tl = (float)_tl; // threshold (left).
            float tr = (float)_tr; // threshold (right).
            float a = (float)_a;

            float y;
            if (x > tl && x < tr)
            {
                y = x;
            }
            else if (x <= tl)
            {
                y = tl + (x - tl) * a;
            }
            else
            {
                y = tr + (x - tr) * a;
            }

            return y;
        }

        public double[] GetRandomAuxArgs(XorShiftRandom rng, double connectionWeightRange)
        {
            throw new SharpNeatException("GetRandomAuxArgs() called on activation function that does not use auxiliary arguments.");
        }

        public void MutateAuxArgs(double[] auxArgs, XorShiftRandom rng, ZigguratGaussianSampler gaussianSampler, double connectionWeightRange)
        {
            throw new SharpNeatException("MutateAuxArgs() called on activation function that does not use auxiliary arguments.");
        }
    }
}
