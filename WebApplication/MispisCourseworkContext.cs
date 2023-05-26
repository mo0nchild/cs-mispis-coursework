using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using WebApplication.Models;

namespace WebApplication;

public partial class MispisCourseworkContext : DbContext
{
    public MispisCourseworkContext()
    {
    }

    public MispisCourseworkContext(DbContextOptions<MispisCourseworkContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<Authorization> Authorizations { get; set; }

    public virtual DbSet<Builder> Builders { get; set; }

    public virtual DbSet<Buildingorder> Buildingorders { get; set; }

    public virtual DbSet<Client> Clients { get; set; }

    public virtual DbSet<Delivery> Deliveries { get; set; }

    public virtual DbSet<Deliveryman> Deliverymen { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<Manager> Managers { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Resource> Resources { get; set; }

    public virtual DbSet<Resourceorder> Resourceorders { get; set; }

    public virtual DbSet<Resourceprovider> Resourceproviders { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Server=localhost;Database=mispis_coursework;Username=postgres;Password=prolodgy778");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Accountid).HasName("accounts_pkey");

            entity.ToTable("accounts");

            entity.HasIndex(e => e.Authorizationid, "accounts_authorizationid_key").IsUnique();

            entity.Property(e => e.Accountid).HasColumnName("accountid");
            entity.Property(e => e.Address)
                .HasMaxLength(50)
                .HasColumnName("address");
            entity.Property(e => e.Authorizationid).HasColumnName("authorizationid");
            entity.Property(e => e.Birthday)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("birthday");
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.Patronymic)
                .HasMaxLength(50)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("patronymic");
            entity.Property(e => e.Phonenumber)
                .HasMaxLength(50)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("phonenumber");
            entity.Property(e => e.Profiletype)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Client'::character varying")
                .HasColumnName("profiletype");
            entity.Property(e => e.Surname)
                .HasMaxLength(50)
                .HasColumnName("surname");

            entity.HasOne(d => d.Authorization).WithOne(p => p.Account)
                .HasForeignKey<Account>(d => d.Authorizationid)
                .HasConstraintName("accounts_authorizationid_fkey");
        });

        modelBuilder.Entity<Authorization>(entity =>
        {
            entity.HasKey(e => e.Authorizationid).HasName("authorization_pkey");

            entity.ToTable("authorization");

            entity.Property(e => e.Authorizationid).HasColumnName("authorizationid");
            entity.Property(e => e.Accesscode)
                .HasMaxLength(50)
                .HasColumnName("accesscode");
            entity.Property(e => e.Login)
                .HasMaxLength(50)
                .HasColumnName("login");
            entity.Property(e => e.Password)
                .HasMaxLength(50)
                .HasColumnName("password");
        });

        modelBuilder.Entity<Builder>(entity =>
        {
            entity.HasKey(e => e.Builderid).HasName("builders_pkey");

            entity.ToTable("builders");

            entity.Property(e => e.Builderid).HasColumnName("builderid");
            entity.Property(e => e.Employeeid).HasColumnName("employeeid");
            entity.Property(e => e.Managerid).HasColumnName("managerid");

            entity.HasOne(d => d.Employee).WithMany(p => p.Builders)
                .HasForeignKey(d => d.Employeeid)
                .HasConstraintName("builders_employeeid_fkey");

            entity.HasOne(d => d.Manager).WithMany(p => p.Builders)
                .HasForeignKey(d => d.Managerid)
                .HasConstraintName("builders_managerid_fkey");
        });

        modelBuilder.Entity<Buildingorder>(entity =>
        {
            entity.HasKey(e => e.Buildingid).HasName("buildingorders_pkey");

            entity.ToTable("buildingorders");

            entity.Property(e => e.Buildingid).HasColumnName("buildingid");
            entity.Property(e => e.Builderid).HasColumnName("builderid");
            entity.Property(e => e.Orderid).HasColumnName("orderid");

            entity.HasOne(d => d.Builder).WithMany(p => p.Buildingorders)
                .HasForeignKey(d => d.Builderid)
                .HasConstraintName("buildingorders_builders_builderid_fk");

            entity.HasOne(d => d.Order).WithMany(p => p.Buildingorders)
                .HasForeignKey(d => d.Orderid)
                .HasConstraintName("buildingorders_orderid_fkey");
        });

        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.Clientid).HasName("clients_pkey");

            entity.ToTable("clients");

            entity.Property(e => e.Clientid).HasColumnName("clientid");
            entity.Property(e => e.Accountid).HasColumnName("accountid");
            entity.Property(e => e.Emailaddress)
                .HasMaxLength(50)
                .HasColumnName("emailaddress");

            entity.HasOne(d => d.Account).WithMany(p => p.Clients)
                .HasForeignKey(d => d.Accountid)
                .HasConstraintName("clients_accountid_fkey");
        });

        modelBuilder.Entity<Delivery>(entity =>
        {
            entity.HasKey(e => e.Deliveryid).HasName("deliveries_pkey");

            entity.ToTable("deliveries");

            entity.HasIndex(e => e.Deliverymanid, "deliveries_deliverymanid_key").IsUnique();

            entity.HasIndex(e => e.Orderid, "deliveries_orderid_key").IsUnique();

            entity.Property(e => e.Deliveryid).HasColumnName("deliveryid");
            entity.Property(e => e.Address)
                .HasMaxLength(50)
                .HasColumnName("address");
            entity.Property(e => e.Deliverymanid).HasColumnName("deliverymanid");
            entity.Property(e => e.Dispatchtime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dispatchtime");
            entity.Property(e => e.Orderid).HasColumnName("orderid");

            entity.HasOne(d => d.Deliveryman).WithOne(p => p.Delivery)
                .HasForeignKey<Delivery>(d => d.Deliverymanid)
                .HasConstraintName("deliveries_deliverymanid_fkey");

            entity.HasOne(d => d.Order).WithOne(p => p.Delivery)
                .HasForeignKey<Delivery>(d => d.Orderid)
                .HasConstraintName("deliveries_orderid_fkey");
        });

        modelBuilder.Entity<Deliveryman>(entity =>
        {
            entity.HasKey(e => e.Deliverymanid).HasName("deliverymen_pkey");

            entity.ToTable("deliverymen");

            entity.Property(e => e.Deliverymanid).HasColumnName("deliverymanid");
            entity.Property(e => e.Driverlicence)
                .HasMaxLength(50)
                .HasColumnName("driverlicence");
            entity.Property(e => e.Employeeid).HasColumnName("employeeid");
            entity.Property(e => e.Transporttype)
                .HasMaxLength(50)
                .HasColumnName("transporttype");

            entity.HasOne(d => d.Employee).WithMany(p => p.Deliverymen)
                .HasForeignKey(d => d.Employeeid)
                .HasConstraintName("deliverymen_employeeid_fkey");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Employeeid).HasName("employees_pkey");

            entity.ToTable("employees");

            entity.Property(e => e.Employeeid).HasColumnName("employeeid");
            entity.Property(e => e.Accountid).HasColumnName("accountid");
            entity.Property(e => e.Activated)
                .HasDefaultValueSql("false")
                .HasColumnName("activated");
            entity.Property(e => e.Education)
                .HasMaxLength(50)
                .HasColumnName("education");
            entity.Property(e => e.Gender)
                .HasMaxLength(50)
                .HasColumnName("gender");
            entity.Property(e => e.Passport).HasMaxLength(50);
            entity.Property(e => e.Salary)
                .HasColumnType("money")
                .HasColumnName("salary");

            entity.HasOne(d => d.Account).WithMany(p => p.Employees)
                .HasForeignKey(d => d.Accountid)
                .HasConstraintName("employees_accountid_fkey");
        });

        modelBuilder.Entity<Manager>(entity =>
        {
            entity.HasKey(e => e.Managerid).HasName("managers_pkey");

            entity.ToTable("managers");

            entity.Property(e => e.Managerid).HasColumnName("managerid");
            entity.Property(e => e.Employeeid).HasColumnName("employeeid");
            entity.Property(e => e.Isadmin)
                .HasDefaultValueSql("false")
                .HasColumnName("isadmin");

            entity.HasOne(d => d.Employee).WithMany(p => p.Managers)
                .HasForeignKey(d => d.Employeeid)
                .HasConstraintName("managers_employeeid_fkey");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.Messageid).HasName("messages_pkey");

            entity.ToTable("messages");

            entity.Property(e => e.Messageid).HasColumnName("messageid");
            entity.Property(e => e.Messagebody).HasColumnName("messagebody");
            entity.Property(e => e.Orderid).HasColumnName("orderid");
            entity.Property(e => e.Sender)
                .HasDefaultValueSql("false")
                .HasColumnName("sender");
            entity.Property(e => e.Sendtime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("sendtime");

            entity.HasOne(d => d.Order).WithMany(p => p.Messages)
                .HasForeignKey(d => d.Orderid)
                .HasConstraintName("messages_orderid_fkey");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Orderid).HasName("orders_pkey");

            entity.ToTable("orders");

            entity.Property(e => e.Orderid).HasColumnName("orderid");
            entity.Property(e => e.Clientid).HasColumnName("clientid");
            entity.Property(e => e.Height).HasColumnName("height");
            entity.Property(e => e.Managerid).HasColumnName("managerid");
            entity.Property(e => e.Ordertime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("ordertime");
            entity.Property(e => e.Packetcount).HasColumnName("packetcount");
            entity.Property(e => e.State)
                .HasMaxLength(50)
                .HasColumnName("state");
            entity.Property(e => e.Width).HasColumnName("width");
            entity.Property(e => e.Windowtype)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Стандарт'::character varying")
                .HasColumnName("windowtype");

            entity.HasOne(d => d.Client).WithMany(p => p.Orders)
                .HasForeignKey(d => d.Clientid)
                .HasConstraintName("orders_clientid_fkey");

            entity.HasOne(d => d.Manager).WithMany(p => p.Orders)
                .HasForeignKey(d => d.Managerid)
                .HasConstraintName("orders_managerid_fkey");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Paymentid).HasName("payments_pkey");

            entity.ToTable("payments");

            entity.Property(e => e.Paymentid).HasColumnName("paymentid");
            entity.Property(e => e.Bankaccount)
                .HasMaxLength(50)
                .HasColumnName("bankaccount");
            entity.Property(e => e.Bankprovider)
                .HasMaxLength(50)
                .HasColumnName("bankprovider");
            entity.Property(e => e.Clientid).HasColumnName("clientid");
            entity.Property(e => e.Cvv)
                .HasMaxLength(3)
                .HasColumnName("cvv");
            entity.Property(e => e.Endtime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("endtime");

            entity.HasOne(d => d.Client).WithMany(p => p.Payments)
                .HasForeignKey(d => d.Clientid)
                .HasConstraintName("payments_clientid_fkey");
        });

        modelBuilder.Entity<Resource>(entity =>
        {
            entity.HasKey(e => e.Resourceid).HasName("resources_pkey");

            entity.ToTable("resources");

            entity.Property(e => e.Resourceid).HasColumnName("resourceid");
            entity.Property(e => e.Count)
                .HasDefaultValueSql("0")
                .HasColumnName("count");
            entity.Property(e => e.Priceforone)
                .HasDefaultValueSql("0.0")
                .HasColumnName("priceforone");
            entity.Property(e => e.Providerid).HasColumnName("providerid");
            entity.Property(e => e.Resourcename)
                .HasMaxLength(50)
                .HasColumnName("resourcename");

            entity.HasOne(d => d.Provider).WithMany(p => p.Resources)
                .HasForeignKey(d => d.Providerid)
                .HasConstraintName("resources_providerid_fkey");
        });

        modelBuilder.Entity<Resourceorder>(entity =>
        {
            entity.HasKey(e => e.Resourceorderid).HasName("resourceorders_pkey");

            entity.ToTable("resourceorders");

            entity.Property(e => e.Resourceorderid).HasColumnName("resourceorderid");
            entity.Property(e => e.Builderid).HasColumnName("builderid");
            entity.Property(e => e.Count).HasColumnName("count");
            entity.Property(e => e.Resourcename)
                .HasMaxLength(50)
                .HasColumnName("resourcename");

            entity.HasOne(d => d.Builder).WithMany(p => p.Resourceorders)
                .HasForeignKey(d => d.Builderid)
                .HasConstraintName("resourceorders_builderid_fkey");
        });

        modelBuilder.Entity<Resourceprovider>(entity =>
        {
            entity.HasKey(e => e.Providerid).HasName("resourceproviders_pkey");

            entity.ToTable("resourceproviders");

            entity.Property(e => e.Providerid).HasColumnName("providerid");
            entity.Property(e => e.Accountid).HasColumnName("accountid");
            entity.Property(e => e.Address)
                .HasMaxLength(50)
                .HasColumnName("address");
            entity.Property(e => e.Companyname)
                .HasMaxLength(50)
                .HasColumnName("companyname");

            entity.HasOne(d => d.Account).WithMany(p => p.Resourceproviders)
                .HasForeignKey(d => d.Accountid)
                .HasConstraintName("resourceproviders_accountid_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
