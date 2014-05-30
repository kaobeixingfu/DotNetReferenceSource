//------------------------------------------------------------------------------
// <copyright file="SystemColors.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
namespace System.Drawing {

    using System.Diagnostics;

    using System;
    

    /// <include file='doc\SystemColors.uex' path='docs/doc[@for="SystemColors"]/*' />
    /// <devdoc>
    ///     Windows system-wide colors.  Whenever possible, try to use
    ///     SystemPens and SystemBrushes rather than SystemColors.
    /// </devdoc>
    public sealed class SystemColors {

        // not creatable...
        //
        private SystemColors() {
        }

        /// <include file='doc\SystemColors.uex' path='docs/doc[@for="SystemColors.ActiveBorder"]/*' />
        /// <devdoc>
        ///     The color of the filled area of an active window border.
        /// </devdoc>
        public static Color ActiveBorder {
            get {
                return new Color(KnownColor.ActiveBorder);
            }
        }

        /// <include file='doc\SystemColors.uex' path='docs/doc[@for="SystemColors.ActiveCaption"]/*' />
        /// <devdoc>
        ///     The color of the background of an active title bar caption.
        /// </devdoc>
        public static Color ActiveCaption {
            get {
                return new Color(KnownColor.ActiveCaption);
            }
        }

        /// <include file='doc\SystemColors.uex' path='docs/doc[@for="SystemColors.ActiveCaptionText"]/*' />
        /// <devdoc>
        ///     The color of the text of an active title bar caption.
        /// </devdoc>
        public static Color ActiveCaptionText {
            get {
                return new Color(KnownColor.ActiveCaptionText);
            }
        }

        /// <include file='doc\SystemColors.uex' path='docs/doc[@for="SystemColors.AppWorkspace"]/*' />
        /// <devdoc>
        ///     The color of the application workspace.  The application workspace
        ///     is the area in a multiple document view that is not being occupied
        ///     by documents.
        /// </devdoc>
        public static Color AppWorkspace {
            get {
                return new Color(KnownColor.AppWorkspace);
            }
        }

        /// <include file='doc\SystemColors.uex' path='docs/doc[@for="SystemColors.ButtonFace"]/*' />
        /// <devdoc>
        ///     Face color for three-dimensional display elements and for dialog box backgrounds.
        /// </devdoc>
        public static Color ButtonFace 
        {
            get 
            {
                return new Color(KnownColor.ButtonFace);
            }
        }

        /// <include file='doc\SystemColors.uex' path='docs/doc[@for="SystemColors.ButtonHighlight"]/*' />
        /// <devdoc>
        ///     Highlight color for three-dimensional display elements (for edges facing the light source.)
        /// </devdoc>
        public static Color ButtonHighlight 
        {
            get 
            {
                return new Color(KnownColor.ButtonHighlight);
            }
        }

        /// <include file='doc\SystemColors.uex' path='docs/doc[@for="SystemColors.ButtonShadow"]/*' />
        /// <devdoc>
        ///     Shadow color for three-dimensional display elements (for edges facing away from the light source.)
        /// </devdoc>
        public static Color ButtonShadow 
        {
            get 
            {
                return new Color(KnownColor.ButtonShadow);
            }
        }

        /// <include file='doc\SystemColors.uex' path='docs/doc[@for="SystemColors.Control"]/*' />
        /// <devdoc>
        ///     The color of the background of push buttons and other 3D objects.
        /// </devdoc>
        public static Color Control {
            [System.Runtime.TargetedPatchingOptOutAttribute("Performance critical to inline across NGen image boundaries")]
            get {
                return new Color(KnownColor.Control);
            }
        }

        /// <include file='doc\SystemColors.uex' path='docs/doc[@for="SystemColors.ControlDark"]/*' />
        /// <devdoc>
        ///     The color of shadows on 3D objects.
        /// </devdoc>
        public static Color ControlDark {
            get {
                return new Color(KnownColor.ControlDark);
            }
        }

        /// <include file='doc\SystemColors.uex' path='docs/doc[@for="SystemColors.ControlDarkDark"]/*' />
        /// <devdoc>
        ///     The color of very dark shadows on 3D objects.
        /// </devdoc>
        public static Color ControlDarkDark {
            get {
                return new Color(KnownColor.ControlDarkDark);
            }
        }

        /// <include file='doc\SystemColors.uex' path='docs/doc[@for="SystemColors.ControlLight"]/*' />
        /// <devdoc>
        ///     The color of highlights on 3D objects.
        /// </devdoc>
        public static Color ControlLight {
            get {
                return new Color(KnownColor.ControlLight);
            }
        }

        /// <include file='doc\SystemColors.uex' path='docs/doc[@for="SystemColors.ControlLightLight"]/*' />
        /// <devdoc>
        ///     The color of very light highlights on 3D objects.
        /// </devdoc>
        public static Color ControlLightLight {
            get {
                return new Color(KnownColor.ControlLightLight);
            }
        }

        /// <include file='doc\SystemColors.uex' path='docs/doc[@for="SystemColors.ControlText"]/*' />
        /// <devdoc>
        ///     The color of the text of push buttons and other 3D objects
        /// </devdoc>
        public static Color ControlText {
            get {
                return new Color(KnownColor.ControlText);
            }
        }

        /// <include file='doc\SystemColors.uex' path='docs/doc[@for="SystemColors.Desktop"]/*' />
        /// <devdoc>
        ///     This color is the user-defined color of the Windows desktop.
        /// </devdoc>
        public static Color Desktop {
            get {
                return new Color(KnownColor.Desktop);
            }
        }

        /// <include file='doc\SystemColors.uex' path='docs/doc[@for="SystemColors.GradientActiveCaption"]/*' />
        /// <devdoc>
        ///     Right side color in the color gradient of an active window's title bar. 
        ///     The ActiveCaption Color specifies the left side color.
        /// </devdoc>
        public static Color GradientActiveCaption 
        {
            get 
            {
                return new Color(KnownColor.GradientActiveCaption);
            }
        }

        /// <include file='doc\SystemColors.uex' path='docs/doc[@for="SystemColors.GradientInactiveCaption"]/*' />
        /// <devdoc>
        ///     Right side color in the color gradient of an inactive window's title bar. 
        ///     The InactiveCaption Color specifies the left side color.
        /// </devdoc>
        public static Color GradientInactiveCaption 
        {
            get 
            {
                return new Color(KnownColor.GradientInactiveCaption);
            }
        }

        /// <include file='doc\SystemColors.uex' path='docs/doc[@for="SystemColors.GrayText"]/*' />
        /// <devdoc>
        ///     The color of text that is being shown in a disabled, or grayed-out
        ///     state.
        /// </devdoc>
        public static Color GrayText {
            get {
                return new Color(KnownColor.GrayText);
            }
        }

        /// <include file='doc\SystemColors.uex' path='docs/doc[@for="SystemColors.Highlight"]/*' />
        /// <devdoc>
        ///     The color of the background of highlighted text.  This includes
        ///     selected menu items as well as selected text.
        /// </devdoc>
        public static Color Highlight {
            get {
                return new Color(KnownColor.Highlight);
            }
        }

        /// <include file='doc\SystemColors.uex' path='docs/doc[@for="SystemColors.HighlightText"]/*' />
        /// <devdoc>
        ///     The color of the text of highlighted text.  This includes
        ///     selected menu items as well as selected text.
        /// </devdoc>
        public static Color HighlightText {
            get {
                return new Color(KnownColor.HighlightText);
            }
        }

        /// <include file='doc\SystemColors.uex' path='docs/doc[@for="SystemColors.HotTrack"]/*' />
        /// <devdoc>
        ///     The hot track color.
        /// </devdoc>
        public static Color HotTrack {
            get {
                return new Color(KnownColor.HotTrack);
            }
        }

        /// <include file='doc\SystemColors.uex' path='docs/doc[@for="SystemColors.InactiveBorder"]/*' />
        /// <devdoc>
        ///     The color of the filled area of an inactive window border.
        /// </devdoc>
        public static Color InactiveBorder {
            get {
                return new Color(KnownColor.InactiveBorder);
            }
        }

        /// <include file='doc\SystemColors.uex' path='docs/doc[@for="SystemColors.InactiveCaption"]/*' />
        /// <devdoc>
        ///     The color of the background of an inactive title bar caption.
        /// </devdoc>
        public static Color InactiveCaption {
            get {
                return new Color(KnownColor.InactiveCaption);
            }
        }

        /// <include file='doc\SystemColors.uex' path='docs/doc[@for="SystemColors.InactiveCaptionText"]/*' />
        /// <devdoc>
        ///     The color of the text of an inactive title bar caption.
        /// </devdoc>
        public static Color InactiveCaptionText {
            get {
                return new Color(KnownColor.InactiveCaptionText);
            }
        }

        /// <include file='doc\SystemColors.uex' path='docs/doc[@for="SystemColors.Info"]/*' />
        /// <devdoc>
        ///     The color of the info/tool tip background.
        /// </devdoc>
        public static Color Info {
            get {
                return new Color(KnownColor.Info);
            }
        }

        /// <include file='doc\SystemColors.uex' path='docs/doc[@for="SystemColors.InfoText"]/*' />
        /// <devdoc>
        ///     The color of the info/tool tip text.
        /// </devdoc>
        public static Color InfoText {
            get {
                return new Color(KnownColor.InfoText);
            }
        }

        /// <include file='doc\SystemColors.uex' path='docs/doc[@for="SystemColors.Menu"]/*' />
        /// <devdoc>
        ///     The color of the background of a menu.
        /// </devdoc>
        public static Color Menu {
            get {
                return new Color(KnownColor.Menu);
            }
        }

        /// <include file='doc\SystemColors.uex' path='docs/doc[@for="SystemColors.MenuBar"]/*' />
        /// <devdoc>
        ///     The color of the background of a menu bar.
        /// </devdoc>
        public static Color MenuBar {
            get {
                return new Color(KnownColor.MenuBar);
            }
        }

        /// <include file='doc\SystemColors.uex' path='docs/doc[@for="SystemColors.MenuHighlight"]/*' />
        /// <devdoc>
        ///     The color used to highlight menu items when the menu appears as a flat menu. 
        ///     The highlighted menu item is outlined with the Highlight Color.
        /// </devdoc>
        public static Color MenuHighlight 
        {
            get 
            {
                return new Color(KnownColor.MenuHighlight);
            }
        }

        /// <include file='doc\SystemColors.uex' path='docs/doc[@for="SystemColors.MenuText"]/*' />
        /// <devdoc>
        ///     The color of the text on a menu.
        /// </devdoc>
        public static Color MenuText {
            get {
                return new Color(KnownColor.MenuText);
            }
        }

        /// <include file='doc\SystemColors.uex' path='docs/doc[@for="SystemColors.ScrollBar"]/*' />
        /// <devdoc>
        ///     The color of the scroll bar area that is not being used by the
        ///     thumb button.
        /// </devdoc>
        public static Color ScrollBar {
            get {
                return new Color(KnownColor.ScrollBar);
            }
        }

        /// <include file='doc\SystemColors.uex' path='docs/doc[@for="SystemColors.Window"]/*' />
        /// <devdoc>
        ///     The color of the client area of a window.
        /// </devdoc>
        public static Color Window {
            get {
                return new Color(KnownColor.Window);
            }
        }

        /// <include file='doc\SystemColors.uex' path='docs/doc[@for="SystemColors.WindowFrame"]/*' />
        /// <devdoc>
        ///     The color of the thin frame drawn around a window.
        /// </devdoc>
        public static Color WindowFrame {
            get {
                return new Color(KnownColor.WindowFrame);
            }
        }

        /// <include file='doc\SystemColors.uex' path='docs/doc[@for="SystemColors.WindowText"]/*' />
        /// <devdoc>
        ///     The color of the text in the client area of a window.
        /// </devdoc>
        public static Color WindowText {
            get {
                return new Color(KnownColor.WindowText);
            }
        }
    }
}
