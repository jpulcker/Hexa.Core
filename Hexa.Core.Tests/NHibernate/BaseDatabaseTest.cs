#region Header

// ===================================================================================
// Copyright 2010 HexaSystems Corporation
// ===================================================================================
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// ===================================================================================
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// See the License for the specific language governing permissions and
// ===================================================================================

#endregion Header

namespace Hexa.Core.Tests.Sql
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    using Core.Data;
    using Core.Domain;

    using Data;

    using Domain;

    using Logging;

    using NUnit.Framework;

    using Security;

    using Validation;

    public abstract class BaseDatabaseTest
    {
        #region Methods

        [Test]
        public void Add_EntityA()
        {
            EntityA entityA = this._Add_EntityA();

            Assert.IsNotNull(entityA);
            Assert.IsNotNull(entityA.Version);
            Assert.IsFalse(entityA.UniqueId == Guid.Empty);
            Assert.AreEqual("Martin", entityA.Name);
        }

        [Test]
        public void Delete_EntityA()
        {
            EntityA entityA = this._Add_EntityA();
            using (IUnitOfWork ctx = UnitOfWorkScope.Start())
            {
                IEntityARepository repo = ServiceLocator.GetInstance<IEntityARepository>();
                IEnumerable<EntityA> results = repo.GetFilteredElements(u => u.UniqueId == entityA.UniqueId);
                Assert.IsTrue(results.Count() > 0);

                EntityA entityA2Delete = results.First();

                repo.Remove(entityA2Delete);

                ctx.Commit();
            }

            using (IUnitOfWork ctx = UnitOfWorkScope.Start())
            {
                IEntityARepository repo = ServiceLocator.GetInstance<IEntityARepository>();
                Assert.AreEqual(0, repo.GetFilteredElements(u => u.UniqueId == entityA.UniqueId).Count());
            }
        }

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            //AggregateCatalog catalog = new AggregateCatalog();
            //AssemblyCatalog thisAssembly = new AssemblyCatalog(System.Reflection.Assembly.GetExecutingAssembly());
            //catalog.Catalogs.Add(thisAssembly);
            //catalog.Catalogs.Add(new DirectoryCatalog(@"C:\Dev\hexa\Hexa.Core\Hexa.Core.Tests\bin\Debug"));

            //CompositionContainer compositionContainer = new CompositionContainer(catalog);

            //Microsoft.Practices.ServiceLocation.ServiceLocator.SetLocatorProvider(() => new Microsoft.Mef.CommonServiceLocator.MefServiceLocator(compositionContainer));

            //IoCContainer containerWrapper = new IoCContainer(
            //    (x, y) => { },
            //    (x, y) => { }
            //);

            //ApplicationContext.Start(containerWrapper, this.ConnectionString());

            ApplicationContext.Start(this.ConnectionString());

            // Validator and TraceManager
            IoCContainer container = ApplicationContext.Container;
            container.RegisterInstance<ILoggerFactory>(new Log4NetLoggerFactory());

            // Context Factory
            NHibernateUnitOfWorkFactory ctxFactory = this.CreateNHContextFactory();

            container.RegisterInstance<IUnitOfWorkFactory>(ctxFactory);
            container.RegisterInstance<IDatabaseManager>(ctxFactory);

            // Repositories
            container.RegisterType<IEntityARepository, EntityARepository>();
            container.RegisterType<IEntityBRepository, EntityBRepository>();

            // Services

            if (!ctxFactory.DatabaseExists())
            {
                ctxFactory.CreateDatabase();
            }

            ctxFactory.ValidateDatabaseSchema();

            ctxFactory.RegisterSessionFactory(container);

            ApplicationContext.User =
                new CorePrincipal(new CoreIdentity("cmendible", "hexa.auth", "cmendible@gmail.com"), new string[] {});
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            try
            {
                var dbManager = ServiceLocator.GetInstance<IDatabaseManager>();
                dbManager.DeleteDatabase();
            }
            finally
            {
                ApplicationContext.Stop();
            }
        }

        [Test]
        public void Query_EntityA()
        {
            EntityA entityA = this._Add_EntityA();

            using (IUnitOfWork ctx = UnitOfWorkScope.Start())
            {
                var repo = ServiceLocator.GetInstance<IEntityARepository>();
                IEnumerable<EntityA> results = repo.GetFilteredElements(u => u.UniqueId == entityA.UniqueId);
                Assert.IsTrue(results.Count() > 0);
            }
        }

        [Test]
        public void Update_EntityA()
        {
            EntityA entityA = this._Add_EntityA();

            Thread.Sleep(1000);

            using (IUnitOfWork ctx = UnitOfWorkScope.Start())
            {
                var repo = ServiceLocator.GetInstance<IEntityARepository>();
                IEnumerable<EntityA> results = repo.GetFilteredElements(u => u.UniqueId == entityA.UniqueId);
                Assert.IsTrue(results.Count() > 0);

                EntityA entityA2Update = results.First();
                entityA2Update.Name = "Maria";
                repo.Modify(entityA2Update);

                ctx.Commit();
            }

            using (IUnitOfWork ctx = UnitOfWorkScope.Start())
            {
                var repo = ServiceLocator.GetInstance<IEntityARepository>();
                entityA = repo.GetFilteredElements(u => u.UniqueId == entityA.UniqueId).Single();
                Assert.AreEqual("Maria", entityA.Name);
                Assert.Greater(entityA.UpdatedAt, entityA.CreatedAt);
            }
        }

        [Test]
        public void Update_EntityA_From_Another_Session()
        {
            EntityA entityA = this._Add_EntityA();

            Thread.Sleep(1000);

            entityA.Name = "Maria";

            using (IUnitOfWork ctx = UnitOfWorkScope.Start())
            {
                var repo = ServiceLocator.GetInstance<IEntityARepository>();
                repo.Modify(entityA);
                ctx.Commit();
            }

            using (IUnitOfWork ctx = UnitOfWorkScope.Start())
            {
                var repo = ServiceLocator.GetInstance<IEntityARepository>();
                entityA = repo.GetFilteredElements(u => u.UniqueId == entityA.UniqueId).Single();
                Assert.AreEqual("Maria", entityA.Name);
                Assert.Greater(entityA.UpdatedAt, entityA.CreatedAt);
            }
        }

        /**
        * These testcase will show, that an assertion failure "...collection xyz
        * was not processed by flush" will be thrown, if you use following
        * entity-constellation and an PostUpdateListener:
        * 
        * You have entities that:
        * 
        * <pre>
        * 1.) two entities are having a m:n relation AND 
        * 2.) we have defined an PostUpdateListener that iterates through all properties of the entity and
        *     so also through the m:n relation (=Collection)
        * </pre>
        * 
        */
        [Test]
        public void Collection_Was_Not_Processed_By_Flush()
        {
            /*
            * create an instance of entity A and an instance of entity B, then link
            * both with each other via an m:n relationship
            */
            EntityA a = this._Create_EntityA_EntityB_And_Many_To_Many_Relation();

            // now update a simple property of EntityA, due to this the
            // MyPostUpdateListener will be called, which iterates through all
            // properties of EntityA (and also the Collection of the m:n relation)
            // --> org.hibernate.AssertionFailure: collection
            // was not processed by flush()
            using (IUnitOfWork ctx = UnitOfWorkScope.Start())
            {
                var repo = ServiceLocator.GetInstance<IEntityARepository>();
                a = repo.GetFilteredElements(u => u.UniqueId == a.UniqueId).Single();

                a.Name = "AA";
                repo.Modify(a);
                ctx.Commit();
            }
        }

        protected abstract string ConnectionString();

        protected abstract NHibernateUnitOfWorkFactory CreateNHContextFactory();

        private EntityA _Add_EntityA()
        {
            var entityA = new EntityA();
            entityA.Name = "Martin";

            using (IUnitOfWork uow = UnitOfWorkScope.Start())
            {
                var repo = ServiceLocator.GetInstance<IEntityARepository>();
                using (IUnitOfWork ctx = UnitOfWorkScope.Start())
                {
                    repo.Add(entityA);
                    ctx.Commit();
                }
                uow.Commit();
            }

            return entityA;
        }

        private EntityA _Create_EntityA_EntityB_And_Many_To_Many_Relation()
        {
            var a = new EntityA();
            a.Name = "A";
            
            var b = new EntityB();
            b.Name = "B";

            a.AddB(b);

            using (IUnitOfWork uow = UnitOfWorkScope.Start())
            {
                var repoA = ServiceLocator.GetInstance<IEntityARepository>();
                var repoB = ServiceLocator.GetInstance<IEntityBRepository>();

                repoB.Add(b);
                repoA.Add(a);
                
                uow.Commit();
            }

            return a;
        }

        #endregion Methods
    }
}