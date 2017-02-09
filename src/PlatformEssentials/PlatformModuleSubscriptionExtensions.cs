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
using System.ServiceModel;

namespace VP.FF.PT.Common.PlatformEssentials
{
    /// <summary>
    /// Extends the platform module to deal with generic service interactions such as subscriptions.
    /// </summary>
    public static class PlatformModuleSubscriptionExtensions
    {
        /// <summary>
        /// Subscribes to an <see cref="IPlatformModuleServiceEvents"/> of the module.
        /// </summary>
        /// <typeparam name="TFault">The respective fault to generate, depending on the module.</typeparam>
        /// <param name="module">The module to subscribe to.</param>
        /// <param name="generateFault">A function to generate the fault of type TFault.</param>
        public static void SubscribeEvents<TFault>(this PlatformModule module, Func<TFault> generateFault)
        {
            try
            {
                var subscriber = OperationContext.Current.GetCallbackChannel<IPlatformModuleServiceEvents>();
                module.ModuleStateChangedCallback += subscriber.ModuleStateChanged;
                module.MetricsChangedCallback += subscriber.MetricsChanged;
                module.SendUserNotificationCallback += subscriber.SendUserNotification;
            }
            catch (Exception e)
            {
                module.Logger.Error(e.Message, e);
                throw new FaultException<TFault>(generateFault(), e.Message);
            }

            module.RaiseModuleStateChangedEvent();
        }

        /// <summary>
        /// Subscribes to a custom event of the module.
        /// </summary>
        /// <typeparam name="TFault">The respective fault to generate, depending on the module.</typeparam>
        /// <typeparam name="TCallbackChannel">The type of the callback channel which will be passed into action <see cref="subscribe"/>.</typeparam>
        /// <param name="module">The module to subscribe to.</param>
        /// <param name="generateFault">A function to generate the fault of type TFault.</param>
        /// <param name="subscribe">A custom action to subscribe to the WCF event. It provides the WCF subscriber.</param>
        public static void SubscribeEvents<TFault, TCallbackChannel>(this PlatformModule module, Func<TFault> generateFault, Action<TCallbackChannel> subscribe)
        {
            try
            {
                var subscriber = OperationContext.Current.GetCallbackChannel<TCallbackChannel>();
                subscribe(subscriber);
            }
            catch (Exception e)
            {
                module.Logger.Error(e.Message, e);
                throw new FaultException<TFault>(generateFault(), e.Message);
            }
        }

        /// <summary>
        /// Unsubscribes from an <see cref="IPlatformModuleServiceEvents"/> of the module.
        /// </summary>
        /// <typeparam name="TFault">The respective fault to generate, depending on the module.</typeparam>
        /// <param name="module">The module to unsubscribe from.</param>
        /// <param name="generateFault">A function to generate the fault of type TFault.</param>
        public static void UnsubscribeEvents<TFault>(this PlatformModule module, Func<TFault> generateFault)
        {
            module.UnsubscribeEvents<TFault, IPlatformModuleServiceEvents>(generateFault,
                subscriber => module.ModuleStateChangedCallback -= subscriber.ModuleStateChanged);

            module.UnsubscribeEvents<TFault, IPlatformModuleServiceEvents>(generateFault,
                subscriber => module.MetricsChangedCallback -= subscriber.MetricsChanged);

            module.UnsubscribeEvents<TFault, IPlatformModuleServiceEvents>(generateFault,
                subscriber => module.SendUserNotificationCallback -= subscriber.SendUserNotification);
        }

        /// <summary>
        /// Unsubscribes from a custom event of the module.
        /// </summary>
        /// <typeparam name="TFault">The respective fault to generate, depending on the module.</typeparam>
        /// <typeparam name="TCallbackChannel">The type of the callback channel which will be passed into action <see cref="unsubscribe"/>.</typeparam>
        /// <param name="module">The module to unsubscribe from.</param>
        /// <param name="generateFault">A function to generate the fault of type TFault.</param>
        /// <param name="unsubscribe">A custom action to unsubscribe from the WCF event. It provides the WCF subscriber.</param>
        public static void UnsubscribeEvents<TFault, TCallbackChannel>(this PlatformModule module, Func<TFault> generateFault, Action<TCallbackChannel> unsubscribe)
        {
            try
            {
                var subscriber = OperationContext.Current.GetCallbackChannel<TCallbackChannel>();
                unsubscribe(subscriber);
            }
            catch (Exception e)
            {
                module.Logger.Error(e.Message, e);
                throw new FaultException<TFault>(generateFault(), e.Message);
            }
        }
    }
}
