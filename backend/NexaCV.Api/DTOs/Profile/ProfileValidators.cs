using FluentValidation;
using NexaCV.Api.DTOs.Profile;

namespace NexaCV.Api.DTOs.Profile;

public class CreateProfileRequestValidator : AbstractValidator<CreateProfileRequest>
{
    public CreateProfileRequestValidator()
    {
        RuleFor(x => x.Bio).MaximumLength(500);
    }
}

public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.FirstName).MaximumLength(100);
        RuleFor(x => x.LastName).MaximumLength(100);
        RuleFor(x => x.Username).MaximumLength(50);
        RuleFor(x => x.Bio).MaximumLength(500);
    }
}
