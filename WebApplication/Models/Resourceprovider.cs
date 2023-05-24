using System;
using System.Collections.Generic;

namespace WebApplication.Models;

public partial class Resourceprovider
{
    public int Providerid { get; set; }

    public string Companyname { get; set; } = null!;

    public string Address { get; set; } = null!;

    public int Accountid { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual ICollection<Resource> Resources { get; set; } = new List<Resource>();
}
