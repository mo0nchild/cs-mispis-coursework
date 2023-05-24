using System;
using System.Collections.Generic;

namespace WebApplication.Models;

public partial class Account
{
    public int Accountid { get; set; }

    public string Surname { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Patronymic { get; set; }

    public string Address { get; set; } = null!;

    public string? Phonenumber { get; set; }

    public DateTime Birthday { get; set; }

    public int Authorizationid { get; set; }

    public string Profiletype { get; set; } = null!;

    public virtual Authorization Authorization { get; set; } = null!;

    public virtual ICollection<Client> Clients { get; set; } = new List<Client>();

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();

    public virtual ICollection<Resourceprovider> Resourceproviders { get; set; } = new List<Resourceprovider>();
}
