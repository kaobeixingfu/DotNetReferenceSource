//---------------------------------------------------------------------------
//
// <copyright file="GroupBox.cs" company="Microsoft">
//    Copyright (C) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// Description: Implementation of the GroupBox Control
//
//---------------------------------------------------------------------------

using System.Windows.Input; // Access Key support

namespace System.Windows.Controls
{
    /// <summary>
    /// GroupBox Control class
    /// </summary>
    [Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)] // cannot be read & localized as string
    public class GroupBox : HeaderedContentControl
    {
        #region Constructors
        
        static GroupBox()
        {
            FocusableProperty.OverrideMetadata(typeof(GroupBox), new FrameworkPropertyMetadata(false));
            IsTabStopProperty.OverrideMetadata(typeof(GroupBox), new FrameworkPropertyMetadata(false));
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GroupBox), new FrameworkPropertyMetadata(typeof(GroupBox)));
            EventManager.RegisterClassHandler(typeof(GroupBox), AccessKeyManager.AccessKeyPressedEvent, new AccessKeyPressedEventHandler(OnAccessKeyPressed));
        }

        #endregion

        #region Override methods
        
        /// <summary>
        /// Creates AutomationPeer (<see cref="UIElement.OnCreateAutomationPeer"/>)
        /// </summary>
        protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer() 
        {
            return new System.Windows.Automation.Peers.GroupBoxAutomationPeer(this);
        }

        /// <summary>
        /// The Access key for this control was invoked.
        /// </summary>
        protected override void OnAccessKey(AccessKeyEventArgs e)
        {
            MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
        }

        private static void OnAccessKeyPressed(object sender, AccessKeyPressedEventArgs e)
        {
            if (!e.Handled && e.Scope == null && e.Target == null)
            {
                e.Target = sender as GroupBox;
            }
        }

        #endregion
    }
}
