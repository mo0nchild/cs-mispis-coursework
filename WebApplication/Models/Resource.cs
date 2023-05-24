using System;
using System.Collections.Generic;

namespace WebApplication.Models;

public partial class Resource
{
    public int Resourceid { get; set; }

    public string Resourcename { get; set; } = null!;

    public int? Count { get; set; }

    public decimal? Priceforone { get; set; }

    public int Providerid { get; set; }

    public virtual Resourceprovider Provider { get; set; } = null!;
}
