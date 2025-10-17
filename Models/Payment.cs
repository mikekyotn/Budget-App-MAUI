//using Java.Time;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Budget_App_MAUI.Models
{
    public class Payment
    {
        //[Key]
        public Guid Id { get; set; }
        public PaymentType Type { get; set; }
        public int DayOfMonthDue { get; set; }
        public PaymentMonth Month { get; set; }
        public int Year { get; set; } = DateTime.Now.Year; //default to current year
        public string? Description { get; set; }
        public string? Category { get; set; }
        public string? Comments { get; set; }
        public bool IsPaid { get; set; }
        public decimal AmountEstimated { get; set; }
        public decimal AmountActual { get; set; }
        //[ObservableProperty] Guid id;
        //[ObservableProperty] PaymentType type;
        //[ObservableProperty] int dayOfMonthDue;
        //[ObservableProperty] PaymentMonth month;
        //[ObservableProperty] int year;
        //[ObservableProperty] string? description;
        //[ObservableProperty] string? category;
        //[ObservableProperty] string? comments;
        //[ObservableProperty] bool isPaid;
        //[ObservableProperty] decimal amountEstimated;
        //[ObservableProperty] decimal amountActual;

        //constructor to create a new payment with just the Guid
        public Payment()
        {
            Id = Guid.NewGuid();
        }
        //constructor to create a new payment that accepts a new Id instead of creates one, with all details or defaults 
        public Payment(Guid newId, PaymentMonth month, int year, PaymentType type = PaymentType.Expense, int dayDue = 1, string description = "Update", decimal amtEstimated = 0.00m,
            string category = "Undefined", string comments = "None", bool isPaid = false, decimal amtActual = 0.00m)
        {
            Id = newId;
            Type = type;
            DayOfMonthDue = dayDue;
            Month = month;
            Year = year;
            Description = description;
            Category = category;
            Comments = comments;
            IsPaid = isPaid;
            AmountEstimated = amtEstimated;
            AmountActual = amtActual;
            
        }
    }
        
    public enum PaymentMonth
    {
        TEMPLATE, January, February, March, April, May, June,
        July, August, September, October, November, December
    }

    public enum  PaymentType
    {
        Income, Expense, Transfer, Investment
    }


}
