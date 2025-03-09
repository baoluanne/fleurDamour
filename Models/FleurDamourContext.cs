using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace fleurDamour.Models;

public partial class FleurDamourContext : DbContext
{
    public FleurDamourContext()
    {
    }

    public FleurDamourContext(DbContextOptions<FleurDamourContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<CommentDetail> CommentDetails { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderDetail> OrderDetails { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ShoppingCart> ShoppingCarts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=LAPTOP-I0FSNP77;uid=sa;pwd=1234;Database=fleurDamour;Trusted_Connection=False;Encrypt=False");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Uid).HasName("PK__Account__DD7012647FE1E6DD");

            entity.ToTable("Account");

            entity.Property(e => e.Uid).HasColumnName("uid");
            entity.Property(e => e.AccountName).HasMaxLength(255);
            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.DateOfBirth).HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.LogDate).HasColumnType("datetime");
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(255);
            entity.Property(e => e.PicAccount).HasMaxLength(255);
            entity.Property(e => e.Role).HasMaxLength(255);
            entity.Property(e => e.UserName).HasMaxLength(255);

            entity.HasMany(d => d.Idorders).WithMany(p => p.Uids)
                .UsingEntity<Dictionary<string, object>>(
                    "History",
                    r => r.HasOne<Order>().WithMany()
                        .HasForeignKey("Idorder")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__History__IDOrder__47E69B3D"),
                    l => l.HasOne<Account>().WithMany()
                        .HasForeignKey("Uid")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__History__uid__46F27704"),
                    j =>
                    {
                        j.HasKey("Uid", "Idorder").HasName("PK__History__78BBAEC945C2E1E3");
                        j.ToTable("History");
                        j.IndexerProperty<int>("Uid").HasColumnName("uid");
                        j.IndexerProperty<string>("Idorder")
                            .HasMaxLength(255)
                            .HasColumnName("IDOrder");
                    });
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Idcategory).HasName("PK__Category__1AA1EC6623E28743");

            entity.ToTable("Category");

            entity.Property(e => e.Idcategory)
                .HasMaxLength(255)
                .HasColumnName("IDCategory");
            entity.Property(e => e.NameCategory).HasMaxLength(255);
            entity.Property(e => e.PicCategory).HasMaxLength(255);

            entity.HasMany(d => d.Idproducts).WithMany(p => p.Idcategories)
                .UsingEntity<Dictionary<string, object>>(
                    "ProductCategory",
                    r => r.HasOne<Product>().WithMany()
                        .HasForeignKey("Idproduct")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__ProductCa__IDPro__3D690CCA"),
                    l => l.HasOne<Category>().WithMany()
                        .HasForeignKey("Idcategory")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__ProductCa__IDCat__3E5D3103"),
                    j =>
                    {
                        j.HasKey("Idcategory", "Idproduct").HasName("PK__ProductC__9E88E171BAFCC851");
                        j.ToTable("ProductCategory");
                        j.IndexerProperty<string>("Idcategory")
                            .HasMaxLength(255)
                            .HasColumnName("IDCategory");
                        j.IndexerProperty<string>("Idproduct")
                            .HasMaxLength(255)
                            .HasColumnName("IDProduct");
                    });
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.Idcomments).HasName("PK__Comments__C64EE1E1F624F655");

            entity.Property(e => e.Idcomments)
                .HasMaxLength(255)
                .HasColumnName("IDComments");
            entity.Property(e => e.DateComments).HasColumnType("datetime");
            entity.Property(e => e.Idproduct)
                .HasMaxLength(255)
                .HasColumnName("IDProduct");
            entity.Property(e => e.StrComments).HasMaxLength(255);
            entity.Property(e => e.Uid).HasColumnName("uid");

            entity.HasOne(d => d.IdproductNavigation).WithMany(p => p.Comments)
                .HasForeignKey(d => d.Idproduct)
                .HasConstraintName("FK__Comments__IDProd__34D3C6C9");

            entity.HasOne(d => d.UidNavigation).WithMany(p => p.Comments)
                .HasForeignKey(d => d.Uid)
                .HasConstraintName("FK__Comments__uid__33DFA290");
        });

        modelBuilder.Entity<CommentDetail>(entity =>
        {
            entity.HasKey(e => e.CommentDetailsId).HasName("PK__CommentD__40EFB4F67357C747");

            entity.Property(e => e.CommentDetailsId)
                .HasMaxLength(255)
                .HasColumnName("CommentDetailsID");
            entity.Property(e => e.Idcomments)
                .HasMaxLength(255)
                .HasColumnName("IDComments");
            entity.Property(e => e.StrCommentDetail).HasMaxLength(255);
            entity.Property(e => e.StrDateCommentDetail).HasColumnType("datetime");
            entity.Property(e => e.Uid).HasColumnName("uid");

            entity.HasOne(d => d.IdcommentsNavigation).WithMany(p => p.CommentDetails)
                .HasForeignKey(d => d.Idcomments)
                .HasConstraintName("FK__CommentDe__IDCom__37B03374");

            entity.HasOne(d => d.UidNavigation).WithMany(p => p.CommentDetails)
                .HasForeignKey(d => d.Uid)
                .HasConstraintName("FK__CommentDeta__uid__38A457AD");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Idorder).HasName("PK__Order__5CBBCADBDDFEE870");

            entity.ToTable("Order");

            entity.Property(e => e.Idorder)
                .HasMaxLength(255)
                .HasColumnName("IDOrder");
            entity.Property(e => e.OrderDay).HasColumnType("datetime");
            entity.Property(e => e.Uid).HasColumnName("uid");

            entity.HasOne(d => d.UidNavigation).WithMany(p => p.Orders)
                .HasForeignKey(d => d.Uid)
                .HasConstraintName("FK__Order__uid__41399DAE");
        });

        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.Idorder)
                .HasMaxLength(255)
                .HasColumnName("IDOrder");
            entity.Property(e => e.Idproduct)
                .HasMaxLength(255)
                .HasColumnName("IDProduct");

            entity.HasOne(d => d.IdorderNavigation).WithMany()
                .HasForeignKey(d => d.Idorder)
                .HasConstraintName("FK__OrderDeta__IDOrd__44160A59");

            entity.HasOne(d => d.IdproductNavigation).WithMany()
                .HasForeignKey(d => d.Idproduct)
                .HasConstraintName("FK__OrderDeta__IDPro__4321E620");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Idproduct).HasName("PK__Product__4290D1797EA8172C");

            entity.ToTable("Product");

            entity.Property(e => e.Idproduct)
                .HasMaxLength(255)
                .HasColumnName("IDProduct");
            entity.Property(e => e.ImgProduct).HasMaxLength(255);
            entity.Property(e => e.InfoProduct).HasMaxLength(255);
            entity.Property(e => e.NameProduct).HasMaxLength(255);
        });

        modelBuilder.Entity<ShoppingCart>(entity =>
        {
            entity.HasKey(e => new { e.Uid, e.Idproduct }).HasName("PK__Shopping__59591F73F45E244E");

            entity.ToTable("ShoppingCart");

            entity.Property(e => e.Uid).HasColumnName("uid");
            entity.Property(e => e.Idproduct)
                .HasMaxLength(255)
                .HasColumnName("IDProduct");
            entity.Property(e => e.AddDate).HasColumnType("datetime");

            entity.HasOne(d => d.IdproductNavigation).WithMany(p => p.ShoppingCarts)
                .HasForeignKey(d => d.Idproduct)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ShoppingC__IDPro__4BB72C21");

            entity.HasOne(d => d.UidNavigation).WithMany(p => p.ShoppingCarts)
                .HasForeignKey(d => d.Uid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ShoppingCar__uid__4AC307E8");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
