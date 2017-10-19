using SharpNeat.Phenomes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public abstract class AAi
{
    public IBlackBox network;
    public Random random;
    public int inputsNum;
    public int outputsNum;

    public AAi(int inps, int outps, Random rand)
    {
        random = rand;
        inputsNum = inps;
        outputsNum = outps;
    }

    public abstract double[] Predict(double[] inp);
}

public class NeuralAI : AAi
{
    public NeuralAI(int inps, int outps, Random rand) : base(inps, outps, rand)
    {

    }

    public override double[] Predict(double[] inp)
    {
        Debug.Assert(network.InputCount == inputsNum);
        Debug.Assert(network.OutputCount == outputsNum);

        var arr = network.InputSignalArray;
        for (int i = 0; i < inputsNum; i++)
        {
            arr[i] = inp[i];
        }
        network.Activate();

        var prediction =new double[outputsNum];
        arr = network.OutputSignalArray;
        for (int i = 0; i < outputsNum; i++)
        {
            prediction[i] = arr[i];
        }
        return prediction;
    }
}

public class RandomAi : AAi
{
    public RandomAi(int inps, int outps, Random rand) : base(inps, outps, rand)
    {

    }

    public override double[] Predict(double[] inp)
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
