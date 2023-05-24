using System;
using System.Collections.Generic;

namespace WebApplication.Models;

public partial class Delivery
{
    public int Deliveryid { get; set; }

    public string Address { get; set; } = null!;

    public DateTime Dispatchtime { get; set; }

    public int Deliverymanid { get; set; }

    public int Orderid { get; set; }

    public virtual Deliveryman Deliveryman { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;
}
