using System;
using System.Collections.Generic;

namespace WebApplication.Models;

public partial class Payment
{
    public int Paymentid { get; set; }

    public string Bankprovider { get; set; } = null!;

    public DateTime Endtime { get; set; }

    public string Cvv { get; set; } = null!;

    public string Bankaccount { get; set; } = null!;

    public int Clientid { get; set; }

    public virtual Client Client { get; set; } = null!;
}
