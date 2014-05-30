//---------------------------------------------------------------------------
//
// Copyright (C) Microsoft Corporation.  All rights reserved.
//
//---------------------------------------------------------------------------


using System.Windows.Automation.Provider;


namespace System.Windows.Controls
{
    /// <summary>
    ///     Control that implements a selectable item inside a ListView.
    /// </summary>
#if OLD_AUTOMATION
    [Automation(AccessibilityControlType = "ListItem")]
#endif
    public class ListViewItem : ListBoxItem
    {
        // NOTE: ListViewItem has no default theme style. It uses ThemeStyleKey 
        // to find default style for different view.

        // helper to set DefaultStyleKey of ListViewItem
        internal void SetDefaultStyleKey(object key)
        {
            DefaultStyleKey = key;
        }

        //  helper to clear DefaultStyleKey of ListViewItem
        internal void ClearDefaultStyleKey()
        {
            ClearValue(DefaultStyleKeyProperty);
        }
    }
}
