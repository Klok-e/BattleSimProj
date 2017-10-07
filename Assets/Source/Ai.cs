using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battle_sim_imp
{
    public abstract class AAi
    {
        public int inputsNum;
        public int outputsNum;

        public AAi(int inps, int outps)
        {
            inputsNum = inps;
            outputsNum = outps;
        }

        public abstract double[] Predict(double[] inp);
    }
}
