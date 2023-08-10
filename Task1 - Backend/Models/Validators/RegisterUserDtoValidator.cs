using FluentValidation;
using Task1Backend;
using Task1Backend.Models;

namespace Task1___Backend.Models.Validators
{
    public class RegisterUserDtoValidator : AbstractValidator<RegisterUserDto>
    {
        /// <summary>
        /// Checking if fields are not empty, password has specified lenght,
        /// confirm password is the same as password,
        /// email adress is actual email adress is not empty and it doesn't exist in database
        /// </summary>
        /// <param name="dataContext"></param>
        public RegisterUserDtoValidator(DataContext dataContext)
        {
            RuleFor(x => x.Email).
                NotEmpty().
                EmailAddress();
            RuleFor(x => x.Password).
                NotEmpty().
                MinimumLength(6);
            RuleFor(x => x.ConfirmPassword).
                NotEmpty().
                Equal(e => e.Password);
            RuleFor(x => x.Email).Custom((value, context) =>
            {
                var emailInUse = dataContext.Users.Any(x => x.Email == value);
                if (emailInUse)
                {
                    context.AddFailure("Email", "Email adress is already in use.");
                }
            });
        }
    }
}