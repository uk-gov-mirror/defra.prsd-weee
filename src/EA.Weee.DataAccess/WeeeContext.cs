﻿namespace EA.Weee.DataAccess
{
    using System.Data.Common;
    using System.Data.Entity;
    using System.Threading;
    using System.Threading.Tasks;
    using Domain;
    using Domain.Admin;
    using Domain.Audit;
    using Domain.Lookup;
    using Domain.Organisation;
    using Domain.Producer;
    using Domain.Scheme;
    using Prsd.Core;
    using Prsd.Core.DataAccess.Extensions;
    using Prsd.Core.Domain;
    using Prsd.Core.Domain.Auditing;
    using StoredProcedure;

    public class WeeeContext : DbContext
    {
        private readonly IUserContext userContext;
        private readonly IEventDispatcher dispatcher;

        public virtual DbSet<AuditLog> AuditLogs { get; set; }

        public virtual DbSet<Organisation> Organisations { get; set; }

        public virtual DbSet<User> Users { get; set; }

        public virtual DbSet<Country> Countries { get; set; }
        
        public virtual DbSet<OrganisationUser> OrganisationUsers { get; set; }

        public virtual DbSet<UKCompetentAuthority> UKCompetentAuthorities { get; set; }

        public virtual DbSet<MemberUpload> MemberUploads { get; set; }

        public virtual DbSet<MemberUploadError> MemberUploadErrors { get; set; }

        public virtual DbSet<Scheme> Schemes { get; set; }

        public virtual DbSet<RegisteredProducer> RegisteredProducers { get; set; }

        public virtual DbSet<ProducerSubmission> ProducerSubmissions { get; set; }

        public virtual DbSet<SystemData> SystemData { get; set; }

        public virtual DbSet<MigratedProducer> MigratedProducers { get; set; }

        public virtual DbSet<ChargeBandAmount> ChargeBandAmounts { get; set; }

        public virtual DbSet<CompetentAuthorityUser> CompetentAuthorityUsers { get; set; }

        public virtual DbSet<DataReturnsUpload> DataReturnsUploads { get; set; }

        public virtual DbSet<DataReturnsUploadError> DataReturnsUploadErrors { get; set; }

        public virtual IStoredProcedures StoredProcedures { get; private set; }

        public WeeeContext(IUserContext userContext, IEventDispatcher dispatcher)
            : base("name=Weee.DefaultConnection")
        {
            this.userContext = userContext;
            this.dispatcher = dispatcher;

            Database.SetInitializer<WeeeContext>(null);

            StoredProcedures = new StoredProcedures(this);
        }

        /// <summary>
        /// This constructor should only be used for integration testing.
        /// </summary>
        /// <param name="userContext"></param>
        /// <param name="connection"></param>
        public WeeeContext(IUserContext userContext, IEventDispatcher dispatcher, DbConnection connection)
            : base(connection, false)
        {
            this.userContext = userContext;
            this.dispatcher = dispatcher;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            var assembly = typeof(WeeeContext).Assembly;
            var coreAssembly = typeof(AuditorExtensions).Assembly;

            modelBuilder.Conventions.AddFromAssembly(assembly);
            modelBuilder.Configurations.AddFromAssembly(assembly);

            modelBuilder.Conventions.AddFromAssembly(coreAssembly);
            modelBuilder.Configurations.AddFromAssembly(coreAssembly);
        }

        public override int SaveChanges()
        {
            bool alreadyHasTransaction = (this.Database.CurrentTransaction != null);

            this.SetEntityId();
            this.AuditChanges(userContext.UserId);
            AuditEntity();
            int result;
            if (alreadyHasTransaction)
            {
                result = base.SaveChanges();

                Task.Run(() => this.DispatchEvents(dispatcher)).Wait();
            }
            else
            {
                using (var transaction = Database.BeginTransaction())
                {
                    result = base.SaveChanges();

                    Task.Run(() => this.DispatchEvents(dispatcher)).Wait();

                    transaction.Commit();
                }
            }

            return result;
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            bool alreadyHasTransaction = (this.Database.CurrentTransaction != null);

            this.SetEntityId();
            this.AuditChanges(userContext.UserId);
            AuditEntity();
            int result;
            if (alreadyHasTransaction)
            {
                result = await base.SaveChangesAsync(cancellationToken);

                await this.DispatchEvents(dispatcher);
            }
            else
            {
                using (var transaction = Database.BeginTransaction())
                {
                    result = await base.SaveChangesAsync(cancellationToken);

                    await this.DispatchEvents(dispatcher);

                    transaction.Commit();
                }
            }

            return result;
        }

        public void DeleteOnCommit(Entity entity)
        {
            Entry(entity).State = EntityState.Deleted;
        }

        private void AuditEntity()
        {
            foreach (var auditableEntity in ChangeTracker.Entries<IAuditableEntity>())
            {
                if (auditableEntity.State == EntityState.Added || auditableEntity.State == EntityState.Modified)
                {
                    auditableEntity.Entity.Date = SystemTime.UtcNow;
                    auditableEntity.Entity.UserId = userContext.UserId.ToString();
                }
            }
        }
    }
}