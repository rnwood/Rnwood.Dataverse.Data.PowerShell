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
        private const string DefaultCategoryName = "Scripts";
        private const string FallbackCategoryName = "General";

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
        /// Authenticates using a Personal Access Token
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
        /// Gets all discussions from the repository
        /// </summary>
        public async Task<List<ScriptGalleryItem>> GetDiscussionsAsync(string category = null)
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
                                    comments(first: 1) {
                                        totalCount
                                    }
                                    category {
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
                    discussions.Add(new ScriptGalleryItem
                    {
                        Id = node.id,
                        Number = node.number,
                        Title = node.title,
                        Body = node.body,
                        Author = node.author?.login ?? "Unknown",
                        CreatedAt = DateTime.Parse(node.createdAt.ToString()),
                        UpdatedAt = DateTime.Parse(node.updatedAt.ToString()),
                        UpvoteCount = node.upvoteCount,
                        CommentCount = node.comments.totalCount,
                        Category = node.category?.name ?? "General"
                    });
                }

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
        /// Creates a new discussion with a PowerShell script
        /// </summary>
        public async Task<ScriptGalleryItem> CreateDiscussionAsync(string title, string scriptContent, string categoryId = null)
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
                
                // Use provided category or find "Scripts" category
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

                return new ScriptGalleryItem
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
                    Category = "Scripts"
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create discussion: {ex.Message}", ex);
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
        public List<DiscussionComment> Comments { get; set; }

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
