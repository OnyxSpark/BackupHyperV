using Microsoft.EntityFrameworkCore;
using System;
using System.IO;

namespace DB
{
    public class BackupDbContext : DbContext
    {
        public DbSet<BackupHistory> History { get; set; }

        public DbSet<Hypervisor> Hypervisors { get; set; }

        public DbSet<VirtualMachine> VirtualMachines { get; set; }


        public BackupDbContext() { }

        public BackupDbContext(DbContextOptions options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // to apply migrations

                string myDocs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string connStr = File.ReadAllText($"{myDocs}\\DevSecrets\\ConnStrings\\DevServer1.txt");

                optionsBuilder.UseSqlServer(connStr);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BackupHistory>()
                        .HasOne<VirtualMachine>(vm => vm.VirtualMachine)
                        .WithMany(h => h.HistoryRecords)
                        .HasForeignKey(k => k.VirtualMachineId)
                        .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
