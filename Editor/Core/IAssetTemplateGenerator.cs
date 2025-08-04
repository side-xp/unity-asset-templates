namespace SideXP.AssetTemplates.EditorOnly
{

    /// <summary>
    /// Qualifies a class as able to generate an asset from a template.
    /// </summary>
    /// <remarks>To make it work as expected, the class must be serializable.</remarks>
    public interface IAssetTemplateGenerator
    {

        /// <summary>
        /// Checks if this template can generate the asset expected with given name, location and other metadata.
        /// </summary>
        /// <param name="info">The information about the asset to create.</param>
        /// <returns>Returns true if this template can generate the asset.</returns>
        bool CanGenerateAsset(AssetInfo info);

        /// <summary>
        /// Generates an asset from this template.<br/>
        /// At this step, the <see cref="CanGenerateAsset(AssetInfo)"/> function is guaranteed to have been called.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="outputInfo"></param>
        /// <returns></returns>
        bool GenerateAsset(AssetInfo info, ref AssetOutputInfo outputInfo);

    }

}
