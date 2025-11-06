using Tempus.Core.Models;

namespace Tempus.Core.Interfaces;

/// <summary>
/// Service interface for managing help topics and content
/// </summary>
public interface IHelpService
{
    /// <summary>
    /// Gets all available help topics
    /// </summary>
    /// <returns>List of all help topics</returns>
    Task<List<HelpTopic>> GetAllTopicsAsync();

    /// <summary>
    /// Gets a specific help topic by ID
    /// </summary>
    /// <param name="topicId">The help topic ID</param>
    /// <returns>The help topic, or null if not found</returns>
    Task<HelpTopic?> GetTopicByIdAsync(string topicId);

    /// <summary>
    /// Gets frequently accessed help topics
    /// </summary>
    /// <param name="count">Number of topics to return</param>
    /// <returns>List of frequently accessed topics</returns>
    Task<List<HelpTopic>> GetFrequentTopicsAsync(int count = 5);

    /// <summary>
    /// Searches help topics by query string
    /// </summary>
    /// <param name="query">Search query</param>
    /// <returns>List of matching help topics</returns>
    Task<List<HelpTopic>> SearchTopicsAsync(string query);

    /// <summary>
    /// Gets related topics for a given topic
    /// </summary>
    /// <param name="topicId">The help topic ID</param>
    /// <returns>List of related help topics</returns>
    Task<List<HelpTopic>> GetRelatedTopicsAsync(string topicId);

    /// <summary>
    /// Gets topics by category
    /// </summary>
    /// <param name="category">The category name</param>
    /// <returns>List of topics in the category</returns>
    Task<List<HelpTopic>> GetTopicsByCategoryAsync(string category);
}
