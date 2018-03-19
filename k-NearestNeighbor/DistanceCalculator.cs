using System;
using System.Collections.Generic;
using System.Text;

namespace k_NearestNeighbor
{
    #region StrategyDesignPattern
    abstract class DistanceCalculator
    {
        public abstract float GetDistance(DataStructure wine1, DataStructure wine2);
    }

    class EuclideanDistanceStrategy : DistanceCalculator
    {
        public override float GetDistance(DataStructure wine1, DataStructure wine2)
        {
            float result = 0;
            float temp = 0;

            for (int i = 0; i < wine1.data.Count; i++)
            {
                temp = wine1.data[i] - wine2.data[i];
                result += temp * temp;
            }

            return (float)Math.Sqrt(result);
        }
    }

    class ManhattanDistanceStrategy : DistanceCalculator
    {
        public override float GetDistance(DataStructure wine1, DataStructure wine2)
        {
            float result = 0;


            for (int i = 0; i < wine1.data.Count; i++)
            {
                result += Math.Abs(wine1.data[i] - wine2.data[i]);
            }

            return result;
        }
    }

    class CorrectedEuclideanStrategy : DistanceCalculator
    {
        public override float GetDistance(DataStructure wine1, DataStructure wine2)
        {
            throw new NotImplementedException();
        }
    }
    #endregion
}
