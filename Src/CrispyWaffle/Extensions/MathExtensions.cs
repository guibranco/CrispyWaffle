namespace CrispyWaffle.Extensions
{
    /// <summary>
    /// The math extensions class.
    /// </summary>
    public static class MathExtensions
    {
        /// <summary>
        /// Rounds down.
        /// </summary>
        /// <param name="currentValue">The current value.</param>
        /// <param name="multipleOf">The multiple of.</param>
        /// <returns>Int32.</returns>
        public static int RoundDown(this int currentValue, int multipleOf = 10)
        {
            return currentValue - currentValue % multipleOf;
        }

        /// <summary>
        /// Rounds up.
        /// </summary>
        /// <param name="currentValue">The current value.</param>
        /// <param name="multipleOf">The multiple of.</param>
        /// <param name="forceDifferentValue">The force different value.</param>
        /// <returns>Int32.</returns>
        public static int RoundUp(this int currentValue, int multipleOf = 10, bool forceDifferentValue = false)
        {
            return !forceDifferentValue &&
                currentValue % multipleOf == 0
                ? currentValue
                : multipleOf - currentValue % multipleOf + currentValue;
        }

        /// <summary>
        /// Rounds the best.
        /// </summary>
        /// <param name="currentValue">The current value.</param>
        /// <param name="multipleOf">The multiple of.</param>
        /// <param name="forceDifferentValue">The force different value.</param>
        /// <returns>Int32.</returns>
        public static int RoundBest(
            this int currentValue,
            int multipleOf = 10,
            bool forceDifferentValue = false)
        {
            if (!forceDifferentValue &&
                currentValue % multipleOf == 0)
            {
                return currentValue;
            }

            var left = currentValue % multipleOf;
            if (left == 1)
            {
                return currentValue - 1;
            }

            var half = multipleOf / 2;

            return left >= half
                       ? currentValue - left + multipleOf
                       : currentValue - left;
        }
    }
}