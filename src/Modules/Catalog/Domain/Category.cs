namespace Ecommerce.Catalog.Domain;

public class Category
{
    public Guid Id { get; private set; }
    public Guid? ParentId { get; private set; }
    public string Name { get; private set; } = default!;
    public string Slug { get; private set; } = default!;

<<<<<<< HEAD
    private readonly List<Category> _children = new();
    public IReadOnlyList<Category> Children => _children.AsReadOnly();

=======
>>>>>>> feature/orders-logic
    private Category() { }

    public static Category Create(string name, string slug, Guid? parentId = null)
    {
        return new Category
        {
            Id = Guid.NewGuid(),
            Name = name,
            Slug = slug,
            ParentId = parentId
        };
    }
}
