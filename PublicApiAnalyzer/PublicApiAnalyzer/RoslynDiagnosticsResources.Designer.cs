﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PublicApiAnalyzer {
    using System;
    using System.Reflection;
    
    
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
    internal class RoslynDiagnosticsResources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal RoslynDiagnosticsResources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("PublicApiAnalyzer.RoslynDiagnosticsResources", typeof(RoslynDiagnosticsResources).GetTypeInfo().Assembly);
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
        ///   Looks up a localized string similar to Symbol &apos;{0}&apos; violates the backcompat requirement: &apos;Do not add multiple overloads with optional parameters&apos;. See &apos;{1}&apos; for details..
        /// </summary>
        internal static string AvoidMultipleOverloadsWithOptionalParametersMessage {
            get {
                return ResourceManager.GetString("AvoidMultipleOverloadsWithOptionalParametersMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Do not add multiple public overloads with optional parameters.
        /// </summary>
        internal static string AvoidMultipleOverloadsWithOptionalParametersTitle {
            get {
                return ResourceManager.GetString("AvoidMultipleOverloadsWithOptionalParametersTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to All public types and members should be declared in PublicAPI.txt. This draws attention to API changes in the code reviews and source control history, and helps prevent breaking changes..
        /// </summary>
        internal static string DeclarePublicApiDescription {
            get {
                return ResourceManager.GetString("DeclarePublicApiDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Symbol &apos;{0}&apos; is not part of the declared API..
        /// </summary>
        internal static string DeclarePublicApiMessage {
            get {
                return ResourceManager.GetString("DeclarePublicApiMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Add public types and members to the declared API.
        /// </summary>
        internal static string DeclarePublicApiTitle {
            get {
                return ResourceManager.GetString("DeclarePublicApiTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The symbol &apos;{0}&apos; appears more than once in the public API files..
        /// </summary>
        internal static string DuplicateSymbolsInPublicApiFilesMessage {
            get {
                return ResourceManager.GetString("DuplicateSymbolsInPublicApiFilesMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Do not duplicate symbols in public API files.
        /// </summary>
        internal static string DuplicateSymbolsInPublicApiFilesTitle {
            get {
                return ResourceManager.GetString("DuplicateSymbolsInPublicApiFilesTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to When a base class is noninheritable because its constructor is internal, a derived class should not make it inheritable by having a public or protected constructor..
        /// </summary>
        internal static string ExposedNoninstantiableTypeDescription {
            get {
                return ResourceManager.GetString("ExposedNoninstantiableTypeDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Constructor makes its noninheritable base class inheritable, thereby exposing its protected members..
        /// </summary>
        internal static string ExposedNoninstantiableTypeMessage {
            get {
                return ResourceManager.GetString("ExposedNoninstantiableTypeMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Constructor make noninheritable base class inheritable.
        /// </summary>
        internal static string ExposedNoninstantiableTypeTitle {
            get {
                return ResourceManager.GetString("ExposedNoninstantiableTypeTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Symbol &apos;{0}&apos; violates the backcompat requirement: &apos;Public API with optional parameter(s) should have the most parameters amongst its public overloads&apos;. See &apos;{1}&apos; for details..
        /// </summary>
        internal static string OverloadWithOptionalParametersShouldHaveMostParametersMessage {
            get {
                return ResourceManager.GetString("OverloadWithOptionalParametersShouldHaveMostParametersMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Public API with optional parameter(s) should have the most parameters amongst its public overloads..
        /// </summary>
        internal static string OverloadWithOptionalParametersShouldHaveMostParametersTitle {
            get {
                return ResourceManager.GetString("OverloadWithOptionalParametersShouldHaveMostParametersTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The contents of the public API files are invalid: {0}.
        /// </summary>
        internal static string PublicApiFilesInvalidMessage {
            get {
                return ResourceManager.GetString("PublicApiFilesInvalidMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The contents of the public API files are invalid.
        /// </summary>
        internal static string PublicApiFilesInvalidTitle {
            get {
                return ResourceManager.GetString("PublicApiFilesInvalidTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to implicit constructor for {0}.
        /// </summary>
        internal static string PublicImplicitConstructorErrorMessageName {
            get {
                return ResourceManager.GetString("PublicImplicitConstructorErrorMessageName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to When removing a public type or member the corresponding entry in PublicAPI.txt should also be removed. This draws attention to API changes in the code reviews and source control history, and helps prevent breaking changes..
        /// </summary>
        internal static string RemoveDeletedApiDescription {
            get {
                return ResourceManager.GetString("RemoveDeletedApiDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Symbol &apos;{0}&apos; is part of the declared API, but is either not public or could not be found.
        /// </summary>
        internal static string RemoveDeletedApiMessage {
            get {
                return ResourceManager.GetString("RemoveDeletedApiMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Remove deleted types and members from the declared API.
        /// </summary>
        internal static string RemoveDeletedApiTitle {
            get {
                return ResourceManager.GetString("RemoveDeletedApiTitle", resourceCulture);
            }
        }
    }
}
