using SharpNeat.Core;
using SharpNeat.Genomes.Neat;
using SharpNeat.Phenomes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ue = UnityEngine;

public class NeuralAI
{
    public Random random;
    public int inputsNum;
    public int outputsNum;

    public IBlackBox network { get; }

    public NeuralAI(int inps, int outps, Random rand, IBlackBox net)
    {
        random = rand;
        inputsNum = inps;
        outputsNum = outps;
        network = net;
    }

    public double[] Predict(double[] inp)
    {
        if (network != null)
        {
            Debug.Assert(network.InputCount == inputsNum);
            Debug.Assert(network.OutputCount == outputsNum);

            var arr = network.InputSignalArray;
            for (int i = 0; i < inputsNum; i++)
            {
                arr[i] = inp[i];
            }
            network.Activate();

            var prediction = new double[outputsNum];
            arr = network.OutputSignalArray;
            for (int i = 0; i < outputsNum; i++)
            {
                prediction[i] = arr[i];
            }
            return prediction;
        }
        else
        {
            //Ue.Debug.Log("Network is null; outputing random");
            return PredictRandom(inp);
        }
    }

    private double[] PredictRandom(double[] inp)
    {
        var pred = new double[outputsNum];
        pred[0] = random.NextDouble() * 10 - 5;
        for (int i = 1; i < pred.Length; i++)
        {
            pred[i] = random.NextDouble() * 2 - 1;
        }
        return pred;
    }
}
