//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------

namespace System.ServiceModel.WasHosting
{
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Activation;

    [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUninstantiatedInternalClasses,
            Justification = "Instantiated by ASP.NET")]
    class MsmqAppDomainProtocolHandler : BaseAppDomainProtocolHandler
    {
        public MsmqAppDomainProtocolHandler()
            : base(MsmqUri.NetMsmqAddressTranslator.Scheme)
        { }

        protected override void OnStart()
        {
            MsmqHostedTransportConfiguration configuration = HostedTransportConfigurationManager.GetConfiguration(MsmqUri.NetMsmqAddressTranslator.Scheme) as MsmqHostedTransportConfiguration;
            configuration.TransportManager.Start(OnMessageReceived);
        }
    }
}

