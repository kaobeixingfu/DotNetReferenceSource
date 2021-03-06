//---------------------------------------------------------------------------
//
// <copyright file="SecurityCriticalDataForMultipleGetAndSet.cs" company="Microsoft">
//    Copyright (C) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// Description:
//              This is a helper class to facilate the storage of data that's Critical for set and get.
//              This file is used as a cannister to hold values for Dynamic properties that are
//              not safe to expose and are built to be used only privately.
//              The other caveat is that these dynamic properties should not be needed in the
//              animation or databinding scenarios example: PresenationSource                
//
// History:
//  04/29/05 : akaza Created. 
//
//---------------------------------------------------------------------------
using System ; 
using System.Security ; 

using MS.Internal.PresentationCore;

namespace MS.Internal 
{
    [FriendAccessAllowed] // Built into Core, also used by Framework.
    internal class SecurityCriticalDataForMultipleGetAndSet<T>
    {
        /// <SecurityNote>
        ///    Critical - "by definition" - this class is intended only for data that's
        ///               Critical for setting.
        /// </SecurityNote>
        [SecurityCritical]
        internal SecurityCriticalDataForMultipleGetAndSet(T value)
        { 
            _value = value; 
        }

        /// <SecurityNote>
        ///    Critical - Setter is Critical "by definition" - this class is intended only
        ///               for data that's Critical for setting.
        /// </SecurityNote>
        internal T Value 
        {
            [SecurityCritical]
            get
            {
                return _value;
            }

            [SecurityCritical]
            set
            {
                _value = value;
            }
        }

        /// <SecurityNote>
        /// Critical - by definition as this data is Critical for set.
        /// </SecurityNote>>
        [SecurityCritical]
        private T _value;
    }
}
