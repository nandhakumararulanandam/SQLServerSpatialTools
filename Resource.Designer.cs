﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.1433
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SQLSpatialTools
{
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "2.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resource {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resource() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Microsoft.SqlServer.SpatialToolbox.Resource", typeof(Resource).Assembly);
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
        ///   Looks up a localized string similar to fi1 and fi2 must be different.
        /// </summary>
        internal static string Fi1AndFi2MustBeDifferent {
            get {
                return ResourceManager.GetString("Fi1AndFi2MustBeDifferent", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Input coordinate is NaN..
        /// </summary>
        internal static string InputCoordinateIsNaN {
            get {
                return ResourceManager.GetString("InputCoordinateIsNaN", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Input latitude {0} is out of range [-{1}, {1}]..
        /// </summary>
        internal static string InputLatitudeIsOutOfRange {
            get {
                return ResourceManager.GetString("InputLatitudeIsOutOfRange", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Input longitude {0} is out of range [-{1}, {1}]..
        /// </summary>
        internal static string InputLongitudeIsOutOfRange {
            get {
                return ResourceManager.GetString("InputLongitudeIsOutOfRange", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Output latitude {0} is out of range [-Pi/2, Pi/2]..
        /// </summary>
        internal static string OutputLatitudeIsOutOfRange {
            get {
                return ResourceManager.GetString("OutputLatitudeIsOutOfRange", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Output longitude {0} is out of range [-Pi, Pi)..
        /// </summary>
        internal static string OutputLongitudeIsOutOfRange {
            get {
                return ResourceManager.GetString("OutputLongitudeIsOutOfRange", resourceCulture);
            }
        }
    }
}
