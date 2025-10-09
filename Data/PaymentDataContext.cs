using Budget_App_MAUI.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Budget_App_MAUI.Data
{
    public class PaymentDataContext:DbContext
    {
        public DbSet<Payment> Payments { get; set; }
        public DbSet<MonthIndex> MonthIndices { get; set; }

        public PaymentDataContext(DbContextOptions<PaymentDataContext> options) : base(options) 
        {
        
        }
    }
}
