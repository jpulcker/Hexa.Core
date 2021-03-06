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

namespace Hexa.Core
{
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using System.Security.Principal;
    using System.Threading;
    using System.Web;

    using log4net;

    /// <summary>
    /// Core Context singleton class. Contains a reference to a root CoreContainer object.
    /// </summary>
    public static class ApplicationContext
    {
        #region Fields

        /// <summary>
        /// log4net logger.
        /// </summary>
        private static readonly ILog log = 
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #endregion Fields

        #region Properties

        public static string ConnectionString
        {
            get;
            private set;
        }

        /// <summary>
        /// private ICoreContainer instance.
        /// </summary>
        public static IoCContainer Container
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is initialized.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is initialized; otherwise, <c>false</c>.
        /// </value>
        public static bool IsInitialized
        {
            get;
            private set;
        }

        public static IPrincipal User
        {
            get
            {
                if (HttpContext.Current != null)
                {
                    return HttpContext.Current.User;
                }
                else
                {
                    return Thread.CurrentPrincipal;
                }
            }
            set
            {
                if (HttpContext.Current != null)
                {
                    HttpContext.Current.User = value;
                }

                Thread.CurrentPrincipal = value;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Starts the Context.
        /// </summary>
        public static void Start(string connectionString)
        {
            var dictionaryContainer = new DictionaryServicesContainer();

            Microsoft.Practices.ServiceLocation.ServiceLocator.SetLocatorProvider(() => dictionaryContainer);

            var container = new IoCContainer(
                (x, y) => dictionaryContainer.RegisterType(x, y),
                (x, y) => dictionaryContainer.RegisterInstance(x, y)
            );

            Start(container, connectionString);
        }

        /// <summary>
        /// Starts the Context.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="defaultDataLayer">The default data layer.</param>
        [SuppressMessage("Microsoft.Design",
                         "CA1062:Validate arguments of public methods", MessageId = "0"),
        SuppressMessage("Microsoft.Naming",
                         "CA2204:Literals should be spelled correctly", MessageId = "CoreContext")]
        public static void Start(IoCContainer container, string connectionString)
        {
            if (!IsInitialized)
            {
                log.InfoFormat("Starting CoreContext with container {0}", container.GetType().Name);
                Container = container;

                ConnectionString = connectionString;

                IsInitialized = true;
                log.Info("Core Context Activation Successful");
            }
            else
            {
                throw new InternalException("CoreContext is already running.");
            }
        }

        /// <summary>
        /// Stops this context.
        /// </summary>
        public static void Stop()
        {
            log.Info("Stopping Core Context");
            Container = null;
            IsInitialized = false;
            log.Info("Core Context Deactivation Successful");
        }

        #endregion Methods
    }
}