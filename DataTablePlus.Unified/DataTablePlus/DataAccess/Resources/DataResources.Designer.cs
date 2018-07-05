﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DataTablePlus.DataAccess.Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class DataResources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal DataResources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("DataTablePlus.DataAccess.Resources.DataResources", typeof(DataResources).Assembly);
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
        ///   Looks up a localized string similar to ALTER TABLE {0} ADD [{1}] INT NULL.
        /// </summary>
        internal static string AddTrackerColumnStatement {
            get {
                return ResourceManager.GetString("AddTrackerColumnStatement", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to CREATE NONCLUSTERED INDEX IX_TrackerColumn   
        ///    ON {0} ([{1}]).
        /// </summary>
        internal static string CreateNonClustedIndex {
            get {
                return ResourceManager.GetString("CreateNonClustedIndex", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to IF EXISTS (SELECT name FROM sys.indexes WHERE name = N&apos;IX_TrackerColumn&apos;)   
        ///    DROP INDEX IX_TrackerColumn ON {0}.
        /// </summary>
        internal static string DropNonClustedIndex {
            get {
                return ResourceManager.GetString("DropNonClustedIndex", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ALTER TABLE {0} DROP COLUMN [{1}].
        /// </summary>
        internal static string DropTrackerColumnStatement {
            get {
                return ResourceManager.GetString("DropTrackerColumnStatement", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT TOP 0 * FROM {0}.
        /// </summary>
        internal static string GetSchemaTable {
            get {
                return ResourceManager.GetString("GetSchemaTable", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to @MaxParam.
        /// </summary>
        internal static string MaxParameterName {
            get {
                return ResourceManager.GetString("MaxParameterName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to @MinParam.
        /// </summary>
        internal static string MinParameterName {
            get {
                return ResourceManager.GetString("MinParameterName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot create more than one transaction..
        /// </summary>
        internal static string MoreThanOneTransaction {
            get {
                return ResourceManager.GetString("MoreThanOneTransaction", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT {0} FROM {1} WHERE [{2}] &gt;= {3} AND [{2}] &lt;= {4} ORDER BY [{2}].
        /// </summary>
        internal static string SelectPrimaryKeysStatement {
            get {
                return ResourceManager.GetString("SelectPrimaryKeysStatement", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to TrackerColumn.
        /// </summary>
        internal static string TrackerColumnName {
            get {
                return ResourceManager.GetString("TrackerColumnName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot commit because the transaction is null..
        /// </summary>
        internal static string TransactionIsNull {
            get {
                return ResourceManager.GetString("TransactionIsNull", resourceCulture);
            }
        }
    }
}
