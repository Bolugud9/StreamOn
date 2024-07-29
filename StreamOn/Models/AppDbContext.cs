using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;

namespace Videos.Models;

public class AppDbContext: DbContext
{
	public DbSet<Account> accounts { get; set; }
	public DbSet<Movie> movies { get; set; }
    public DbSet<MovieWatched> movieWatched { get; set; }

    public AppDbContext()
	{

	}

	public AppDbContext(DbContextOptions<AppDbContext> options):base(options)
	{

	}

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
		optionsBuilder.UseMySQL("server=localhost;port=3306;user=root;password=;database=streaming");
    }

	protected override async void OnModelCreating(ModelBuilder modelBuilder)
	{
        modelBuilder.Entity<Movie>()
            .HasOne(b => b.publisherAccount)
            .WithMany(b => b.PublishedMovies)
            .HasForeignKey(b => b.PublisherId);


        modelBuilder.Entity<MovieWatched>()
            .HasOne(b => b.watcherAccount)
            .WithMany(b => b.MovieWatcheds)
            .HasForeignKey(b => b.UserId);

        modelBuilder.Entity<Account>().HasData(
			new Account
			{
				email = "admin@email.com",
				Id = System.Guid.NewGuid(),
				password = HashPassword("admin"),
                isAdmin = true,
                isPublisher = false,
			}
		);


        modelBuilder.Entity<Account>(entity =>
        {
            entity.Property(e => e.Id).IsRequired();
            entity.Property(e => e.email).IsRequired();
            entity.Property(e => e.password).IsRequired();
            entity.Property(e => e.isAdmin).HasDefaultValue(false);
            entity.Property(e => e.isPublisher).HasDefaultValue(false);
        });

        modelBuilder.Entity<MovieWatched>(entity =>
        {
            entity.Property(e => e.id).ValueGeneratedOnAdd();//.HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            entity.Property(e => e.MovieId).IsRequired();
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.liked).HasDefaultValue(false);
            entity.Property(e => e.disliked).HasDefaultValue(false);
        });

        modelBuilder.Entity<Movie>(entity =>
        {
            entity.Property(e => e.MovieId).IsRequired();
            entity.Property(e => e.PublisherId).IsRequired();
            entity.Property(e => e.MovieName).IsRequired();
            entity.Property(e => e.MovieLink).IsRequired();
            entity.Property(e => e.ImageLink).IsRequired();
            entity.Property(e => e.ReleaseDate).IsRequired();
            entity.Property(e => e.PublishedDate).HasDefaultValue(DateTime.Now);
            entity.Property(e => e.rating).HasDefaultValue(0);
            entity.Property(e => e.approved).HasDefaultValue(false);
            entity.Property(e => e.genre).IsRequired();
            entity.Property(e => e.views).HasDefaultValue(0);
            entity.Property(e => e.like).HasDefaultValue(0);
            entity.Property(e => e.dislike).HasDefaultValue(0);
        });
    }

    private string HashPassword(string password)
    {
        // Generate a random salt
        byte[] salt = new byte[16];
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        // Create a password hasher using PBKDF2
        var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);

        // Combine salt and hashed password
        byte[] hash = pbkdf2.GetBytes(20);
        byte[] hashedPassword = new byte[36];
        Array.Copy(salt, 0, hashedPassword, 0, 16);
        Array.Copy(hash, 0, hashedPassword, 16, 20);

        // Convert the combined salt and hashed password to Base64
        string base64HashedPassword = Convert.ToBase64String(hashedPassword);

        return base64HashedPassword;
    }
}

