//------------------------------------------------------------------------------
// <copyright file="ConflictOptions.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data {

    public enum ConflictOption {
        CompareAllSearchableValues = 1,
        CompareRowVersion          = 2,
        OverwriteChanges           = 3,
    }
}

