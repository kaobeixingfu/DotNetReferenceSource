//------------------------------------------------------------------------------
// <copyright file="NotifyParentPropertyAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */

namespace System.ComponentModel {
    
    using System;
    using System.Security.Permissions;

    /// <devdoc>
    ///    <para>
    ///       Indicates whether the parent property is notified
    ///       if a child namespace property is modified.
    ///    </para>
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class NotifyParentPropertyAttribute : Attribute {

        /// <devdoc>
        ///    <para>
        ///       Specifies that the parent property should be notified on changes to the child class property. This field is read-only.
        ///    </para>
        /// </devdoc>
        public static readonly NotifyParentPropertyAttribute Yes = new NotifyParentPropertyAttribute(true);

        /// <devdoc>
        ///    <para>Specifies that the parent property should not be notified of changes to the child class property. This field is read-only.</para>
        /// </devdoc>
        public static readonly NotifyParentPropertyAttribute No = new NotifyParentPropertyAttribute(false);

        /// <devdoc>
        ///    <para>Specifies the default attribute state, that the parent property should not be notified of changes to the child class property.
        ///       This field is read-only.</para>
        /// </devdoc>
        public static readonly NotifyParentPropertyAttribute Default = No;

        private bool notifyParent = false;


        /// <devdoc>
        /// <para>Initiailzes a new instance of the NotifyPropertyAttribute class 
        ///    that uses the specified value
        ///    to indicate whether the parent property should be notified when a child namespace property is modified.</para>
        /// </devdoc>
        public NotifyParentPropertyAttribute(bool notifyParent) {
            this.notifyParent = notifyParent;
        }


        /// <devdoc>
        ///    <para>
        ///       Gets or sets whether the parent property should be notified
        ///       on changes to a child namespace property.
        ///    </para>
        /// </devdoc>
        public bool NotifyParent {
            get {
                return notifyParent;
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Tests whether the specified object is the same as the current object.
        ///    </para>
        /// </devdoc>
        public override bool Equals(object obj) {
            if (obj == this) {
                return true;
            }
            if ((obj != null) && (obj is NotifyParentPropertyAttribute)) {
                return ((NotifyParentPropertyAttribute)obj).NotifyParent == notifyParent;
            }

            return false;
        }

        /// <devdoc>
        ///    <para>
        ///       Returns the hashcode for this object.
        ///    </para>
        /// </devdoc>
        public override int GetHashCode() {
            return base.GetHashCode();
        }

        /// <devdoc>
        ///    <para>
        ///       Gets whether this attribute is <see langword='true'/> by default.
        ///    </para>
        /// </devdoc>
        public override bool IsDefaultAttribute() {
            return this.Equals(Default);
        }
    }
}
