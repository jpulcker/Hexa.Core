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

#if !MONO

namespace Hexa.Core.Tests.Raven
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

    using Validation;

    [TestFixture]
    public class RavenTests
    {
        #region Methods

        [Test]
        public void Add_EntityA()
        {
            EntityA entityA = this._Add_EntityA();

            Assert.IsNotNull(entityA);
            //Assert.IsNotNull(entityA.Version);
            Assert.IsFalse(entityA.UniqueId == Guid.Empty);
            Assert.AreEqual("Martin", entityA.Name);

            //return entityA.UniqueId;
        }

        [Test]
        public void Delete_EntityA()
        {
            EntityA entityA = this._Add_EntityA();

            using (IUnitOfWork ctx = UnitOfWorkScope.Start())
            {
                var repo = ServiceLocator.GetInstance<IEntityARepository>();
                IEnumerable<EntityA> results = repo.GetFilteredElements(u => u.UniqueId == entityA.UniqueId);
                Assert.IsTrue(results.Count() > 0);

                EntityA entityA2Delete = results.First();

                repo.Remove(entityA2Delete);

                ctx.Commit();
            }

            using (IUnitOfWork ctx = UnitOfWorkScope.Start())
            {
                var repo = ServiceLocator.GetInstance<IEntityARepository>();
                Assert.AreEqual(0, repo.GetFilteredElements(u => u.UniqueId == entityA.UniqueId).Count());
            }
        }

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            ApplicationContext.Start("Data");

            // Validator and TraceManager
            IoCContainer container = ApplicationContext.Container;
            container.RegisterInstance<ILoggerFactory>(new Log4NetLoggerFactory());

            // Context Factory
            var ctxFactory = new RavenUnitOfWorkFactory();

            container.RegisterInstance<IUnitOfWorkFactory>(ctxFactory);
            container.RegisterInstance<IDatabaseManager>(ctxFactory);

            // Repositories
            container.RegisterType<IEntityARepository, EntityARepository>();

            // Services

            if (!ctxFactory.DatabaseExists())
            {
                ctxFactory.CreateDatabase();
            }

            ctxFactory.ValidateDatabaseSchema();

            ctxFactory.RegisterSessionFactory(container);
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            var dbManager = ServiceLocator.GetInstance<IDatabaseManager>();
            dbManager.DeleteDatabase();

            ApplicationContext.Stop();
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

            using (IUnitOfWork ctx = UnitOfWorkScope.Start())
            {
                var repo = ServiceLocator.GetInstance<IEntityARepository>();
                IEnumerable<EntityA> results = repo.GetFilteredElements(u => u.UniqueId == entityA.UniqueId);
                Assert.IsTrue(results.Count() > 0);

                EntityA entityA2Update = results.First();
                entityA2Update.Name = "Maria";
                repo.Modify(entityA2Update);

                Thread.Sleep(1000);

                ctx.Commit();
            }

            using (IUnitOfWork ctx = UnitOfWorkScope.Start())
            {
                var repo = ServiceLocator.GetInstance<IEntityARepository>();
                EntityA entityA2 = repo.GetFilteredElements(u => u.UniqueId == entityA.UniqueId).Single();
                Assert.AreEqual("Maria", entityA2.Name);
                //Assert.Greater(entityA2.UpdatedAt, entityA2.CreatedAt);
            }
        }

        private EntityA _Add_EntityA()
        {
            var entityA = new EntityA();
            entityA.Name = "Martin";

            using (IUnitOfWork ctx = UnitOfWorkScope.Start())
            {
                var repo = ServiceLocator.GetInstance<IEntityARepository>();
                repo.Add(entityA);
                ctx.Commit();
            }

            return entityA;
        }

        #endregion Methods
    }
}

#endif