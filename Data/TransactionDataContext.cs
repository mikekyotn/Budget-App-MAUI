using Budget_App_MAUI.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Budget_App_MAUI.Data
{
    public class TransactionDataContext:DbContext
    {
        public DbSet<Transaction> Transactions { get; set; }

        public TransactionDataContext(DbContextOptions<TransactionDataContext> options) : base(options) 
        {
        
        }
    }
}
