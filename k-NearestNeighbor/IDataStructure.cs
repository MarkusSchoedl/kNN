using System;
using System.Collections.Generic;
using System.Text;

namespace k_NearestNeighbor
{
    interface IDataStructure
    {
        bool Equals(IDataStructure ds);

        string ToString();
    }
}
