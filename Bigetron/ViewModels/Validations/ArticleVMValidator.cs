namespace Bigetron.ViewModels.Validations
{
    using FluentValidation;

    public class ArticleVMValidator: AbstractValidator<ArticleVM>
    {
        public ArticleVMValidator()
        {
            RuleFor(c => c.Title).NotEmpty().WithMessage("Title cannot be empty.");
            RuleFor(c => c.Title).Must(x => x.Length <= 100).WithMessage("Length of title must be less than 100.");
            RuleFor(c => c.CoverImageUrl).NotEmpty().WithMessage("Cover image url cannot be empty.");
            RuleFor(c => c.Content).NotEmpty().WithMessage("Content cannot be empty.");
        }
    }
}
