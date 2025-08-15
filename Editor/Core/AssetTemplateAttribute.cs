using System;

namespace SideXP.AssetTemplates.EditorOnly
{

    /// <summary>
    /// Links additional informations to an <see cref="IAssetTemplate"/> class for users.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class AssetTemplateAttribute : Attribute
    {

        /// <summary>
        /// The name of this template, as displayed in the Templates editor window.
        /// </summary>
        public string Name { get; set; } = null;

        /// <summary>
        /// The description of this template, as displayed in the Templates editor window.
        /// </summary>
        /// <remarks>>Describe here what this template will do when triggered.</remarks>
        public string Description { get; set; } = null;

        /// <summary>
        /// The list of name patterns or special locations that will trigger this template.
        /// </summary>
        public string[] Triggers { get; set; } = null;

        /// <summary>
        /// By default, asset templates are all enabled. If this option is checked, this template will be disabled by default.
        /// </summary>
        /// <remarks>This is mostly meant to reusability, to make sure that this template won't be conflicting when copied to another
        /// project if it alters the normal behavior of Unity.</remarks>
        public bool DisabledByDefault { get; set; } = false;

        /// <summary>
        /// The order in which this template should be processed. The lower the value, the first.
        /// </summary>
        public int Order { get; set; } = 0;

        /// <inheritdoc cref="AssetTemplateAttribute(string, string, string[])"/>
        public AssetTemplateAttribute(string name)
            : this(name, null) { }

        /// <inheritdoc cref="AssetTemplateAttribute"/>
        /// <param name="name"><inheritdoc cref="Name" path="/summary"/></param>
        /// <param name="description"><inheritdoc cref="Description" path="/summary"/></param>
        /// <param name="triggers"><inheritdoc cref="Triggers" path="/summary"/></param>
        public AssetTemplateAttribute(string name, string description, params string[] triggers)
        {
            Name = name;
            Description = description;
            Triggers = triggers;
        }

    }

}
