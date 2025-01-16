using System;
using Microsoft.EntityFrameworkCore;
using HotelReservation.Models;

namespace HotelReservation.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext()
        {
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public virtual DbSet<Client> Clients { get; set; }
        public virtual DbSet<Reservation> Reservations { get; set; }
        public virtual DbSet<Room> Rooms { get; set; }
        public virtual DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\Yassir\\Documents\\HotelMangementSystem.mdf;Integrated Security=True;Connect Timeout=30;Encrypt=True");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Client>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Clients__6EC2B6C03CB0D642");

                entity.HasIndex(e => e.Email, "UQ__Clients__AB6E61642429BA87").IsUnique();

                entity.Property(e => e.Id).HasColumnName("id_client");
                entity.Property(e => e.Address).HasColumnType("text");
                entity.Property(e => e.Email)
                    .HasMaxLength(255)
                    .IsUnicode(false);
                entity.Property(e => e.FirstName)
                    .HasMaxLength(100)
                    .IsUnicode(false);
                entity.Property(e => e.LastName)
                    .HasMaxLength(100)
                    .IsUnicode(false);
                entity.Property(e => e.Phone)
                    .HasMaxLength(15)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Reservation>(entity =>
            {
                entity.HasKey(e => e.ReservationId).HasName("Reservation_Table_Reservation_ID_PK");

                entity.Property(e => e.ReservationId).HasColumnName("Reservation_ID");
                entity.Property(e => e.ReservationClientId).HasColumnName("Reservation_Client_ID");
                entity.Property(e => e.ReservationIn).HasColumnName("Reservation_In");
                entity.Property(e => e.ReservationOut).HasColumnName("Reservation_Out");
                entity.Property(e => e.ReservationRoomNumber).HasColumnName("Reservation_Room_Number");
                entity.Property(e => e.ReservationRoomType)
                    .HasMaxLength(6)
                    .IsUnicode(false)
                    .HasColumnName("Reservation_Room_Type");

                entity.HasOne(d => d.ReservationClient)
                    .WithMany(p => p.Reservations)
                    .HasForeignKey(d => d.ReservationClientId)
                    .OnDelete(DeleteBehavior.ClientSetNull) 
                    .HasConstraintName("Reservation_Table_Client_ID_FK");

                entity.HasOne(d => d.ReservationRoomNumberNavigation)
                    .WithMany(p => p.Reservations)
                    .HasForeignKey(d => d.ReservationRoomNumber)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Reservation_Table_Room_Number_FK");
            });


            modelBuilder.Entity<Room>(entity =>
            {
                entity.HasKey(e => e.RoomNumber).HasName("Room_Table_Room_Number_PK");

                entity.HasIndex(e => e.RoomPhone, "UQ__Rooms__9E15527483B139AA").IsUnique();

                entity.Property(e => e.RoomNumber).HasColumnName("Room_Number");
                entity.Property(e => e.DateCreated)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnName("Date_Created");
                entity.Property(e => e.RoomCapacity).HasColumnName("Room_Capacity");
                entity.Property(e => e.RoomFree)
                    .HasMaxLength(3)
                    .IsUnicode(false)
                    .HasColumnName("Room_Free");
                entity.Property(e => e.RoomPhone)
                    .HasMaxLength(15)
                    .IsUnicode(false)
                    .HasColumnName("Room_Phone");
                entity.Property(e => e.RoomPrice)
                    .HasColumnType("decimal(10, 2)")
                    .HasColumnName("Room_Price");
                entity.Property(e => e.RoomStatus)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("Room_Status");
                entity.Property(e => e.RoomType)
                    .HasMaxLength(6)
                    .IsUnicode(false)
                    .HasColumnName("Room_Type");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__users__3213E83F41C90FC8");

                entity.ToTable("users");

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.DateRegister).HasColumnName("date_register");
                entity.Property(e => e.Password)
                    .IsUnicode(false)
                    .HasColumnName("password");
                entity.Property(e => e.Role)
                    .IsUnicode(false)
                    .HasColumnName("role");
                entity.Property(e => e.Status)
                    .IsUnicode(false)
                    .HasColumnName("status");
                entity.Property(e => e.Username)
                    .IsUnicode(false)
                    .HasColumnName("username");
            });
        }
    }
}
