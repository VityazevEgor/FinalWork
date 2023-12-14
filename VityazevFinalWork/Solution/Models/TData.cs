using System;
using VityazevFinalWork.Solution.Misc;

namespace VityazevFinalWork.Solution.Models
{
    internal class TData
    {
        // месяц к которому относятья данные
        public required DateTime date { get; set; }
        // количество заявок на кредит
        public required double amount { get; set; }

        public TrainModels.InputData ToInpData()
        {
            return new TrainModels.InputData { 
                amount = Convert.ToSingle(this.amount),
                date = Shared.ConvertToUnixTime(this.date),
            };
        }
    }
}
