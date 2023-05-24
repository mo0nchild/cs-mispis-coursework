using System;
using System.Collections.Generic;

namespace WebApplication.Models;

public partial class Buildingorder
{
    public int Buildingid { get; set; }

    public int Builderid { get; set; }

    public int Orderid { get; set; }

    public virtual Builder Building { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;
}
