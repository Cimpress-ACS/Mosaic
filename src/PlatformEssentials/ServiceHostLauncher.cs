/* Copyright 2017 Cimpress

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License. */


using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using VP.FF.PT.Common.Infrastructure.Logging;
using VP.FF.PT.Common.Infrastructure.Wcf;

namespace VP.FF.PT.Common.PlatformEssentials
{
    /// <summary>
    /// Starts local hosted WCF services.
    /// The services are configured in App.config.
    /// </summary>
    public class ServiceHostLauncher
    {
        private readonly List<ServiceHost> _serviceHosts = new List<ServiceHost>();

        /// <summary>
        /// Runs the specified logger.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="container">The container is needed to get the service implementation for the specified service contracts.</param>
        /// <param name="generateMexEndpoint">if set to <c>true</c> it generates metadata endpoints. The address gets logged and can be used with copy and paste to generate proxy code in VS (with the Add Service Reference feature).</param>
        public void Run(ILogger logger, CompositionContainer container, bool generateMexEndpoint = false)
        {
            foreach (var serviceConfig in WcfServiceConfiguration.GetServiceConfigList())
            {
                if (serviceConfig.ContractType == null)
                {
                    logger.Warn("Can't find assembly for module " + serviceConfig.Item.ContractTypeAssembly + ". Initialization will be skipped...");
                    continue;
                }

                var serviceHostInstance = container.GetExports(serviceConfig.ContractType, null, null).FirstOrDefault();

                if (serviceHostInstance == null)
                {
                    logger.Warn("Can't find MEF export for contract " + serviceConfig.Item.ContractTypeFullname + ". Initialization will be skipped...");
                    continue;
                }

                try
                {
                    ServiceHost host = CreateServiceHost(serviceConfig, serviceHostInstance.Value);
                    if (host != null)
                    {
                        host.Open();
                        _serviceHosts.Add(host);
                        logger.InfoFormat("Opened service host {0} with address {1}", host.Description.Name, host.BaseAddresses.First().AbsoluteUri);
                    }
                }
                catch (Exception e)
                {
                    logger.Error("Cannot create WCF ServiceHost for " + serviceConfig.Item.EndpointName, e);
                }

                if (generateMexEndpoint)
                {
                    try
                    {
                        ServiceHost hostWithMetadata = CreateMetadataServiceHost(serviceConfig, serviceHostInstance.Value);
                        if (hostWithMetadata != null)
                        {
                            hostWithMetadata.Open();
                            _serviceHosts.Add(hostWithMetadata);
                            logger.InfoFormat("Opened service host {0} with MEX metadata endpoint with address \n--> {1}/mex. It can be used in VS->Add Service Reference to generate proxy code.",
                                hostWithMetadata.Description.Name, hostWithMetadata.BaseAddresses.First().AbsoluteUri);
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Warn("Cannot create Metadata WCF ServiceHost for " + serviceConfig.Item.EndpointName, e);
                    }
                }
            }
        }

        public void Stop(ILogger logger)
        {
            foreach (var host in _serviceHosts)
            {
                if (host != null)
                {
                    try
                    {
                        host.Close();
                    }
                    catch (Exception e)
                    {
                        e.AddWindowsEventLog("WcfServiceHost");

                        logger.Error("Cannot stop WCF ServiceHost", e);
                    }
                }
            }
        }

        public static ServiceHost CreateServiceHost(ServiceConfig item, object serviceHostInstance)
        {
            string addressPort = "localhost:" + item.Port;

            //check configuration, no launch if no config
            if (string.IsNullOrEmpty(item.Port) || string.IsNullOrEmpty(item.Item.EndpointName))
                return null;

            Type contractType = Type.GetType(item.Item.ContractTypeDeclaration);

            var baseAddress = new Uri(string.Format("net.tcp://{0}/", addressPort));
            var fullAddress = new Uri(string.Format("net.tcp://{0}/{1}", addressPort, item.Item.EndpointName));
            var host = new ServiceHost(serviceHostInstance, baseAddress);

            var binding = TcpBindingUtility.CreateNetTcpBinding();
            //var binding = HttpBindingUtility.CreateBasicHttpBinding();
            //var binding = HttpBindingUtility.CreateWsDualHttpBinding();

            host.AddServiceEndpoint(contractType, binding, fullAddress);

            //this is the default but good to know if we want to change it later
            //host.Authorization.PrincipalPermissionMode = PrincipalPermissionMode.UseWindowsGroups;

            host.Authorization.PrincipalPermissionMode = PrincipalPermissionMode.None;

            host.Description.Behaviors.Add(new ServiceCallTimerBehavior());

            var debugBehavior = host.Description.Behaviors.OfType<ServiceDebugBehavior>().FirstOrDefault();
            if (debugBehavior == null)
                host.Description.Behaviors.Add(new ServiceDebugBehavior {IncludeExceptionDetailInFaults = true});
            else
                debugBehavior.IncludeExceptionDetailInFaults = true;

            return host;
        }

        /// <summary>
        /// Creates a http endpoint to provide MEX metadata needed for VS proxy code generation. It uses the port of the service incremented by 1.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="serviceHostInstance"></param>
        public static ServiceHost CreateMetadataServiceHost(ServiceConfig item, object serviceHostInstance)
        {
            string addressPort = "localhost:" + (int.Parse(item.Port) + 1);

            if (string.IsNullOrEmpty(item.Port) || string.IsNullOrEmpty(item.Item.EndpointName))
                return null;

            Type contractType = Type.GetType(item.Item.ContractTypeDeclaration);

            var fullAddress = new Uri(string.Format("http://{0}/{1}", addressPort, item.Item.EndpointName));
            var host = new ServiceHost(serviceHostInstance, fullAddress);

            var binding = HttpBindingUtility.CreateWsDualHttpBinding();

            host.Description.Behaviors.Add(new ServiceMetadataBehavior { HttpGetEnabled = true });
            host.AddServiceEndpoint(contractType, binding, string.Empty);
            host.AddServiceEndpoint(ServiceMetadataBehavior.MexContractName, MetadataExchangeBindings.CreateMexHttpBinding(), "mex");

            return host;
        }
    }
}
