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

namespace Hexa.Core.Logging
{
    using System;
    using System.ComponentModel.Composition;
    using System.IO;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Web;

    using log4net;
    using log4net.Config;

    [Export(typeof(ILoggerFactory))]
    public class Log4NetLoggerFactory : ILoggerFactory
    {
        #region Fields

        private static bool initialized;

        #endregion Fields

        #region Constructors

        public Log4NetLoggerFactory()
            : this(null)
        {
        }

        public Log4NetLoggerFactory(FileInfo configFile)
        {
            if (!initialized)
            {
                if (configFile != null)
                {
                    XmlConfigurator.ConfigureAndWatch(configFile);
                }
                else
                {
                    XmlConfigurator.Configure();
                }

                // Register log4net context loggers..
                if (_isWebContext())
                {
                    GlobalContext.Properties["UserHostAddress"] = new UserHostAddressLogContext();
                    GlobalContext.Properties["User"] = new UserLogContext();
                    GlobalContext.Properties["SessionId"] = new UserSessionIdLogContext();
                }

                initialized = true;
            }
        }

        #endregion Constructors

        #region Methods

        public ILogger Create(Type type)
        {
            return new Log4NetLogger(type);
        }

        public ILogger Create(string typeName)
        {
            return new Log4NetLogger(typeName);
        }

        private static bool _isWebContext()
        {
            if (HttpContext.Current != null)
            {
                return true;
            }

            if (OperationContext.Current != null)
            {
                return true;
            }

            return false;
        }

        #endregion Methods

        #region Nested Types

        private class UserHostAddressLogContext
        {
            #region Methods

            public override string ToString()
            {
                try
                {
                    if (HttpContext.Current != null && HttpContext.Current.Request != null)
                    {
                        return HttpContext.Current.Request.UserHostAddress;
                    }

                    OperationContext context = OperationContext.Current;
                    if (context != null && context.IncomingMessageProperties != null &&
                        context.IncomingMessageProperties.ContainsKey(RemoteEndpointMessageProperty.Name))
                    {
                        var endpointProperty =
                            context.IncomingMessageProperties[RemoteEndpointMessageProperty.Name] as
                            RemoteEndpointMessageProperty;
                        return endpointProperty.Address;
                    }

                    return null;
                }
                catch
                {
                    return null;
                }
            }

            #endregion Methods
        }

        private class UserLogContext
        {
            #region Methods

            public override string ToString()
            {
                try
                {
                    if (HttpContext.Current == null)
                    {
                        return null;
                    }

                    if (HttpContext.Current.User == null)
                    {
                        return null;
                    }

                    if (HttpContext.Current.User.Identity == null)
                    {
                        return null;
                    }

                    return HttpContext.Current.User.Identity.Name;
                }
                catch
                {
                    return null;
                }
            }

            #endregion Methods
        }

        private class UserSessionIdLogContext
        {
            #region Methods

            public override string ToString()
            {
                try
                {
                    if (HttpContext.Current != null && HttpContext.Current.Request != null &&
                        HttpContext.Current.Session != null)
                    {
                        return HttpContext.Current.Session.SessionID;
                    }

                    return null;
                }
                catch
                {
                    return null;
                }
            }

            #endregion Methods
        }

        #endregion Nested Types
    }
}