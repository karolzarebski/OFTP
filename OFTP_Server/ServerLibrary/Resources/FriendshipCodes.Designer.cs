﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ServerLibrary.Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class FriendshipCodes {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal FriendshipCodes() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("ServerLibrary.Resources.FriendshipCodes", typeof(FriendshipCodes).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 401.
        /// </summary>
        internal static string AddFriend {
            get {
                return ResourceManager.GetString("AddFriend", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 403.
        /// </summary>
        internal static string AddToFriendsAccepted {
            get {
                return ResourceManager.GetString("AddToFriendsAccepted", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 404.
        /// </summary>
        internal static string AddToFriendsRejected {
            get {
                return ResourceManager.GetString("AddToFriendsRejected", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 400.
        /// </summary>
        internal static string AskForFriendship {
            get {
                return ResourceManager.GetString("AskForFriendship", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 405.
        /// </summary>
        internal static string NewFriend {
            get {
                return ResourceManager.GetString("NewFriend", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 402.
        /// </summary>
        internal static string RemoveFriend {
            get {
                return ResourceManager.GetString("RemoveFriend", resourceCulture);
            }
        }
    }
}
