using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public abstract class AAi
{
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

public class RandomAi : AAi
{
    public RandomAi(int inps, int outps, Random rand) : base(inps, outps, rand)
    {

    }

    public override double[] Predict(double[] inp)
    {
        var pred = new double[outputsNum];
        for (int i = 0; i < pred.Length; i++)
        {
            pred[i] = random.NextDouble() * 20 - 10;
        }
        return pred;
    }
}
