using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace APTSWebApp.Models
{
    public partial class APTS_RZA_Context : DbContext
    {
        public APTS_RZA_Context()
        {
        }

        public APTS_RZA_Context(DbContextOptions<APTS_RZA_Context> options)
            : base(options)
        {
        }

        public virtual DbSet<Actions> Actions { get; set; }
        public virtual DbSet<DeviceTypes> DeviceTypes { get; set; }
        public virtual DbSet<Devices> Devices { get; set; }
        public virtual DbSet<MigrationHistory> MigrationHistory { get; set; }
        public virtual DbSet<OicTs> OicTs { get; set; }
        public virtual DbSet<PowerObjectDevices> PowerObjectDevices { get; set; }
        public virtual DbSet<PowerObjects> PowerObjects { get; set; }
        public virtual DbSet<PowerSystems> PowerSystems { get; set; }
        public virtual DbSet<PrimaryEquipmentDevices> PrimaryEquipmentDevices { get; set; }
        public virtual DbSet<PrimaryEquipmentPowerObjects> PrimaryEquipmentPowerObjects { get; set; }
        public virtual DbSet<PrimaryEquipments> PrimaryEquipments { get; set; }
        public virtual DbSet<ReceivedTsvalues> ReceivedTsvalues { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Actions>(entity =>
            {
                entity.Property(e => e.Dtime)
                    .HasColumnName("DTime")
                    .HasColumnType("datetime");
            });

            modelBuilder.Entity<DeviceTypes>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Name).IsRequired();
            });

            modelBuilder.Entity<Devices>(entity =>
            {
                entity.HasKey(e => e.Shifr)
                    .HasName("PK_dbo.Devices");

                entity.HasIndex(e => e.DeviceTypeId)
                    .HasName("IX_DeviceTypeId");

                entity.Property(e => e.Shifr)
                    .HasMaxLength(128)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.Name).IsRequired();

                entity.HasOne(d => d.DeviceType)
                    .WithMany(p => p.Devices)
                    .HasForeignKey(d => d.DeviceTypeId)
                    .HasConstraintName("FK_dbo.Devices_dbo.DeviceTypes_DeviceTypeId");
            });

            modelBuilder.Entity<MigrationHistory>(entity =>
            {
                entity.HasKey(e => new { e.MigrationId, e.ContextKey })
                    .HasName("PK_dbo.__MigrationHistory");

                entity.ToTable("__MigrationHistory");

                entity.Property(e => e.MigrationId).HasMaxLength(150);

                entity.Property(e => e.ContextKey).HasMaxLength(300);

                entity.Property(e => e.Model).IsRequired();

                entity.Property(e => e.ProductVersion)
                    .IsRequired()
                    .HasMaxLength(32);
            });

            modelBuilder.Entity<OicTs>(entity =>
            {
                entity.ToTable("OicTS");

                entity.HasIndex(e => e.DeviceShifr)
                    .HasName("IX_DeviceShifr");

                entity.Property(e => e.DeviceShifr).HasMaxLength(128);

                entity.Property(e => e.IsOiсTs).HasColumnName("IsOiсTS");

                entity.Property(e => e.IsStatusTs).HasColumnName("IsStatusTS");

                entity.HasOne(d => d.DeviceShifrNavigation)
                    .WithMany(p => p.OicTs)
                    .HasForeignKey(d => d.DeviceShifr)
                    .HasConstraintName("FK_dbo.OikTS_dbo.Devices_Device_Shifr");
            });

            modelBuilder.Entity<PowerObjectDevices>(entity =>
            {
                entity.HasKey(e => new { e.PowerObjectId, e.DeviceShifr })
                    .HasName("PK_dbo.PowerObjectDevices");

                entity.HasIndex(e => e.DeviceShifr)
                    .HasName("IX_Device_Shifr");

                entity.HasIndex(e => e.PowerObjectId)
                    .HasName("IX_PowerObject_Id");

                entity.Property(e => e.PowerObjectId).HasColumnName("PowerObject_Id");

                entity.Property(e => e.DeviceShifr)
                    .HasColumnName("Device_Shifr")
                    .HasMaxLength(128);

                entity.HasOne(d => d.DeviceShifrNavigation)
                    .WithMany(p => p.PowerObjectDevices)
                    .HasForeignKey(d => d.DeviceShifr)
                    .HasConstraintName("FK_dbo.PowerObjectDevices_dbo.Devices_Device_Shifr");

                entity.HasOne(d => d.PowerObject)
                    .WithMany(p => p.PowerObjectDevices)
                    .HasForeignKey(d => d.PowerObjectId)
                    .HasConstraintName("FK_dbo.PowerObjectDevices_dbo.PowerObjects_PowerObject_Id");
            });

            modelBuilder.Entity<PowerObjects>(entity =>
            {
                entity.HasIndex(e => e.PowerSystemId)
                    .HasName("IX_PowerSystemId");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Name).IsRequired();

                entity.HasOne(d => d.PowerSystem)
                    .WithMany(p => p.PowerObjects)
                    .HasForeignKey(d => d.PowerSystemId)
                    .HasConstraintName("FK_dbo.PowerObjects_dbo.PowerSystems_PowerSystemId");
            });

            modelBuilder.Entity<PowerSystems>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Name).IsRequired();
            });

            modelBuilder.Entity<PrimaryEquipmentDevices>(entity =>
            {
                entity.HasKey(e => new { e.PrimaryEquipmentShifr, e.DeviceShifr })
                    .HasName("PK_dbo.PrimaryEquipmentDevices");

                entity.HasIndex(e => e.DeviceShifr)
                    .HasName("IX_Device_Shifr");

                entity.HasIndex(e => e.PrimaryEquipmentShifr)
                    .HasName("IX_PrimaryEquipment_Shifr");

                entity.Property(e => e.PrimaryEquipmentShifr)
                    .HasColumnName("PrimaryEquipment_Shifr")
                    .HasMaxLength(128);

                entity.Property(e => e.DeviceShifr)
                    .HasColumnName("Device_Shifr")
                    .HasMaxLength(128);

                entity.HasOne(d => d.DeviceShifrNavigation)
                    .WithMany(p => p.PrimaryEquipmentDevices)
                    .HasForeignKey(d => d.DeviceShifr)
                    .HasConstraintName("FK_dbo.PrimaryEquipmentDevices_dbo.Devices_Device_Shifr");

                entity.HasOne(d => d.PrimaryEquipmentShifrNavigation)
                    .WithMany(p => p.PrimaryEquipmentDevices)
                    .HasForeignKey(d => d.PrimaryEquipmentShifr)
                    .HasConstraintName("FK_dbo.PrimaryEquipmentDevices_dbo.PrimaryEquipments_PrimaryEquipment_Shifr");
            });

            modelBuilder.Entity<PrimaryEquipmentPowerObjects>(entity =>
            {
                entity.HasKey(e => new { e.PrimaryEquipmentShifr, e.PowerObjectId })
                    .HasName("PK_dbo.PrimaryEquipmentPowerObjects");

                entity.HasIndex(e => e.PowerObjectId)
                    .HasName("IX_PowerObject_Id");

                entity.HasIndex(e => e.PrimaryEquipmentShifr)
                    .HasName("IX_PrimaryEquipment_Shifr");

                entity.Property(e => e.PrimaryEquipmentShifr)
                    .HasColumnName("PrimaryEquipment_Shifr")
                    .HasMaxLength(128);

                entity.Property(e => e.PowerObjectId).HasColumnName("PowerObject_Id");

                entity.HasOne(d => d.PowerObject)
                    .WithMany(p => p.PrimaryEquipmentPowerObjects)
                    .HasForeignKey(d => d.PowerObjectId)
                    .HasConstraintName("FK_dbo.PowerObjectPrimaryEquipments_dbo.PowerObjects_PowerObject_Id");

                entity.HasOne(d => d.PrimaryEquipmentShifrNavigation)
                    .WithMany(p => p.PrimaryEquipmentPowerObjects)
                    .HasForeignKey(d => d.PrimaryEquipmentShifr)
                    .HasConstraintName("FK_dbo.PowerObjectPrimaryEquipments_dbo.PrimaryEquipments_PrimaryEquipment_Shifr");
            });

            modelBuilder.Entity<PrimaryEquipments>(entity =>
            {
                entity.HasKey(e => e.Shifr)
                    .HasName("PK_dbo.PrimaryEquipments");

                entity.Property(e => e.Shifr).HasMaxLength(128);

                entity.Property(e => e.Name).IsRequired();
            });

            modelBuilder.Entity<ReceivedTsvalues>(entity =>
            {
                entity.ToTable("ReceivedTSValues");

                entity.HasIndex(e => e.OicTsid)
                    .HasName("IX_OicTSId");

                entity.Property(e => e.Dt)
                    .HasColumnName("DT")
                    .HasColumnType("datetime");

                entity.Property(e => e.OicTsid).HasColumnName("OicTSId");

                entity.HasOne(d => d.OicTs)
                    .WithMany(p => p.ReceivedTsvalues)
                    .HasForeignKey(d => d.OicTsid)
                    .HasConstraintName("FK_dbo.ReceivedTSValues_dbo.OikTS_OikTSId");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
