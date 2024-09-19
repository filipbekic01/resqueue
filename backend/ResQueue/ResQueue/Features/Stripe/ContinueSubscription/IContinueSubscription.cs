namespace ResQueue.Features.Stripe.ContinueSubscription;

public interface IContinueSubscriptionFeature
{
    Task<OperationResult<ContinueSubscriptionResponse>> ExecuteAsync(ContinueSubscriptionRequest request);
}