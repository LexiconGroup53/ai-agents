using Azure;
using Azure.AI.Inference;

var builder = WebApplication.CreateBuilder(args);

DotNetEnv.Env.Load();
builder.Services.AddOpenApi();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/api/synonym/{word}", (string word) =>
{
    var endpoint = new Uri("https://models.github.ai/inference");
    var credential = new AzureKeyCredential(Environment.GetEnvironmentVariable("GH_MODEL"));
    var model = "openai/gpt-4.1";

    var client = new ChatCompletionsClient(
        endpoint,
        credential,
        new AzureAIInferenceClientOptions());

    var requestOptions = new ChatCompletionsOptions()
    {
        Messages =
        {
            new ChatRequestSystemMessage("respond with a JSON object, where the prompt  is key and an array of five prompt synonyms is its value"),
            new ChatRequestUserMessage(word),
        },
        Model = model
    };

    Response<ChatCompletions> response = client.Complete(requestOptions);
    return response.Value.Content;
});
app.UseHttpsRedirection();
app.Run();