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

namespace Hexa.Core.Domain
{
    using System.Collections.Generic;
    using System.Runtime.Remoting.Messaging;
    using System.ServiceModel;
    using System.Web;

    public class UnitOfWorkScope
    {
        #region Fields

        private static string _key = "Hexa.Core.Domain.RunningContexts.Key";

        #endregion Fields

        #region Properties

        public static IUnitOfWork Current
        {
            get
            {
                if (RunningScopes.Count > 0)
                {
                    IUnitOfWork unitOfWork = RunningScopes.Peek();
                    return unitOfWork;
                }
                else
                {
                    return null;
                }
            }
            private set
            {
                if (value == null)
                {
                    if (RunningScopes.Count > 0)
                    {
                        RunningScopes.Pop();
                    }
                }
                else
                {
                    RunningScopes.Push(value);
                }
            }
        }

        public static Stack<IUnitOfWork> RunningScopes
        {
            get
            {
                //Get object depending on  execution environment ( WCF without HttpContext,HttpContext or CallContext)
                if (OperationContext.Current != null)
                {
                    //WCF without HttpContext environment
                    var containerExtension = OperationContext.Current.Extensions.Find<ContainerExtension>();

                    if (containerExtension == null)
                    {
                        containerExtension = new ContainerExtension
                        {
                            Value = new Stack<IUnitOfWork>()
                        };

                        OperationContext.Current.Extensions.Add(containerExtension);
                    }

                    return containerExtension.Value as Stack<IUnitOfWork>;
                }
                else if (HttpContext.Current != null)
                {
                    //HttpContext avaiable ( ASP.NET ..)
                    if (HttpContext.Current.Items[_key] == null)
                    {
                        HttpContext.Current.Items[_key] = new Stack<IUnitOfWork>();
                    }

                    return HttpContext.Current.Items[_key] as Stack<IUnitOfWork>;
                }
                else
                {
                    if (CallContext.GetData(_key) == null)
                    {
                        CallContext.SetData(_key, new Stack<IUnitOfWork>());
                    }

                    //Not in WCF or ASP.NET Environment, UnitTesting, WinForms, WPF etc.
                    return CallContext.GetData(_key) as Stack<IUnitOfWork>;
                }
            }
        }

        #endregion Properties

        #region Methods

        public static void DisposeCurrent()
        {
            Current = null;
        }

        public static IUnitOfWork Start()
        {
            Current = ServiceLocator.GetInstance<IUnitOfWorkFactory>().Create();
            return Current;
        }

        #endregion Methods

        #region Nested Types

        /// <summary>
        /// Custom extension for OperationContext scope
        /// </summary>
        private class ContainerExtension : IExtension<OperationContext>
        {
            #region Properties

            public object Value
            {
                get;
                set;
            }

            #endregion Properties

            #region Methods

            public void Attach(OperationContext owner)
            {
            }

            public void Detach(OperationContext owner)
            {
            }

            #endregion Methods
        }

        #endregion Nested Types
    }
}