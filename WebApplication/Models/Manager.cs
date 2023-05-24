using System;
using System.Collections.Generic;

namespace WebApplication.Models;

public partial class Manager
{
    public int Managerid { get; set; }

    public int Employeeid { get; set; }

    public bool? Isadmin { get; set; }

    public virtual ICollection<Builder> Builders { get; set; } = new List<Builder>();

    public virtual Employee Employee { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
