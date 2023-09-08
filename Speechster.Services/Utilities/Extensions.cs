using Microsoft.IdentityModel.Tokens;
using SharpToken;

namespace Speechster.Services.Utilities;

public static class Extensions
{
    public static string DecodeBase64URL(this string url)
    {
        try
        {
            var decodedPath = Base64UrlEncoder.Decode(url);
            return decodedPath;
        }
        catch (FormatException)
        {
            return string.Empty;
        }
    }

    public static int TokensCount(this string text, string model)
    {
        if (string.IsNullOrEmpty(model))
            throw new ArgumentNullException(nameof(model));

        var encoding = GptEncoding.GetEncodingForModel(model);
        var encoded = encoding.Encode(text);
        return encoded.Count;
    }
}
