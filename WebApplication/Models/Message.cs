using System;
using System.Collections.Generic;

namespace WebApplication.Models;

public partial class Message
{
    public int Messageid { get; set; }

    public string Messagebody { get; set; } = null!;

    public DateTime Sendtime { get; set; }

    public bool? Sender { get; set; }

    public int Orderid { get; set; }

    public virtual Order Order { get; set; } = null!;
}
