using System;
using System.Collections.Generic;

namespace WebApplication.Models;

public partial class Order
{
    public int Orderid { get; set; }

    public int Packetcount { get; set; }

    public decimal Width { get; set; }

    public decimal Height { get; set; }

    public string State { get; set; } = null!;

    public DateTime Ordertime { get; set; }

    public int? Managerid { get; set; }

    public int Clientid { get; set; }

    public string Windowtype { get; set; } = null!;

    public virtual ICollection<Buildingorder> Buildingorders { get; set; } = new List<Buildingorder>();

    public virtual Client Client { get; set; } = null!;

    public virtual Delivery? Delivery { get; set; }

    public virtual Manager? Manager { get; set; }

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
}
