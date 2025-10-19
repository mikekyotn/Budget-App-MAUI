using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Budget_App_MAUI.Models
{
    public class MonthIndex
    {
        public int Id { get; set; }
        public int Year { get; set; }

        public decimal AvailableFunds { get; set; }
        public PaymentMonth Month { get; set; }
    }
}
