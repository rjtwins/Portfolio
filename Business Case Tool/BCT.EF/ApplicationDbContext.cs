#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

using BCT.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BCT.EF;

public class ApplicationDbContext : DbContext
{
	public DbSet<Project> Projects { get; set; }
	public DbSet<Company> Companies { get; set; }
	public DbSet<User> Users { get; set; }
	public DbSet<DoubleValue> DoubleValues { get; set; }
    //public DbSet<Domain.Entities.Attribute> Attributes { get; set; }
    public DbSet<ProjectGridWizard> ProjectGridWizards { get; set; }
    public DbSet<Scenario> SensitivityScenarios { get; set; }
    public DbSet<Tag> Tags { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
		: base(options)
	{
		
	}
    #region Override save changes to set time stamps:
    public override int SaveChanges()
    {
        SetTimeStamps();
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        SetTimeStamps();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetTimeStamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        SetTimeStamps();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void SetTimeStamps()
    {
        var entries = ChangeTracker.Entries<IdModel>();

        foreach (var e in entries)
        {
            if (e.State == EntityState.Added)
            {
                e.Entity.RecordCreatedAt = DateTime.UtcNow;
                e.Entity.RecordUpdatedAt = DateTime.UtcNow;
            }
            else if (e.State == EntityState.Modified)
            {
                e.Entity.RecordUpdatedAt = DateTime.UtcNow;
            }
        }
    }
    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

        #region Join table definitions (many-to-many relationships)
        modelBuilder.Entity<Company>()
			.HasMany(c => c.Users)                // Company has many Users
			.WithMany(u => u.Companies)           // User has many Companies
			.UsingEntity<Dictionary<string, object>>(
				"CompanyUser",                    // The name of the join table (optional)
				j => j.HasOne<User>().WithMany().HasForeignKey("UserId").OnDelete(DeleteBehavior.Cascade),  // Define the foreign key for Users
				j => j.HasOne<Company>().WithMany().HasForeignKey("CompanyId").OnDelete(DeleteBehavior.Cascade) // Define the foreign key for Companies
			);

		// Configure one-to-many relationship between User and Company
		modelBuilder.Entity<Company>()
			.HasOne(c => c.Creator)      // Company has one Creator (User)
			.WithMany(u => u.CreatedCompanies) // User can have many created Companies
			.HasForeignKey(c => c.CreatorId) // CreatorId is the foreign key in Company
			.OnDelete(DeleteBehavior.SetNull); // Prevent cascade delete if needed

        // Configure many-to-many relationship between Project and Tag
        modelBuilder.Entity<Tag>()
            .HasMany(t => t.Projects)
            .WithMany(p => p.Tags)
            .UsingEntity<Dictionary<string, object>>(
                "ProjectTag",                    // The name of the join table (optional)
                j => j.HasOne<Project>().WithMany().HasForeignKey("ProjectId").OnDelete(DeleteBehavior.Cascade),  // Define the foreign key for Projects
                j => j.HasOne<Tag>().WithMany().HasForeignKey("TagId").OnDelete(DeleteBehavior.Cascade) // Define the foreign key for Tags
            );
		#endregion

		#region includes
		#endregion

		#region  Ignores
		modelBuilder.Entity<User>()
			.Ignore(u => u.Roles)
			.Ignore(u => u.Email)
			.Ignore(u => u.UpdatedAt)
			.Ignore(u => u.EmailVerified)
			.Ignore(u => u.CreatedAt)
			.Ignore(u => u.LastLogin)
			.Ignore(u => u.LastIP)
			.Ignore(u => u.LoginCount);

        modelBuilder.Entity<StringValue>()
            .Ignore(u => u.DynamicValue);
        modelBuilder.Entity<BoolValue>()
            .Ignore(u => u.DynamicValue);
        modelBuilder.Entity<DoubleValue>()
            .Ignore(u => u.DynamicValue);
        #endregion

        #region text length
        //Project
        modelBuilder.Entity<Project>()
			.Property(p => p.Name)
			.HasMaxLength(100)
			.HasColumnType("nvarchar(100)");
		
		//Company
		modelBuilder.Entity<Company>()
			.Property(p => p.Name)
			.HasMaxLength(100)
			.HasColumnType("nvarchar(100)");
		
		//User
		modelBuilder.Entity<User>()
			.Property(p => p.Name)
			.HasMaxLength(50)
			.HasColumnType("nvarchar(50)");
			
		modelBuilder.Entity<User>()
			.Property(p => p.AuthId)
			.HasMaxLength(128)
			.HasColumnType("nvarchar(128)");
			
		//OverTimeValue		
		modelBuilder.Entity<DoubleValue>()
			.Property(p => p.Year)
			.HasMaxLength(100)
			.HasColumnType("varchar(100)");
		#endregion
		
		#region constraints
		modelBuilder.Entity<User>()
			.HasIndex(x => x.AuthId)
			.IsUnique();

        modelBuilder.Entity<Tag>()
            .HasIndex(x => new { x.CompanyId, x.Text })
            .IsUnique();
        #endregion
    }
}

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
