using System;
using System.Collections.Generic;

namespace WebApplication.Models;

public partial class Authorization
{
    public int Authorizationid { get; set; }

    public string Login { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Accesscode { get; set; } = null!;

    public virtual Account? Account { get; set; }
}
