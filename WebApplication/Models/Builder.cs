using System;
using System.Collections.Generic;

namespace WebApplication.Models;

public partial class Builder
{
    public int Builderid { get; set; }

    public int Employeeid { get; set; }

    public int? Managerid { get; set; }

    public virtual ICollection<Buildingorder> Buildingorders { get; set; } = new List<Buildingorder>();

    public virtual Employee Employee { get; set; } = null!;

    public virtual Manager? Manager { get; set; }

    public virtual ICollection<Resourceorder> Resourceorders { get; set; } = new List<Resourceorder>();
}
