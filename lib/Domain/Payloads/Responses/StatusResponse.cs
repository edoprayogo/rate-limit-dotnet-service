namespace Domain.Payloads.Responses;

public class StatusResponse
{
    public int Status { get; set; }
    public string Message { get; set; }
    public DateTime Timestamp { get; set; }

    public StatusResponse(int status, string message)
    {
        Status = status;
        Message = message;
        Timestamp = DateTime.UtcNow;
    }
}


