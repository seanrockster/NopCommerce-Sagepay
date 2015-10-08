﻿using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;
using Nop.Core;
using Nop.Data;
using System.Collections.Generic;
using System;


namespace Nop.Plugin.Payments.SagePayServer.Data
{
    public class SagePayServerTransactionObjectContext: DbContext, IDbContext
    {
        public SagePayServerTransactionObjectContext(string nameOrConnectionString) : base(nameOrConnectionString) {}
        
        #region Implementation of IDbContext

        public new IDbSet<TEntity> Set<TEntity>() where TEntity : BaseEntity
        {
            return base.Set<TEntity>();
        }

        public IList<TEntity> ExecuteStoredProcedureList<TEntity>(string commandText, params object[] parameters) where TEntity : BaseEntity, new()
        {
            throw new NotImplementedException();
        }

        #endregion

        protected override void OnModelCreating(DbModelBuilder modelBuilder) {
            //Add references to the mapping files
            modelBuilder.Configurations.Add(new SagePayServerTransactionMap());

            //Disable EdmMetaDataGeneration
            modelBuilder.Conventions.Remove<IncludeMetadataConvention>();
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();

            base.OnModelCreating(modelBuilder);
        }

        public IEnumerable<TElement> SqlQuery<TElement>(string sql, params object[] parameters)
        {
            return null;
        }

        public int ExecuteSqlCommand(string sql, bool doNotEnsureTransaction = false, int? timeout = null, params object[] parameters)
        {
            return 0;
        }

        public void Detach(object entity)
        {
            throw new NotImplementedException();
        }

        public bool ProxyCreationEnabled
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public bool AutoDetectChangesEnabled
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }


        public void InstallSchema()
        {
            ((IObjectContextAdapter)this).ObjectContext.CreateDatabaseScript();
        }
    }

}
