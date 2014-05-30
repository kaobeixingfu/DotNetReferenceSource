//---------------------------------------------------------------------------
//
// File: CursorInteropHelper.cs
//
// Description: Implements Avalon CursorInteropHelper class, which helps
//              interop b/w Cursor handles and Avalon Cursor objects.
//
// Copyright (C) 2005 by Microsoft Corporation.  All rights reserved.
//
// History:
// 06/30/05     jdmack      Created
//---------------------------------------------------------------------------

using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Input;
using SecurityHelper=MS.Internal.SecurityHelper; 

namespace System.Windows.Interop
{
    #region class CursorInteropHelper
    /// <summary>
    ///     Implements Avalon CursorInteropHelper classes, which helps
    ///     interop b/w legacy Cursor handles and Avalon Cursor objects.
    /// </summary>
    public static class CursorInteropHelper
    {
        //---------------------------------------------------
        //
        // Public Methods
        //
        //---------------------------------------------------
        #region Public Methods

        /// <summary>
        ///     Creates a Cursor from a SafeHandle to a native Win32 Cursor
        /// </summary>
        /// <param name="cursorHandle">
        ///     SafeHandle to a native Win32 cursor
        /// </param>
        ///<remarks>
        ///     Callers must have UIPermission(UIPermissionWindow.AllWindows) to call this API.
        ///</remarks>
        /// <SecurityNote>
        ///    Critical: This causes the cursor to change and accesses the SetHandleInternalMethod
        ///    PublicOK: There is a demand.
        /// </SecurityNote>
        [SecurityCritical ]
        public static Cursor Create(SafeHandle cursorHandle)
        {
            SecurityHelper.DemandUIWindowPermission();

            return CriticalCreate(cursorHandle);
        }

        #endregion Public Methods

        //---------------------------------------------------
        //
        // Internal Methods
        //
        //---------------------------------------------------
        #region Internal Methods

        /// <summary>
        ///     Creates a Cursor from a SafeHandle to a native Win32 Cursor
        /// </summary>
        /// <param name="cursorHandle">
        ///     SafeHandle to a native Win32 cursor
        /// </param>
        /// <SecurityNote>
        ///    Critical: This causes the cursor to change and accesses the SetHandleInternalMethod
        /// </SecurityNote>
        [SecurityCritical]
        internal static Cursor CriticalCreate(SafeHandle cursorHandle)
        {
            return new Cursor(cursorHandle);
        }

        #endregion Internal Methods
    }
    #endregion class CursorInteropHelper
}

