using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VityazevFinalWork.Solution.Models
{
    internal class TrainModels
    {
        public class InputData
        {
            public float date { get; set; }
            public float amount { get; set; }
        }

        public class PredictData
        {
            [ColumnName("Score")]
            public float amount { get; set; }
        }
    }
}
