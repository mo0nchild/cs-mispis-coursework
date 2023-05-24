using System;
using System.Collections.Generic;

namespace WebApplication.Models;

public partial class Deliveryman
{
    public int Deliverymanid { get; set; }

    public string Driverlicence { get; set; } = null!;

    public string Transporttype { get; set; } = null!;

    public int Employeeid { get; set; }

    public virtual Delivery? Delivery { get; set; }

    public virtual Employee Employee { get; set; } = null!;
}
