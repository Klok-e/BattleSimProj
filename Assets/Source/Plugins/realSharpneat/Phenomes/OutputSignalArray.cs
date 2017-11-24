namespace SharpNeat.Phenomes
{
    /// <summary>
    /// A SignalArray that applies the bounds interval [-1,1] to returned values.
    /// </summary>
    public class OutputSignalArray : SignalArray
    {
        #region Constructor

        /// <summary>
        /// Construct an OutputSignalArray that wraps the provided wrappedArray.
        /// </summary>
        public OutputSignalArray(double[] wrappedArray, int offset, int length) : base(wrappedArray, offset, length)
        {
        }

        #endregion Constructor

        #region Indexer

        /// <summary>
        /// Gets the value at the specified index, applying the bounds interval [-1,1] to the return value.
        /// </summary>
        /// <param name="index">The index of the value to retrieve.</param>
        /// <returns>A double.</returns>
        public override double this[int index]
        {
            get
            {
                // Apply bounds of [-1,1].
                double y = base[index];
                if (y < -1.0) y = -1.0;
                else if (y > 1.0) y = 1.0;
                return y;
            }
        }

        #endregion Indexer
    }
}
