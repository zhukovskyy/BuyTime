namespace BuyTime_Application.Feedback.Command.Create;

public class CreateFeedbackResult
{
    public Guid FeedbackId { get; set; }
    public CreateFeedbackResult(Guid feedbackId)
    {
        FeedbackId = feedbackId;
    }

    public CreateFeedbackResult()
    {
    }
}