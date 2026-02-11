using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Gestion_Universitaire.Models;

namespace Gestion_Universitaire.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Etudiant> Etudiants { get; set; }
        public DbSet<Professeur> Professeurs { get; set; }
        public DbSet<Cours> Cours { get; set; }
        public DbSet<Inscription> Inscriptions { get; set; }
        public DbSet<Note> Notes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Etudiant>()
                .HasIndex(e => e.Matricule)
                .IsUnique();

            modelBuilder.Entity<Professeur>()
                .HasIndex(p => p.Matricule)
                .IsUnique();

            modelBuilder.Entity<Cours>()
                .HasIndex(c => c.Code)
                .IsUnique();

            modelBuilder.Entity<Inscription>()
                .HasOne(i => i.Etudiant)
                .WithMany(e => e.Inscriptions)
                .HasForeignKey(i => i.EtudiantId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Inscription>()
                .HasOne(i => i.Cours)
                .WithMany(c => c.Inscriptions)
                .HasForeignKey(i => i.CoursId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Note>()
                .HasOne(n => n.Etudiant)
                .WithMany(e => e.Notes)
                .HasForeignKey(n => n.EtudiantId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Note>()
                .HasOne(n => n.Cours)
                .WithMany(c => c.Notes)
                .HasForeignKey(n => n.CoursId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Note>()
                .Property(n => n.Valeur)
                .HasPrecision(4, 2);
        }
    }
}
