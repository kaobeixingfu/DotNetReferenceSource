//---------------------------------------------------------------------------
//
// <copyright file="UserInitiatedRoutedEventPermission.cs" company="Microsoft">
//    Copyright (C) Microsoft Corporation.  All rights reserved.
// </copyright>
//
//---------------------------------------------------------------------------

using System;
using System.Security;
using System.Security.Permissions;
using System.Windows;
using MS.Internal.Permissions;

namespace MS.Internal.Permissions
{
    // This permission was moved into WindowsBase from PresentationCore since its corresponding
    // Attribute class must be defined in a seperate assembly from where it is used (PresentationCore).
    // The reason for this is explained in the following connect article.  The MSDN documentation has
    // yet to be updated:
    // https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=297627
    [Serializable]
    internal class UserInitiatedRoutedEventPermission : InternalParameterlessPermissionBase
    {
        public UserInitiatedRoutedEventPermission() : this(PermissionState.Unrestricted)
        {
        }

        public UserInitiatedRoutedEventPermission(PermissionState state): base(state)
        {
        }

        public override IPermission Copy()
        {
            // copy is easy there is no state !
            return new UserInitiatedRoutedEventPermission();
        }
    }

}
