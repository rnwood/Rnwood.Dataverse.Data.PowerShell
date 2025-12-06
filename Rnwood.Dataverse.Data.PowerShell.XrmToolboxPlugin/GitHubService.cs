using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Octokit;

namespace Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin
{
    /// <summary>
    /// Service for interacting with GitHub API using Octokit
    /// </summary>
    public class GitHubService : IDisposable
    {
        private GitHubClient _client;
        private string _currentUsername;
        private readonly string _repositoryOwner;
        private readonly string _repositoryName;
        private const string GraphQLEndpoint = "https://api.github.com/graphql";
        private const string DefaultCategoryName = "Show and tell";
        private const string FallbackCategoryName = "General";
        private const string TagPrefix = "script-category-";
        private const string ClientId = "Ov23liZ8M5xdHlQQgGbT"; // GitHub OAuth App for XrmToolbox PowerShell Plugin
        private string _currentDeviceCode;
        private int _deviceCodeExpiresIn;
        private int _deviceCodeInterval;

        public bool IsAuthenticated => _client != null && !string.IsNullOrEmpty(_currentUsername);
        public string CurrentUsername => _currentUsername;

        public GitHubService(string repositoryOwner = "rnwood", string repositoryName = "Rnwood.Dataverse.Data.PowerShell")
        {
            _repositoryOwner = repositoryOwner;
            _repositoryName = repositoryName;
            // Create an unauthenticated client initially
            _client = new GitHubClient(new ProductHeaderValue("XrmToolbox-PowerShell-Plugin"));
        }

        /// <summary>
        /// Initiates GitHub Device Flow authentication
        /// </summary>
        public async Task<(bool Success, string Message, string UserCode, string VerificationUri)> InitiateDeviceFlowAsync()
        {
            try
            {
                var request = new OauthDeviceFlowRequest(ClientId);
                var response = await _client.Oauth.InitiateDeviceFlow(request);
                
                _currentDeviceCode = response.DeviceCode;
                _deviceCodeExpiresIn = response.ExpiresIn;
                _deviceCodeInterval = response.Interval;
                
                return (true, "Device flow initiated", response.UserCode, response.VerificationUri);
            }
            catch (Exception ex)
            {
                return (false, $"Failed to initiate device flow: {ex.Message}", null, null);
            }
        }

        /// <summary>
        /// Polls for device flow authentication completion
        /// </summary>
        public async Task<(bool Success, string Message, bool Pending)> PollDeviceFlowAsync()
        {
            if (string.IsNullOrEmpty(_currentDeviceCode))
            {
                return (false, "No device flow in progress", false);
            }

            try
            {
                var request = new OauthTokenRequest(ClientId, "", _currentDeviceCode);
                var token = await _client.Oauth.CreateAccessToken(request);
                
                // Create authenticated client
                _client = new GitHubClient(new ProductHeaderValue("XrmToolbox-PowerShell-Plugin"))
                {
                    Credentials = new Credentials(token.AccessToken)
                };

                // Get current user info
                var user = await _client.User.Current();
                _currentUsername = user.Login;
                
                _currentDeviceCode = null;

                return (true, $"Successfully authenticated as {_currentUsername}", false);
            }
            catch (Octokit.AuthorizationException)
            {
                // Still waiting for user to authorize
                return (false, "Authorization pending", true);
            }
            catch (Exception ex)
            {
                _currentDeviceCode = null;
                return (false, $"Authentication failed: {ex.Message}", false);
            }
        }

        /// <summary>
        /// Authenticates using a Personal Access Token (fallback method)
        /// </summary>
        public async Task<(bool Success, string Message)> AuthenticateWithTokenAsync(string token)
        {
            try
            {
                // Create authenticated client
                _client = new GitHubClient(new ProductHeaderValue("XrmToolbox-PowerShell-Plugin"))
                {
                    Credentials = new Credentials(token)
                };

                // Get current user info to verify token
                var user = await _client.User.Current();
                _currentUsername = user.Login;

                return (true, $"Successfully authenticated as {_currentUsername}");
            }
            catch (Exception ex)
            {
                return (false, $"Authentication failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Logs out by clearing credentials
        /// </summary>
        public void Logout()
        {
            _currentUsername = null;
            _client = new GitHubClient(new ProductHeaderValue("XrmToolbox-PowerShell-Plugin"));
        }

        /// <summary>
        /// Gets all discussions from the repository filtered by "Show and tell" category
        /// </summary>
        public async Task<List<ScriptGalleryItem>> GetDiscussionsAsync(string searchText = null, List<string> filterTags = null, bool onlyMySubmissions = false)
        {
            try
            {
                // Use GraphQL to get discussions since REST API doesn't support it
                var query = @"
                    query($owner: String!, $name: String!, $first: Int!) {
                        repository(owner: $owner, name: $name) {
                            discussions(first: $first, orderBy: {field: CREATED_AT, direction: DESC}) {
                                nodes {
                                    id
                                    number
                                    title
                                    body
                                    author {
                                        login
                                    }
                                    createdAt
                                    updatedAt
                                    upvoteCount
                                    closed
                                    comments(first: 1) {
                                        totalCount
                                    }
                                    category {
                                        name
                                    }
                                    labels(first: 20) {
                                        nodes {
                                            name
                                        }
                                    }
                                    reactions(first: 100) {
                                        totalCount
                                        nodes {
                                            content
                                        }
                                    }
                                }
                            }
                        }
                    }";

                var variables = new
                {
                    owner = _repositoryOwner,
                    name = _repositoryName,
                    first = 100
                };

                var result = await _client.Connection.Post<dynamic>(
                    new Uri(GraphQLEndpoint),
                    new { query, variables },
                    "application/json",
                    "application/json");

                var discussions = new List<ScriptGalleryItem>();
                
                foreach (var node in result.Body.data.repository.discussions.nodes)
                {
                    var categoryName = node.category?.name?.ToString() ?? "General";
                    
                    // Filter by "Show and tell" category
                    if (categoryName != DefaultCategoryName && categoryName != FallbackCategoryName)
                    {
                        continue;
                    }
                    
                    // Always hide closed discussions
                    bool isClosed = node.closed != null && (bool)node.closed;
                    if (isClosed)
                    {
                        continue;
                    }
                    
                    // Extract tags from labels
                    var tags = new List<string>();
                    if (node.labels?.nodes != null)
                    {
                        foreach (var label in node.labels.nodes)
                        {
                            string labelName = label.name?.ToString() ?? "";
                            if (labelName.StartsWith(TagPrefix))
                            {
                                // Remove prefix and add to tags
                                tags.Add(labelName.Substring(TagPrefix.Length));
                            }
                        }
                    }
                    
                    // Count reactions
                    int thumbsUpCount = 0;
                    int thumbsDownCount = 0;
                    int totalReactionCount = 0;
                    
                    if (node.reactions?.nodes != null)
                    {
                        totalReactionCount = node.reactions.totalCount;
                        foreach (var reaction in node.reactions.nodes)
                        {
                            string content = reaction.content?.ToString() ?? "";
                            if (content == "THUMBS_UP")
                            {
                                thumbsUpCount++;
                            }
                            else if (content == "THUMBS_DOWN")
                            {
                                thumbsDownCount++;
                            }
                        }
                    }
                    
                    var author = node.author?.login?.ToString() ?? "Unknown";
                    
                    // Filter by author if onlyMySubmissions is true
                    if (onlyMySubmissions && IsAuthenticated && author != CurrentUsername)
                    {
                        continue;
                    }
                    
                    var item = new ScriptGalleryItem
                    {
                        Id = node.id,
                        Number = node.number,
                        Title = node.title,
                        Body = node.body,
                        Author = author,
                        CreatedAt = DateTime.Parse(node.createdAt.ToString()),
                        UpdatedAt = DateTime.Parse(node.updatedAt.ToString()),
                        UpvoteCount = node.upvoteCount,
                        CommentCount = node.comments.totalCount,
                        Category = categoryName,
                        Tags = tags,
                        IsClosed = isClosed,
                        ThumbsUpCount = thumbsUpCount,
                        ThumbsDownCount = thumbsDownCount,
                        TotalReactionCount = totalReactionCount
                    };
                    
                    // Apply search filter
                    if (!string.IsNullOrWhiteSpace(searchText))
                    {
                        var searchLower = searchText.ToLower();
                        if (!item.Title.ToLower().Contains(searchLower) && 
                            !item.Body.ToLower().Contains(searchLower) &&
                            !item.Author.ToLower().Contains(searchLower))
                        {
                            continue;
                        }
                    }
                    
                    // Apply tag filter
                    if (filterTags != null && filterTags.Count > 0)
                    {
                        bool hasMatchingTag = false;
                        foreach (var tag in filterTags)
                        {
                            if (item.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase))
                            {
                                hasMatchingTag = true;
                                break;
                            }
                        }
                        if (!hasMatchingTag)
                        {
                            continue;
                        }
                    }
                    
                    discussions.Add(item);
                }

                // Sort by thumbs up count (descending), then by comment count (descending)
                discussions = discussions
                    .OrderByDescending(d => d.ThumbsUpCount)
                    .ThenByDescending(d => d.CommentCount)
                    .ToList();

                return discussions;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get discussions: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets a specific discussion by number
        /// </summary>
        public async Task<ScriptGalleryItem> GetDiscussionAsync(int number)
        {
            try
            {
                var query = @"
                    query($owner: String!, $name: String!, $number: Int!) {
                        repository(owner: $owner, name: $name) {
                            discussion(number: $number) {
                                id
                                number
                                title
                                body
                                author {
                                    login
                                }
                                createdAt
                                updatedAt
                                upvoteCount
                                comments(first: 50) {
                                    totalCount
                                    nodes {
                                        id
                                        body
                                        author {
                                            login
                                        }
                                        createdAt
                                    }
                                }
                                category {
                                    name
                                }
                                labels(first: 20) {
                                    nodes {
                                        name
                                    }
                                }
                            }
                        }
                    }";

                var variables = new
                {
                    owner = _repositoryOwner,
                    name = _repositoryName,
                    number
                };

                var result = await _client.Connection.Post<dynamic>(
                    new Uri(GraphQLEndpoint),
                    new { query, variables },
                    "application/json",
                    "application/json");

                var discussion = result.Body.data.repository.discussion;
                
                // Extract tags from labels
                var tags = new List<string>();
                if (discussion.labels?.nodes != null)
                {
                    foreach (var label in discussion.labels.nodes)
                    {
                        string labelName = label.name?.ToString() ?? "";
                        if (labelName.StartsWith(TagPrefix))
                        {
                            tags.Add(labelName.Substring(TagPrefix.Length));
                        }
                    }
                }
                
                var item = new ScriptGalleryItem
                {
                    Id = discussion.id,
                    Number = discussion.number,
                    Title = discussion.title,
                    Body = discussion.body,
                    Author = discussion.author?.login ?? "Unknown",
                    CreatedAt = DateTime.Parse(discussion.createdAt.ToString()),
                    UpdatedAt = DateTime.Parse(discussion.updatedAt.ToString()),
                    UpvoteCount = discussion.upvoteCount,
                    CommentCount = discussion.comments.totalCount,
                    Category = discussion.category?.name ?? "General",
                    Tags = tags,
                    Comments = new List<DiscussionComment>()
                };

                foreach (var comment in discussion.comments.nodes)
                {
                    item.Comments.Add(new DiscussionComment
                    {
                        Id = comment.id,
                        Body = comment.body,
                        Author = comment.author?.login ?? "Unknown",
                        CreatedAt = DateTime.Parse(comment.createdAt.ToString())
                    });
                }

                return item;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get discussion: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Creates a new discussion with a PowerShell script and tags
        /// </summary>
        public async Task<ScriptGalleryItem> CreateDiscussionAsync(string title, string scriptContent, List<string> tags = null, string categoryId = null)
        {
            if (!IsAuthenticated)
            {
                throw new InvalidOperationException("You must be authenticated to create discussions");
            }

            try
            {
                // First get the repository ID and category ID
                var repoQuery = @"
                    query($owner: String!, $name: String!) {
                        repository(owner: $owner, name: $name) {
                            id
                            discussionCategories(first: 10) {
                                nodes {
                                    id
                                    name
                                }
                            }
                        }
                    }";

                var repoResult = await _client.Connection.Post<dynamic>(
                    new Uri(GraphQLEndpoint),
                    new { query = repoQuery, variables = new { owner = _repositoryOwner, name = _repositoryName } },
                    "application/json",
                    "application/json");

                string repositoryId = repoResult.Body.data.repository.id;
                
                // Use provided category or find "Show and tell" category
                if (string.IsNullOrEmpty(categoryId))
                {
                    foreach (var cat in repoResult.Body.data.repository.discussionCategories.nodes)
                    {
                        if (cat.name == DefaultCategoryName || cat.name == FallbackCategoryName)
                        {
                            categoryId = cat.id;
                            break;
                        }
                    }
                    
                    if (string.IsNullOrEmpty(categoryId) && repoResult.Body.data.repository.discussionCategories.nodes.Count > 0)
                    {
                        categoryId = repoResult.Body.data.repository.discussionCategories.nodes[0].id;
                    }
                }

                // Format the body with the script in a code block
                string body = $"```powershell\n{scriptContent}\n```";

                // Create the discussion
                var mutation = @"
                    mutation($repositoryId: ID!, $categoryId: ID!, $title: String!, $body: String!) {
                        createDiscussion(input: {repositoryId: $repositoryId, categoryId: $categoryId, title: $title, body: $body}) {
                            discussion {
                                id
                                number
                                title
                                body
                                author {
                                    login
                                }
                                createdAt
                                updatedAt
                                upvoteCount
                            }
                        }
                    }";

                var createResult = await _client.Connection.Post<dynamic>(
                    new Uri(GraphQLEndpoint),
                    new { query = mutation, variables = new { repositoryId, categoryId, title, body } },
                    "application/json",
                    "application/json");

                var discussion = createResult.Body.data.createDiscussion.discussion;
                
                var newItem = new ScriptGalleryItem
                {
                    Id = discussion.id,
                    Number = discussion.number,
                    Title = discussion.title,
                    Body = discussion.body,
                    Author = discussion.author?.login ?? "Unknown",
                    CreatedAt = DateTime.Parse(discussion.createdAt.ToString()),
                    UpdatedAt = DateTime.Parse(discussion.updatedAt.ToString()),
                    UpvoteCount = discussion.upvoteCount,
                    CommentCount = 0,
                    Category = DefaultCategoryName,
                    Tags = tags ?? new List<string>()
                };
                
                // Add labels/tags if provided
                if (tags != null && tags.Count > 0)
                {
                    try
                    {
                        await AddLabelsToDiscussionAsync(discussion.id, tags);
                    }
                    catch (Exception ex)
                    {
                        // Log but don't fail - discussion was created
                        System.Diagnostics.Debug.WriteLine($"Failed to add labels: {ex.Message}");
                    }
                }

                return newItem;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create discussion: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Adds labels to a discussion (for tags)
        /// </summary>
        private async Task AddLabelsToDiscussionAsync(string discussionId, List<string> tags)
        {
            if (!IsAuthenticated)
            {
                throw new InvalidOperationException("You must be authenticated to add labels");
            }

            try
            {
                // Get repository ID and existing labels
                var repoQuery = @"
                    query($owner: String!, $name: String!) {
                        repository(owner: $owner, name: $name) {
                            id
                            labels(first: 100) {
                                nodes {
                                    id
                                    name
                                }
                            }
                        }
                    }";

                var repoResult = await _client.Connection.Post<dynamic>(
                    new Uri(GraphQLEndpoint),
                    new { query = repoQuery, variables = new { owner = _repositoryOwner, name = _repositoryName } },
                    "application/json",
                    "application/json");

                var repositoryId = repoResult.Body.data.repository.id;
                var existingLabels = new Dictionary<string, string>();
                
                foreach (var label in repoResult.Body.data.repository.labels.nodes)
                {
                    existingLabels[label.name.ToString()] = label.id.ToString();
                }

                // For each tag, ensure label exists and add to discussion
                foreach (var tag in tags)
                {
                    var labelName = TagPrefix + tag;
                    string labelId;

                    if (!existingLabels.ContainsKey(labelName))
                    {
                        // Create label
                        var createLabelMutation = @"
                            mutation($repositoryId: ID!, $name: String!, $color: String!) {
                                createLabel(input: {repositoryId: $repositoryId, name: $name, color: $color}) {
                                    label {
                                        id
                                        name
                                    }
                                }
                            }";

                        var createLabelResult = await _client.Connection.Post<dynamic>(
                            new Uri(GraphQLEndpoint),
                            new { query = createLabelMutation, variables = new { repositoryId, name = labelName, color = "0366d6" } },
                            "application/json",
                            "application/json");

                        labelId = createLabelResult.Body.data.createLabel.label.id;
                    }
                    else
                    {
                        labelId = existingLabels[labelName];
                    }

                    // Add label to discussion using REST API (GraphQL doesn't support this)
                    try
                    {
                        await _client.Issue.Labels.AddToIssue(_repositoryOwner, _repositoryName, Convert.ToInt32(discussionId), new[] { labelName });
                    }
                    catch
                    {
                        // GitHub Discussions don't support labels via REST API directly
                        // This is a known limitation - labels need to be added manually or via GitHub UI
                        System.Diagnostics.Debug.WriteLine($"Note: Labels for discussions must be added via GitHub UI");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to add labels: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Adds a comment to a discussion
        /// </summary>
        public async Task<DiscussionComment> AddCommentAsync(string discussionId, string body)
        {
            if (!IsAuthenticated)
            {
                throw new InvalidOperationException("You must be authenticated to add comments");
            }

            try
            {
                var mutation = @"
                    mutation($discussionId: ID!, $body: String!) {
                        addDiscussionComment(input: {discussionId: $discussionId, body: $body}) {
                            comment {
                                id
                                body
                                author {
                                    login
                                }
                                createdAt
                            }
                        }
                    }";

                var result = await _client.Connection.Post<dynamic>(
                    new Uri(GraphQLEndpoint),
                    new { query = mutation, variables = new { discussionId, body } },
                    "application/json",
                    "application/json");

                var comment = result.Body.data.addDiscussionComment.comment;

                return new DiscussionComment
                {
                    Id = comment.id,
                    Body = comment.body,
                    Author = comment.author?.login ?? "Unknown",
                    CreatedAt = DateTime.Parse(comment.createdAt.ToString())
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to add comment: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Upvotes a discussion
        /// </summary>
        public async Task UpvoteDiscussionAsync(string discussionId)
        {
            if (!IsAuthenticated)
            {
                throw new InvalidOperationException("You must be authenticated to upvote");
            }

            try
            {
                var mutation = @"
                    mutation($discussionId: ID!) {
                        addReaction(input: {subjectId: $discussionId, content: THUMBS_UP}) {
                            reaction {
                                id
                            }
                        }
                    }";

                await _client.Connection.Post<dynamic>(
                    new Uri(GraphQLEndpoint),
                    new { query = mutation, variables = new { discussionId } },
                    "application/json",
                    "application/json");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to upvote discussion: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Adds a reaction to a discussion
        /// </summary>
        public async Task AddReactionAsync(string discussionId, string reactionType)
        {
            if (!IsAuthenticated)
            {
                throw new InvalidOperationException("You must be authenticated to add reactions");
            }

            try
            {
                var mutation = @"
                    mutation($discussionId: ID!, $content: ReactionContent!) {
                        addReaction(input: {subjectId: $discussionId, content: $content}) {
                            reaction {
                                id
                            }
                        }
                    }";

                await _client.Connection.Post<dynamic>(
                    new Uri(GraphQLEndpoint),
                    new { query = mutation, variables = new { discussionId, content = reactionType } },
                    "application/json",
                    "application/json");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to add reaction: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Updates a discussion (title and body)
        /// </summary>
        public async Task UpdateDiscussionAsync(string discussionId, string title, string body)
        {
            if (!IsAuthenticated)
            {
                throw new InvalidOperationException("You must be authenticated to update discussions");
            }

            try
            {
                var mutation = @"
                    mutation($discussionId: ID!, $title: String, $body: String) {
                        updateDiscussion(input: {discussionId: $discussionId, title: $title, body: $body}) {
                            discussion {
                                id
                                number
                            }
                        }
                    }";

                await _client.Connection.Post<dynamic>(
                    new Uri(GraphQLEndpoint),
                    new { query = mutation, variables = new { discussionId, title, body } },
                    "application/json",
                    "application/json");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update discussion: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Closes a discussion
        /// </summary>
        public async Task CloseDiscussionAsync(string discussionId)
        {
            if (!IsAuthenticated)
            {
                throw new InvalidOperationException("You must be authenticated to close discussions");
            }

            try
            {
                var mutation = @"
                    mutation($discussionId: ID!) {
                        closeDiscussion(input: {discussionId: $discussionId}) {
                            discussion {
                                id
                                closed
                            }
                        }
                    }";

                await _client.Connection.Post<dynamic>(
                    new Uri(GraphQLEndpoint),
                    new { query = mutation, variables = new { discussionId } },
                    "application/json",
                    "application/json");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to close discussion: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets all available tags from repository labels
        /// </summary>
        public async Task<List<string>> GetAvailableTagsAsync()
        {
            try
            {
                var query = @"
                    query($owner: String!, $name: String!) {
                        repository(owner: $owner, name: $name) {
                            labels(first: 100) {
                                nodes {
                                    name
                                }
                            }
                        }
                    }";

                var result = await _client.Connection.Post<dynamic>(
                    new Uri(GraphQLEndpoint),
                    new { query, variables = new { owner = _repositoryOwner, name = _repositoryName } },
                    "application/json",
                    "application/json");

                var tags = new HashSet<string>();
                foreach (var label in result.Body.data.repository.labels.nodes)
                {
                    string labelName = label.name?.ToString() ?? "";
                    if (labelName.StartsWith(TagPrefix))
                    {
                        tags.Add(labelName.Substring(TagPrefix.Length));
                    }
                }

                return tags.OrderBy(t => t).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get available tags: {ex.Message}", ex);
            }
        }

        public void Dispose()
        {
            // Clear sensitive data
            _currentUsername = null;
            
            // Create new unauthenticated client to clear credentials
            _client = new GitHubClient(new ProductHeaderValue("XrmToolbox-PowerShell-Plugin"));
        }
    }

    /// <summary>
    /// Represents a script item in the gallery (backed by a GitHub Discussion)
    /// </summary>
    public class ScriptGalleryItem
    {
        public string Id { get; set; }
        public int Number { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string Author { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int UpvoteCount { get; set; }
        public int CommentCount { get; set; }
        public string Category { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public List<DiscussionComment> Comments { get; set; }
        public bool IsClosed { get; set; }
        public int ThumbsUpCount { get; set; }
        public int ThumbsDownCount { get; set; }
        public int TotalReactionCount { get; set; }

        /// <summary>
        /// Extracts the PowerShell script content from the discussion body
        /// </summary>
        public string GetScriptContent()
        {
            if (string.IsNullOrEmpty(Body))
                return string.Empty;

            // Look for PowerShell code blocks
            var lines = Body.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var inCodeBlock = false;
            var scriptLines = new List<string>();

            foreach (var line in lines)
            {
                if (line.Trim().StartsWith("```powershell") || line.Trim().StartsWith("```ps1"))
                {
                    inCodeBlock = true;
                    continue;
                }

                if (inCodeBlock && line.Trim().StartsWith("```"))
                {
                    inCodeBlock = false;
                    break;
                }

                if (inCodeBlock)
                {
                    scriptLines.Add(line);
                }
            }

            return string.Join(Environment.NewLine, scriptLines);
        }
    }

    /// <summary>
    /// Represents a comment on a discussion
    /// </summary>
    public class DiscussionComment
    {
        public string Id { get; set; }
        public string Body { get; set; }
        public string Author { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
