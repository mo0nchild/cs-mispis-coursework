using System;
using System.Collections.Generic;

namespace WebApplication.Models;

public partial class Resourceorder
{
    public int Resourceorderid { get; set; }

    public int Count { get; set; }

    public string Resourcename { get; set; } = null!;

    public int Builderid { get; set; }

    public virtual Builder Builder { get; set; } = null!;
}
