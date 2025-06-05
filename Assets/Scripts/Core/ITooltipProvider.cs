using UnityEngine;
/// <summary>
/// Anything that wants to show a tooltip on hover implements this.
/// </summary>
public interface ITooltipProvider
{
    /// <summary>
    /// Returns **which** prefab should be instantiated for this tooltip. 
    /// E.g. some objects might want a “TraitTooltipPrefab”, others might want a “SimpleHelpTooltipPrefab”.
    /// </summary>
    GameObject GetTooltipPrefab();

    /// <summary>
    /// Once TooltipOnHover instantiates the prefab, it will call this method 
    /// so you can fill in all of the UI fields (text, image, stat bars, etc.).
    /// </summary>
    /// <param name="tooltipInstance">
    ///     The freshly instantiated tooltip GameObject. You can do
    ///     tooltipInstance.GetComponent<…>() or FindObjectOfType<…>() on it
    ///     to grab your own UI‐scripts and write in the data.
    /// </param>
    void PopulateTooltip(GameObject tooltipInstance);
}
