//------------------------------------------------------------------------------
// <copyright file="IFilter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
// <owner current="false" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    using System;
    using System.Data;
    using System.Diagnostics;

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    internal interface IFilter {
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        bool Invoke(DataRow row, DataRowVersion version);
    }
}
