using Java.Time;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Budget_App_MAUI.Models
{
    public class Transaction
    {
        [Key]
        public Guid Id { get; set; }
        public string Type { get; set; }
        public int DayOfMonthDue { get; set; }
        public TransactMonth Month { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Comments { get; set; }        
        public bool IsPaid { get; set; }

    }
    public enum TransactMonth
    {
        TEMPLATE, January, February, March, April, May, June,
        July, August, September, October, November, December
    }
}
