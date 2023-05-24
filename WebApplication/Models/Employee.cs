using System;
using System.Collections.Generic;

namespace WebApplication.Models;

public partial class Employee
{
    public int Employeeid { get; set; }

    public string Education { get; set; } = null!;

    public string Gender { get; set; } = null!;

    public decimal Salary { get; set; }

    public string Passport { get; set; } = null!;

    public int Accountid { get; set; }

    public bool? Activated { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual ICollection<Builder> Builders { get; set; } = new List<Builder>();

    public virtual ICollection<Deliveryman> Deliverymen { get; set; } = new List<Deliveryman>();

    public virtual ICollection<Manager> Managers { get; set; } = new List<Manager>();
}
