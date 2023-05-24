using System;
using System.Collections.Generic;

namespace WebApplication.Models;

public partial class Client
{
    public int Clientid { get; set; }

    public string Emailaddress { get; set; } = null!;

    public int Accountid { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
