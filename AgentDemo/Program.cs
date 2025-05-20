using System.Text.Json;
using AgentDemo.Models;
using Azure;
using Azure.AI.Inference;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Images;


var builder = WebApplication.CreateBuilder(args);

DotNetEnv.Env.Load();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("openai",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173", "http://localhost:5174")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSession();

app.MapPost("/api/image/single", (UserInput userInput) => {
    OpenAIClient client = new(Environment.GetEnvironmentVariable("OPENAI_API_KEY"));
    ImageClient imageClient = client.GetImageClient("dall-e-3");

    ImageGenerationOptions options = new()
    {
        Quality = GeneratedImageQuality.High,
        Size = GeneratedImageSize.W1024xH1024,
        Style = GeneratedImageStyle.Vivid,
        ResponseFormat = GeneratedImageFormat.Bytes
    };
    
    GeneratedImage image = imageClient.GenerateImage($"{userInput.Input}", options);;
    
    File.WriteAllBytes("Assets/generatedImage.png", image.ImageBytes.ToArray());
    return "success";
});

app.MapPost("/api/chat/", (HttpContext _context, UserInput userInput) =>
{
    OpenAIClient client = new(Environment.GetEnvironmentVariable("OPENAI_API_KEY"));
    ChatClient chatClient = client.GetChatClient("gpt-4o");
    
    string chatSessionHistory = _context.Session.GetString("chat_history")!;
    Console.WriteLine(chatSessionHistory);
    List<ChatContent> persisted = chatSessionHistory is null ?
        new List<ChatContent>()
        {
            { new ChatContent("system", "Respond with accuracy") }
        } :
        JsonSerializer.Deserialize<List<ChatContent>>(chatSessionHistory);
    
    /*List<ChatMessage> messages = new()
    {
        { new SystemChatMessage("Respond with accuracy") },
        { new UserChatMessage("What is the capital of France?")},
        {new AssistantChatMessage("Capital of France is Paris")}
    };*/
    persisted.Add(new ChatContent("user", userInput.Input));

    List<ChatMessage> messages = persisted.Select(chatContent => chatContent.Role switch
    {
        "assistant" => new AssistantChatMessage(chatContent.Content) as ChatMessage,
        "user" => new UserChatMessage(chatContent.Content),
        "system" => new SystemChatMessage(chatContent.Content),
        _ => new AssistantChatMessage("I don't know what to say")
    }).ToList();
    
    ChatCompletion completion = chatClient.CompleteChat(messages);
    persisted.Add(new ChatContent("assistant", completion.Content[0].Text));
    
    _context.Session.SetString("chat_history", JsonSerializer.Serialize(persisted));
    
    /*foreach (ChatMessage message in messages)
    {
        Console.WriteLine(message.Content[0].Text);
    }*/
    return completion.Content[0].Text;
});


/*app.MapPost("/api/translate", async (Translation translation) =>
{
    var apiKey = "not-needed"; 
    var baseUrl = "http://localhost:1234";   

    var openAiClient = new OpenAIClient(new OpenAIAuthentication(apiKey), new OpenAIClientSettings(baseUrl, "v1"));

    var chatRequest = new ChatRequest(
        new[]
        {
            new Message(Role.System, $"Respond with a single translation of the prompt text to {translation.Language}, with no formating"),
            new Message(Role.User, translation.Phrase)
        },
        model: "gemma-3-12b-it-qat", 
        temperature: 0.7f
    );
    

    var result = await openAiClient.ChatEndpoint.GetCompletionAsync(chatRequest);

    return result.FirstChoice.Message.Content;
  
});*/

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
app.UseCors("openai");
app.Run();

record ChatContent(string Role, string Content);