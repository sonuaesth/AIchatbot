using Microsoft.Extensions.Options;

namespace AiChat.Backend.Contracts.Options;

public class OpenAIOptionsValidator : IValidateOptions<OpenAIOptions>
{
    public ValidateOptionsResult Validate(string? name, OpenAIOptions options)
    {
        if(string.IsNullOrEmpty(options.ApiKey)) return ValidateOptionsResult.Fail("OpenAI: Api key is required");
        if(string.IsNullOrEmpty(options.BaseUrl)) return ValidateOptionsResult.Fail("OpenAI:Base url is required");
        if(string.IsNullOrWhiteSpace(options.Model)) return ValidateOptionsResult.Fail("OpenAI: Model is required");

        return ValidateOptionsResult.Success;
    }
}