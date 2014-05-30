// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// <OWNER>[....]</OWNER>
// 

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using Microsoft.Win32.SafeHandles;

namespace System.Security.Policy
{

    /// <summary>
    ///     Factory class which can create evidence on demand for an assembly
    /// </summary>
    internal sealed class AssemblyEvidenceFactory : IRuntimeEvidenceFactory
    {
        private PEFileEvidenceFactory m_peFileFactory;
        private RuntimeAssembly m_targetAssembly;

        /// <summary>
        ///     Create a factory which can generate evidence for the specified assembly
        /// </summary>
        private AssemblyEvidenceFactory(RuntimeAssembly targetAssembly, PEFileEvidenceFactory peFileFactory)
        {
            Contract.Assert(targetAssembly != null);
            Contract.Assert(peFileFactory != null);

            m_targetAssembly = targetAssembly;
            m_peFileFactory = peFileFactory;
        }

        /// <summary>
        ///     PEFile that the assembly is loaded from
        /// </summary>
        internal SafePEFileHandle PEFile
        {
            [SecurityCritical]
            get { return m_peFileFactory.PEFile; }
        }

        /// <summary>
        ///     Assembly that the evidence generated is for
        /// </summary>
        public IEvidenceFactory Target
        {
            get { return m_targetAssembly; }
        }

        /// <summary>
        ///     Generate a specific type of evidence for this assembly
        /// </summary>
        public EvidenceBase GenerateEvidence(Type evidenceType)
        {
            // Assembly evidence is a superset of the evidence that a PEFile can supply, so first see if the
            // requested evidence type can be generated by the assembly's PEFile
            EvidenceBase evidence = m_peFileFactory.GenerateEvidence(evidenceType);
            if (evidence != null)
            {
                return evidence;
            }

            // If the PEFile didn't know about this type of evidence, see if it is an evidence type that the
            // Assembly knows how to generate
            if (evidenceType == typeof(GacInstalled))
            {
                return GenerateGacEvidence();
            }
            else if (evidenceType == typeof(Hash))
            {
                return GenerateHashEvidence();
            }
#pragma warning disable 618 // We need to generate PermissionRequestEvidence in compatibility mode
            else if (evidenceType == typeof(PermissionRequestEvidence))
            {
                return GeneratePermissionRequestEvidence();
            }
#pragma warning restore 618
            else if (evidenceType == typeof(StrongName))
            {
                return GenerateStrongNameEvidence();
            }

            return null;
        }

        /// <summary>
        ///     Generate evidence if the assembly is installed in the GAC
        /// </summary>
        private GacInstalled GenerateGacEvidence()
        {
            if (!m_targetAssembly.GlobalAssemblyCache)
            {
                return null;
            }

            m_peFileFactory.FireEvidenceGeneratedEvent(EvidenceTypeGenerated.Gac);
            return new GacInstalled();
        }

        /// <summary>
        ///     Generate evidence for the assembly's hash value
        /// </summary>
        private Hash GenerateHashEvidence()
        {
            if (m_targetAssembly.IsDynamic)
            {
                return null;
            }

            m_peFileFactory.FireEvidenceGeneratedEvent(EvidenceTypeGenerated.Hash);
            return new Hash(m_targetAssembly);
        }

#pragma warning disable 618 // We need to generate PermissionRequestEvidence in compatibility mode
        /// <summary>
        ///     Generate evidence for the assembly's declarative security
        /// </summary>
        [SecuritySafeCritical]
        private PermissionRequestEvidence GeneratePermissionRequestEvidence()
        {
            Contract.Assert(AppDomain.CurrentDomain.IsLegacyCasPolicyEnabled);

            PermissionSet minimumPermissions = null;
            PermissionSet optionalPermissions = null;
            PermissionSet refusedPermissions = null;

            GetAssemblyPermissionRequests(m_targetAssembly.GetNativeHandle(),
                                          JitHelpers.GetObjectHandleOnStack(ref minimumPermissions),
                                          JitHelpers.GetObjectHandleOnStack(ref optionalPermissions),
                                          JitHelpers.GetObjectHandleOnStack(ref refusedPermissions));

            if (minimumPermissions != null || optionalPermissions != null || refusedPermissions != null)
            {
                return new PermissionRequestEvidence(minimumPermissions,
                                                     optionalPermissions,
                                                     refusedPermissions);
            }

            return null;
        }
#pragma warning restore 618

        /// <summary>
        ///     Generate evidence for this file's strong name
        /// </summary>
        [SecuritySafeCritical]
        private StrongName GenerateStrongNameEvidence()
        {
            byte[] publicKeyBlob = null;
            string simpleName = null;
            ushort majorVersion = 0;
            ushort minorVersion = 0;
            ushort build = 0;
            ushort revision = 0;

            GetStrongNameInformation(m_targetAssembly.GetNativeHandle(),
                                     JitHelpers.GetObjectHandleOnStack(ref publicKeyBlob),
                                     JitHelpers.GetStringHandleOnStack(ref simpleName),
                                     out majorVersion,
                                     out minorVersion,
                                     out build,
                                     out revision);

            if (publicKeyBlob == null || publicKeyBlob.Length == 0)
            {
                return null;
            }

            return new StrongName(new StrongNamePublicKeyBlob(publicKeyBlob),
                                  simpleName,
                                  new Version(majorVersion, minorVersion, build, revision),
                                  m_targetAssembly);
        }

        [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode)]
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        private static extern void GetAssemblyPermissionRequests(RuntimeAssembly assembly,
                                                                 ObjectHandleOnStack retMinimumPermissions,
                                                                 ObjectHandleOnStack retOptionalPermissions,
                                                                 ObjectHandleOnStack retRefusedPermissions);

        /// <summary>
        ///     Get any evidence that was serialized into the assembly
        /// </summary>
        public IEnumerable<EvidenceBase> GetFactorySuppliedEvidence()
        {
            // The PEFile knows how to read the serialized evidence, so we can just delegate to it
            return m_peFileFactory.GetFactorySuppliedEvidence();
        }

        [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode)]
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        private static extern void GetStrongNameInformation(RuntimeAssembly assembly,
                                                            ObjectHandleOnStack retPublicKeyBlob,
                                                            StringHandleOnStack retSimpleName,
                                                            [Out] out ushort majorVersion,
                                                            [Out] out ushort minorVersion,
                                                            [Out] out ushort build,
                                                            [Out] out ushort revision);

        /// <summary>
        ///     Retarget an evidence object from generating evidence for a PEFile to generating evidence for
        ///     the file's assembly.
        /// </summary>
        [SecurityCritical]
        private static Evidence UpgradeSecurityIdentity(Evidence peFileEvidence, RuntimeAssembly targetAssembly)
        {
            Contract.Assert(peFileEvidence != null);
            Contract.Assert(targetAssembly != null);
            Contract.Assert(peFileEvidence.Target is PEFileEvidenceFactory, "Expected upgrade path is from PEFile to Assembly");

            peFileEvidence.Target = new AssemblyEvidenceFactory(targetAssembly,
                                                                peFileEvidence.Target as PEFileEvidenceFactory);

            // Whidbey hosts would provide evidence for assemblies up front rather than on demand.  If there
            // is a HostSecurityManager which does want to provide evidence, then we should provide it the
            // opprotunity to do the same for compatibility.
            HostSecurityManager securityManager = AppDomain.CurrentDomain.HostSecurityManager;
            if ((securityManager.Flags & HostSecurityManagerOptions.HostAssemblyEvidence) == HostSecurityManagerOptions.HostAssemblyEvidence)
            {
                peFileEvidence = securityManager.ProvideAssemblyEvidence(targetAssembly, peFileEvidence);
                if (peFileEvidence == null)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("Policy_NullHostEvidence", securityManager.GetType().FullName, targetAssembly.FullName));
                }
            }

            return peFileEvidence;
        }
    }
}
