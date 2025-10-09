using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Budget_App_MAUI.Models;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Budget_App_MAUI.Messages
{
    public class TransactionUpdatedMessage:ValueChangedMessage<TransactMonth>
    {
        public TransactionUpdatedMessage(TransactMonth month) : base(month)
        {
        }
    }
}
