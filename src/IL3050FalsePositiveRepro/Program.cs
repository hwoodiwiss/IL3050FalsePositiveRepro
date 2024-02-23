using GitHub;
using GitHub.Octokit.Authentication;
using GitHub.Octokit.Client;
using GitHub.Repos.Item.Item.Pulls.Item.Reviews;

var adapter = RequestAdapter.Create(new TokenAuthenticationProvider("client_id", "token"));
var client = new GitHubClient(adapter);

await client.Repos["hwoodiwiss"]["HwoodiwissHelper"].Pulls[123].Reviews.PostAsync(new ReviewsPostRequestBody
{
	Body = "Automatically approving pull request",
	Event = ReviewsPostRequestBody_event.APPROVE,
});
