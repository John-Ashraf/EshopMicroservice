using BuildingBlocks.CQRS;
using CatalogAPI.Models;
using FluentValidation;

namespace CatalogAPI.Products.CreateProduct;
public record CreateProductCommand(string Name, List<string> Category, string Description, string ImageFile, decimal Price)
    : ICommand<CreateProductResult>;
public record CreateProductResult(Guid Id);
public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        _ = RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");
        _ = RuleFor(x => x.Category).NotEmpty().WithMessage("Category is required");
        _ = RuleFor(x => x.ImageFile).NotEmpty().WithMessage("ImageFile is required");
        _ = RuleFor(x => x.Price).GreaterThan(0).WithMessage("Price must be greater than 0");
    }
}
internal class CreateProductCommandHandler(IDocumentSession session) : ICommandHandler<CreateProductCommand, CreateProductResult>
{
    public async Task<CreateProductResult> Handle(CreateProductCommand command, CancellationToken cancellationToken)
    {

        var product = new Product
        {
            Name = command.Name,
            Category = command.Category,
            Description = command.Description,
            ImageFile = command.ImageFile,
            Price = command.Price
        };

        // save to database
        session.Store(product);
        await session.SaveChangesAsync();
        return new CreateProductResult(product.Id);
    }
}

