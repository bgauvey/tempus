namespace Tempus.Core.Models;

/// <summary>
/// Represents a help topic with content and related topics
/// </summary>
public class HelpTopic
{
    /// <summary>
    /// Unique identifier for the help topic
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Display title of the help topic
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Category of the help topic (e.g., "Getting Started", "Calendar", "Events")
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// HTML content of the help topic
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Tags for search and categorization
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// IDs of related help topics
    /// </summary>
    public List<string> RelatedTopicIds { get; set; } = new();

    /// <summary>
    /// Indicates if this is a frequently accessed topic
    /// </summary>
    public bool IsFrequentlyAccessed { get; set; } = false;

    /// <summary>
    /// Display order for frequently accessed topics
    /// </summary>
    public int DisplayOrder { get; set; } = 0;
}
