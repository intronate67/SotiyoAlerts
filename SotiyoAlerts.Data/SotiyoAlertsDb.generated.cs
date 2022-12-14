//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
//
//     Produced by Entity Framework Visual Editor v4.1.2.0
//     Source:                    https://github.com/msawczyn/EFDesigner
//     Visual Studio Marketplace: https://marketplace.visualstudio.com/items?itemName=michaelsawczyn.EFDesigner
//     Documentation:             https://msawczyn.github.io/EFDesigner/
//     License (MIT):             https://github.com/msawczyn/EFDesigner/blob/master/LICENSE
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SotiyoAlerts.Data
{
	/// <summary>
	/// Sotiyo Alerts Database Context
	/// </summary>
	public partial class SotiyoAlertsDb : DbContext
	{
		#region DbSets

		/// <summary>
		/// Repository for global::SotiyoAlerts.Data.Models.Channel - Channels that have (or
		/// had previously) active filters
		/// </summary>
		public virtual Microsoft.EntityFrameworkCore.DbSet<global::SotiyoAlerts.Data.Models.Channel> Channels { get; set; }

		/// <summary>
		/// Repository for global::SotiyoAlerts.Data.Models.ChannelFilter - Filters for channels
		/// </summary>
		public virtual Microsoft.EntityFrameworkCore.DbSet<global::SotiyoAlerts.Data.Models.ChannelFilter> ChannelFilters { get; set; }

		/// <summary>
		/// Repository for global::SotiyoAlerts.Data.Models.Filter - Filter types
		/// </summary>
		public virtual Microsoft.EntityFrameworkCore.DbSet<global::SotiyoAlerts.Data.Models.Filter> Filters { get; set; }

		/// <summary>
		/// Repository for global::SotiyoAlerts.Data.Models.Guild - Servers that are (or have
		/// previously) used the bot.
		/// </summary>
		public virtual Microsoft.EntityFrameworkCore.DbSet<global::SotiyoAlerts.Data.Models.Guild> Guilds { get; set; }

		/// <summary>
		/// Repository for global::SotiyoAlerts.Data.Models.SubFilter - Sub filters
		/// </summary>
		public virtual Microsoft.EntityFrameworkCore.DbSet<global::SotiyoAlerts.Data.Models.SubFilter> SubFilters { get; set; }

		#endregion DbSets

		/// <summary>
		///     <para>
		///         Initializes a new instance of the <see cref="T:Microsoft.EntityFrameworkCore.DbContext" /> class using the specified options.
		///         The <see cref="M:Microsoft.EntityFrameworkCore.DbContext.OnConfiguring(Microsoft.EntityFrameworkCore.DbContextOptionsBuilder)" /> method will still be called to allow further
		///         configuration of the options.
		///     </para>
		/// </summary>
		/// <param name="options">The options for this context.</param>
		public SotiyoAlertsDb(DbContextOptions<SotiyoAlertsDb> options) : base(options)
		{
		}

		partial void CustomInit(DbContextOptionsBuilder optionsBuilder);

		partial void OnModelCreatingImpl(ModelBuilder modelBuilder);
		partial void OnModelCreatedImpl(ModelBuilder modelBuilder);

		/// <summary>
		///     Override this method to further configure the model that was discovered by convention from the entity types
		///     exposed in <see cref="T:Microsoft.EntityFrameworkCore.DbSet`1" /> properties on your derived context. The resulting model may be cached
		///     and re-used for subsequent instances of your derived context.
		/// </summary>
		/// <remarks>
		///     If a model is explicitly set on the options for this context (via <see cref="M:Microsoft.EntityFrameworkCore.DbContextOptionsBuilder.UseModel(Microsoft.EntityFrameworkCore.Metadata.IModel)" />)
		///     then this method will not be run.
		/// </remarks>
		/// <param name="modelBuilder">
		///     The builder being used to construct the model for this context. Databases (and other extensions) typically
		///     define extension methods on this object that allow you to configure aspects of the model that are specific
		///     to a given database.
		/// </param>
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);
			OnModelCreatingImpl(modelBuilder);

			modelBuilder.HasDefaultSchema("dbo");

			modelBuilder.Entity<global::SotiyoAlerts.Data.Models.Channel>()
			            .ToTable("Channels")
			            .HasKey(t => t.Id);
			modelBuilder.Entity<global::SotiyoAlerts.Data.Models.Channel>()
			            .Property(t => t.Id)
			            .ValueGeneratedNever()
			            .IsRequired();
			modelBuilder.Entity<global::SotiyoAlerts.Data.Models.Channel>()
			            .Property(t => t.GuildId)
			            .IsRequired();
			modelBuilder.Entity<global::SotiyoAlerts.Data.Models.Channel>()
			            .Property(t => t.Name)
			            .HasMaxLength(100)
			            .HasColumnType("nvarchar(100)")
			            .IsRequired();
			modelBuilder.Entity<global::SotiyoAlerts.Data.Models.Channel>()
			            .Property(t => t.CreateTime)
			            .IsRequired();
			modelBuilder.Entity<global::SotiyoAlerts.Data.Models.Channel>()
			            .HasMany<global::SotiyoAlerts.Data.Models.ChannelFilter>(p => p.ChannelFilters)
			            .WithOne(p => p.Channel);

			modelBuilder.Entity<global::SotiyoAlerts.Data.Models.ChannelFilter>()
			            .ToTable("ChannelFilters")
			            .HasKey(t => t.Id);
			modelBuilder.Entity<global::SotiyoAlerts.Data.Models.ChannelFilter>()
			            .Property(t => t.Id)
			            .ValueGeneratedOnAdd()
			            .IsRequired();
			modelBuilder.Entity<global::SotiyoAlerts.Data.Models.ChannelFilter>()
			            .Property(t => t.FilterId)
			            .IsRequired();
			modelBuilder.Entity<global::SotiyoAlerts.Data.Models.ChannelFilter>()
			            .Property(t => t.SubFilterId)
			            .IsRequired();
			modelBuilder.Entity<global::SotiyoAlerts.Data.Models.ChannelFilter>().HasIndex(t => t.SubFilterId);
			modelBuilder.Entity<global::SotiyoAlerts.Data.Models.ChannelFilter>()
			            .Property(t => t.ChannelId)
			            .IsRequired();
			modelBuilder.Entity<global::SotiyoAlerts.Data.Models.ChannelFilter>()
			            .Property(t => t.IsActive)
			            .IsRequired();
			modelBuilder.Entity<global::SotiyoAlerts.Data.Models.ChannelFilter>()
			            .Property(t => t.IsDeleted)
			            .IsRequired();
			modelBuilder.Entity<global::SotiyoAlerts.Data.Models.ChannelFilter>()
			            .Property(t => t.CreateTime)
			            .IsRequired();

			modelBuilder.Entity<global::SotiyoAlerts.Data.Models.Filter>()
			            .ToTable("Filters")
			            .HasKey(t => t.Id);
			modelBuilder.Entity<global::SotiyoAlerts.Data.Models.Filter>()
			            .Property(t => t.Id)
			            .ValueGeneratedOnAdd()
			            .IsRequired();
			modelBuilder.Entity<global::SotiyoAlerts.Data.Models.Filter>()
			            .Property(t => t.Name)
			            .HasMaxLength(250)
			            .IsRequired();
			modelBuilder.Entity<global::SotiyoAlerts.Data.Models.Filter>()
			            .Property(t => t.CreateTime)
			            .IsRequired();
			modelBuilder.Entity<global::SotiyoAlerts.Data.Models.Filter>()
			            .HasMany<global::SotiyoAlerts.Data.Models.ChannelFilter>(p => p.ChannelFilters)
			            .WithOne(p => p.Filter);
			modelBuilder.Entity<global::SotiyoAlerts.Data.Models.Filter>()
			            .HasMany<global::SotiyoAlerts.Data.Models.SubFilter>(p => p.SubFilters)
			            .WithOne(p => p.Filter);

			modelBuilder.Entity<global::SotiyoAlerts.Data.Models.Guild>()
			            .ToTable("Guilds")
			            .HasKey(t => t.Id);
			modelBuilder.Entity<global::SotiyoAlerts.Data.Models.Guild>()
			            .Property(t => t.Id)
			            .ValueGeneratedNever()
			            .IsRequired();
			modelBuilder.Entity<global::SotiyoAlerts.Data.Models.Guild>()
			            .Property(t => t.Name)
			            .HasMaxLength(100)
			            .HasColumnType("nvarchar(100)")
			            .IsRequired();
			modelBuilder.Entity<global::SotiyoAlerts.Data.Models.Guild>()
			            .Property(t => t.IsActive)
			            .IsRequired();
			modelBuilder.Entity<global::SotiyoAlerts.Data.Models.Guild>()
			            .Property(t => t.IsDeleted)
			            .IsRequired();
			modelBuilder.Entity<global::SotiyoAlerts.Data.Models.Guild>()
			            .Property(t => t.CreateTime)
			            .IsRequired();
			modelBuilder.Entity<global::SotiyoAlerts.Data.Models.Guild>()
			            .HasMany<global::SotiyoAlerts.Data.Models.Channel>(p => p.Channels)
			            .WithOne(p => p.Guild);

			modelBuilder.Entity<global::SotiyoAlerts.Data.Models.SubFilter>()
			            .ToTable("SubFilters")
			            .HasKey(t => t.Id);
			modelBuilder.Entity<global::SotiyoAlerts.Data.Models.SubFilter>()
			            .Property(t => t.Id)
			            .ValueGeneratedOnAdd()
			            .IsRequired();
			modelBuilder.Entity<global::SotiyoAlerts.Data.Models.SubFilter>()
			            .Property(t => t.FilterId)
			            .IsRequired();
			modelBuilder.Entity<global::SotiyoAlerts.Data.Models.SubFilter>()
			            .Property(t => t.Name)
			            .HasMaxLength(250)
			            .IsRequired();
			modelBuilder.Entity<global::SotiyoAlerts.Data.Models.SubFilter>()
			            .Property(t => t.CreateTime)
			            .IsRequired();

			OnModelCreatedImpl(modelBuilder);
		}
	}
}
