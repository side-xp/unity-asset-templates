using System;
using System.CodeDom;

using SideXP.Core.EditorOnly;

namespace SideXP.AssetTemplates.EditorOnly
{

    public class ScriptGenerator
    {

        #region Fields

        /// <summary>
        /// The compile unit that represents the script being generated.
        /// </summary>
        private CodeCompileUnit _domCompileUnit = null;

        /// <summary>
        /// The information about the script asset to generate.
        /// </summary>
        private AssetInfo _info = default;

        /// <summary>
        /// The element that represents the main "namespace", used to add the header comments and imports.
        /// </summary>
        private CodeNamespace _importsNamespace = null;

        /// <summary>
        /// The element that represents the actual namespace of the script. If no namespace has been defined in the context, this is
        /// equivalent to <see cref="_importsNamespace"/> 
        /// </summary>
        private CodeNamespace _domNamespace = null;

        /// <summary>
        /// The element that represents the main class of the the script, added to the DOM Namespace.
        /// </summary>
        private CodeTypeDeclaration _mainClass = null;

        #endregion


        #region Lifecycle

        /// <inheritdoc cref="ScriptGenerator"/>
        /// <param name="context"><inheritdoc cref="_info" path="/summary"/></param>
        public ScriptGenerator(AssetInfo info, string summary = null)
        {
            _info = info;
            _domCompileUnit = CodeGenerationUtility.MakeScriptCompileUnit(out _importsNamespace, out _domNamespace);

            // Declare script namespace if applicable
            {
                // Use parent type's namespace if defined
                if (!string.IsNullOrWhiteSpace(info.ParentNamespace))
                    _domNamespace = new CodeNamespace(info.ParentNamespace);
                // Use assembly or global root namespace if defined
                else if (!string.IsNullOrWhiteSpace(info.Namespace))
                    _domNamespace = new CodeNamespace(info.Namespace);

                // Add namespace to DOM
                if (_domNamespace != _importsNamespace)
                    _domCompileUnit.Namespaces.Add(_domNamespace);
            }

            // Declare the main class of the script
            _mainClass = new CodeTypeDeclaration(info.Name);
            _domNamespace.Types.Add(_mainClass);
            _mainClass.Comments.Add(new CodeCommentStatement("<summary>", true));
            _mainClass.Comments.Add(new CodeCommentStatement(!string.IsNullOrWhiteSpace(summary) ? summary : "", true));
            _mainClass.Comments.Add(new CodeCommentStatement("</summary>", true));
        }

        #endregion


        #region Public API

        /// <inheritdoc cref="_info"/>
        public AssetInfo Info => _info;

        /// <inheritdoc cref="_importsNamespace"/>
        public CodeNamespace ImportsNamespace => _importsNamespace;

        /// <inheritdoc cref="_domNamespace"/>
        public CodeNamespace DomNamespace => _domNamespace;

        /// <inheritdoc cref="_mainClass"/>
        public CodeTypeDeclaration MainClass => _mainClass;

        /// <summary>
        /// Appends the base type to the main type from the context, if applicable.
        /// </summary>
        /// <inheritdoc cref="InheritFrom(Type, bool)"/>
        public bool InheritFromContext(bool noOverride = false)
        {
            return _info.ParentType != null && InheritFrom(_info.ParentType, noOverride);
        }

        /// <summary>
        /// Appends a given base type to the main type.
        /// </summary>
        /// <inheritdoc cref="CodeDomUtilities.InheritFrom(CodeTypeDeclaration, Type, CodeNamespace, CodeNamespace, bool)"/>
        public bool InheritFrom(Type baseType, bool noOverride = false)
        {
            return CodeDomUtilities.InheritFrom(_mainClass, baseType, _importsNamespace, _domNamespace, noOverride);
        }

        /// <summary>
        /// Checks the namespace of a given type is imported in the script being generated.
        /// </summary>
        /// <inheritdoc cref="ContainsImport(string)"/>
        public bool ContainsImport(Type type)
        {
            return ContainsImport(type.Namespace);
        }

        /// <summary>
        /// Checks if a given namespace is imported in the script being generated.
        /// </summary>
        /// <param name="namespaceStr">The namespace to check.</param>
        /// <returns>Returns true if the namespace is part of the imports of the script being generated, or if that script is part of
        /// that namespace.</returns>
        public bool ContainsImport(string namespaceStr)
        {
            return CodeDomUtilities.ContainsImport(namespaceStr, _importsNamespace, _domNamespace);
        }

        /// <summary>
        /// Gets a type reference from the script being generated.
        /// </summary>
        /// <inheritdoc cref="CodeDomUtilities.GetTypeReference(Type, CodeNamespace, CodeNamespace, bool, bool)"/>
        public CodeTypeReference GetTypeReference(Type type, bool skipImport = false, bool fullyQualified = false)
        {
            return CodeDomUtilities.GetTypeReference(type, _importsNamespace, _domNamespace, fullyQualified, skipImport);
        }

        /// <typeparam name="T"><inheritdoc cref="GetTypeReference(Type, bool, bool)" path="/param[@name='type']"/></typeparam>
        /// <inheritdoc cref="GetTypeReference(Type, bool, bool)"/>
        public CodeTypeReference GetTypeReference<T>(bool skipImport = false, bool fullyQualified = false)
        {
            return GetTypeReference(typeof(T), skipImport, fullyQualified);
        }

        /// <summary>
        /// Generates the script from this generator's configuration.
        /// </summary>
        /// <returns>Returns the generated script's content.</returns>
        public string Generate()
        {
            return CodeGenerationUtility.GenerateScript(_domCompileUnit);
        }

        #endregion

    }

}
