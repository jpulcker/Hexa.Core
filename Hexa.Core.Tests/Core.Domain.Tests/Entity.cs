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

namespace Hexa.Core.Tests
{
    using Hexa.Core.Domain;
    using NUnit.Framework;

    public class Entity
    {
        #region Properties

        public int Id
        {
            get;
            set;
        }

        public string SampleProperty
        {
            get;
            set;
        }

        #endregion Properties
    }

    public class AuditableEntity : Hexa.Core.Domain.AuditableEntity<AuditableEntity>
    { 
    
    }

    [TestFixture]
    public class EntityTests
    {
        [Test]
        public void New_AuditableEntity_Is_Transient()
        {
            AuditableEntity entity = new AuditableEntity();
            Assert.IsTrue(entity.IsTransient());
        }
    }
}