//
// Copyright (c) 2012 Citrix Systems, Inc.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

using System;
using System.ServiceModel;

namespace Udbus.WCF.Service.Host
{
    /// <summary>
    /// WCF Service creation parameters coupled with a function to make a WCF service.
    /// </summary>
    /// <typeparam name="TService">Type of WCF Service to create.</typeparam>
    /// <typeparam name="TParams">Parmaters to creation function type.</typeparam>
    public struct WCFServiceCreationData<TService, TParams>
    {
        /// <summary>
        /// Function to create ServiceHost.
        /// </summary>
        public readonly Func<TParams, TService> func;
        /// <summary>
        /// Parameters to ServiceHost creation function.
        /// </summary>
        public readonly DbusServiceCreationParams CreationParams;

        #region Constructors
        public WCFServiceCreationData(Func<TParams, TService> func, DbusServiceCreationParams CreationParams)
        {
            this.func = func;
            this.CreationParams = CreationParams;
        }
        #endregion // Constructors
    } // Ends struct DbusServiceCreationData

    /// <summary>
    /// Implements IInstanceProvider to pass through instance creation to generic delegate.
    /// </summary>
    /// <typeparam name="TService">Type of WCF Service instance to create.</typeparam>
    /// <typeparam name="TParams">Parameters to creation delegate.</typeparam>
    public class DbusServiceInstanceProvider<TService, TParams> : System.ServiceModel.Dispatcher.IInstanceProvider
    {
        #region Fields
        /// <summary>
        /// Parameters to creation delegate.
        /// </summary>
        protected readonly TParams tparams;
        /// <summary>
        /// Creation delegate.
        /// </summary>
        protected readonly Func<TParams, TService> func;
        #endregion // Fields

        #region Constructors
        /// <summary>
        /// Creates instance provider which passes through to delegate call providing supplied parameters.
        /// </summary>
        /// <param name="tparams">Parameters to delegate.</param>
        /// <param name="func">Delegate to call to create instance.</param>
        public DbusServiceInstanceProvider(TParams tparams, Func<TParams, TService> func)
        {
            this.tparams = tparams;
            this.func = func;
        }
        #endregion Constructors

        #region IInstanceProvider Members

        /// <summary>
        /// Returns a WCF Service object given the specified InstanceContext object.
        /// </summary>
        /// <param name="instanceContext">The current InstanceContext object.</param>
        /// <param name="message">The message that triggered the creation of a service object.</param>
        /// <returns>The service object.</returns>
        public object GetInstance(System.ServiceModel.InstanceContext instanceContext, System.ServiceModel.Channels.Message message)
        {
            return this.GetInstance(instanceContext);
        }

        /// <summary>
        /// Returns a WCF Service object given the specified InstanceContext object,
        /// created by passing through to the underlying delegate.
        /// </summary>
        /// <param name="instanceContext">The current InstanceContext object.</param>
        /// <returns>A user-defined service object.</returns>
        public object GetInstance(System.ServiceModel.InstanceContext instanceContext)
        {
            TService service = this.func(this.tparams);
            return service;
        }

        /// <summary>
        /// Called when an InstanceContext object recycles a service object.
        /// </summary>
        /// <param name="instanceContext">The service's instance context.</param>
        /// <param name="instance">The service object to be recycled.</param>
        public void ReleaseInstance(System.ServiceModel.InstanceContext instanceContext, object instance)
        {
            // No action.
        }

        #endregion // IInstanceProvider Members
    } // Ends DbusServiceInstanceProvider

    /// <summary>
    /// ServiceHost which adds IInstanceProvider to contract behaviors.
    /// </summary>
    public partial class DbusServiceHost : System.ServiceModel.ServiceHost
    {
        #region Creation with a parameter
        // Helper functions to reduce code bloat.

        /// <summary>
        /// Create a ServiceHost instance.
        /// </summary>
        /// <typeparam name="TService">Type of WCF Service to create.</typeparam>
        /// <typeparam name="TParams">Parameters to ServiceHost creation function type.</typeparam>
        /// <param name="tparams">ServiceHost creation function parameters.</param>
        /// <param name="func">Function creating an instance of the ServiceHost type.</param>
        /// <param name="serviceType">Type of WCF Service to be hosted.</param>
        /// <param name="baseAddresses">Base addresses for WCF ServiceHost endpoints.</param>
        /// <returns>Newly created DbusServiceHost.</returns>
        static public DbusServiceHost Create<TService, TParams>(TParams tparams,
            Func<TParams, TService> func,
            Type serviceType, params Uri[] baseAddresses)
        {
            return new DbusServiceHost(
                new DbusServiceInstanceProvider<TService, TParams>(tparams, func),
                serviceType,
                baseAddresses
            );
        }

        /// <summary>
        /// Create a ServiceHost instance, deducing WCF Service type from parameters.
        /// </summary>
        /// <typeparam name="TService">Type of WCF Service to create.</typeparam>
        /// <typeparam name="TParams">Parameters to ServiceHost creation function type.</typeparam>
        /// <param name="tparams">ServiceHost creation function parameters.</param>
        /// <param name="func">Function creating an instance of the ServiceHost type.</param>
        /// <param name="baseAddresses">Base addresses for WCF ServiceHost endpoints.</param>
        /// <returns>Newly created DbusServiceHost.</returns>
        static public DbusServiceHost Create<TService, TParams>(TParams tparams
            , Func<TParams, TService> func
            , params Uri[] baseAddresses)
        {
            return Create(tparams,
                func,
                typeof(TService),
                baseAddresses
            );
        }

        /// <summary>
        /// Create a ServiceHost with accompanying creation data.
        /// </summary>
        /// <typeparam name="TService">Type of WCF Service to create.</typeparam>
        /// <typeparam name="TParams">Parameters to ServiceHost creation function type.</typeparam>
        /// <param name="tparams">ServiceHost creation function parameters.</param>
        /// <param name="func">Function creating an instance of the ServiceHost type with accompanying creation data.</param>
        /// <param name="serviceType">Type of WCF Service to be hosted.</param>
        /// <param name="baseAddresses">Base addresses for WCF ServiceHost endpoints.</param>
        /// <returns>Created ServiceHost with accompanying creation data.</returns>
        static public ServiceHostCreationData CreateWithData<TService, TParams>(TParams tparams,
            Func<WCFServiceCreationData<TService, TParams>> func,
            Type serviceType, params Uri[] baseAddresses)
        {
            // Get the host creation function and creation data.
            WCFServiceCreationData<TService, TParams> hostdata = func();
            // Instantiate the host.
            DbusServiceHost host = DbusServiceHost.Create(tparams, hostdata.func, serviceType, baseAddresses);
            // Package up the data.
            ServiceHostCreationData hostcreation = new ServiceHostCreationData(host, hostdata.CreationParams);
            return hostcreation;
        }

        /// <summary>
        /// Create a ServiceHost with accompanying creation data deducing the WCF Service from the type parameters.
        /// </summary>
        /// <typeparam name="TService">Type of WCF Service to create.</typeparam>
        /// <typeparam name="TParams">Parameters to ServiceHost creation function type.</typeparam>
        /// <param name="tparams">ServiceHost creation function parameters.</param>
        /// <param name="func">Function creating an instance of the ServiceHost type with accompanying creation data.</param>
        /// <param name="baseAddresses">Base addresses for WCF ServiceHost endpoints.</param>
        /// <returns>Created ServiceHost with accompanying creation data.</returns>
        static public ServiceHostCreationData CreateWithData<TService, TParams>(TParams tparams,
            Func<WCFServiceCreationData<TService, TParams>> func,
            params Uri[] baseAddresses)
        {
            return CreateWithData(tparams, func, typeof(TService), baseAddresses);
        }

        #endregion // Creation with a parameter

    } // Ends DbusServiceHost

} // Ends Udbus.WCF.Service.Host
