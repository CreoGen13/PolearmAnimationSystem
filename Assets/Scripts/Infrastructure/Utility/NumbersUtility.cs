using UnityEngine;

namespace Infrastructure.Utility
{
    public class NumbersUtility
    {
        public static float RoundFloat(float value, int digits)
        {
            var powValue = Mathf.Pow(10, digits);
            var result = value * powValue;
            result = Mathf.Round(result) / powValue;
            return result;
        }
    }
}