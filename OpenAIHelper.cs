using RestSharp;
using System;
using System.Threading.Tasks;

public class OpenAIHelper
{
    private readonly string apiKey;

    public OpenAIHelper(string apiKey)
    {
        this.apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey), "API key cannot be null.");
    }

    public async Task<string> GetAISuggestions(string prompt, int maxTokens = 100, double temperature = 0.7)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            throw new ArgumentException("Prompt cannot be null or empty.", nameof(prompt));

        var options = new RestClientOptions("https://api.openai.com/v1/completions")
        {
            ThrowOnAnyError = true // Ensures all errors throw exceptions
        };

        var client = new RestClient(options);
        var request = new RestRequest
        {
            Method = Method.Post 
        };

        request.AddHeader("Authorization", $"Bearer {apiKey}");
        request.AddHeader("Content-Type", "application/json");

        var body = new
        {
            model = "text-davinci-003",
            prompt = prompt,
            max_tokens = maxTokens,
            temperature = temperature
        };

        request.AddJsonBody(body);

        try
        {
            var response = await client.ExecuteAsync(request);
            if (response.IsSuccessful && response.Content != null)
            {
                dynamic jsonResponse = Newtonsoft.Json.JsonConvert.DeserializeObject(response.Content);
                return jsonResponse.choices[0].text.ToString().Trim();
            }
            else
            {
                throw new Exception($"API Error: {response.StatusDescription} - {response.Content}");
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error calling OpenAI API", ex);
        }
    }
}
