namespace ResQueue.Features.Stripe.ChangePlan;

public interface IChangePlanFeature
{
    Task<OperationResult<ChangePlanResponse>> ExecuteAsync(ChangePlanRequest request);
}