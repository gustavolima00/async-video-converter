namespace Services.Exceptions;

public class SendWebhookException : ServicesException
{

  public SendWebhookException(
    string webhookUrl,
    string serializedData,
    HttpResponseMessage? responseMessage
  ) : base(BuildMessage(webhookUrl, serializedData, responseMessage))
  {

    WebhookUrl = webhookUrl;
    SerializedData = serializedData;
    ResponseMessage = responseMessage;
  }

  public string WebhookUrl { get; }
  public string SerializedData { get; }
  public HttpResponseMessage? ResponseMessage { get; }

  private static string BuildMessage(
    string webhookUrl,
    string serializedData,
    HttpResponseMessage? responseMessage
  )
  {
    var message = $"Error sending webhook to {webhookUrl} with data {serializedData}";
    if (responseMessage != null)
    {
      var statusCode = (int)responseMessage.StatusCode;
      var body = responseMessage.Content.ReadAsStringAsync().Result;
      message += $" with status code {statusCode} and body {body}";
    }
    return message;
  }
}
