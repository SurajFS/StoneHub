using Catalog.Domain;
using Xunit;

namespace Catalog.Tests;

public sealed class CategoryTests
{
    [Fact]
    public void Create_WithValidName_SucceedsAndDerivesSlug()
    {
        var result = Category.Create("Kota Stone", parentId: null, displayOrder: 4);

        Assert.True(result.IsSuccess);
        var category = result.Value;
        Assert.Equal("Kota Stone", category.Name);
        Assert.Equal("kota-stone", category.Slug);
        Assert.Null(category.ParentId);
        Assert.Equal(4, category.DisplayOrder);
        Assert.True(category.IsActive);
    }

    [Fact]
    public void Create_TrimsNameAndPassesThroughParent()
    {
        var parentId = Guid.NewGuid();

        var result = Category.Create("  Italian Marble  ", parentId, displayOrder: 1);

        Assert.True(result.IsSuccess);
        Assert.Equal("Italian Marble", result.Value.Name);
        Assert.Equal("italian-marble", result.Value.Slug);
        Assert.Equal(parentId, result.Value.ParentId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("!!!")] // no alphanumerics → empty slug
    public void Create_WithUnusableName_Fails(string name)
    {
        var result = Category.Create(name, parentId: null, displayOrder: 0);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Create_NegativeDisplayOrder_ClampsToZero()
    {
        var result = Category.Create("Granite", parentId: null, displayOrder: -5);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value.DisplayOrder);
    }

    [Theory]
    [InlineData("Black  &  White", "black-white")]
    [InlineData("  Floor Tiles!! ", "floor-tiles")]
    [InlineData("Krishna Idol", "krishna-idol")]
    public void Slugify_CollapsesNonAlphanumericRunsToSingleHyphens(string input, string expected) =>
        Assert.Equal(expected, Category.Slugify(input));

    [Fact]
    public void Rename_UpdatesNameSlugAndOrder()
    {
        var category = Category.Create("Tiles", parentId: null, displayOrder: 3).Value;

        var result = category.Rename("Floor Tiles", displayOrder: 7);

        Assert.True(result.IsSuccess);
        Assert.Equal("Floor Tiles", category.Name);
        Assert.Equal("floor-tiles", category.Slug);
        Assert.Equal(7, category.DisplayOrder);
    }

    [Fact]
    public void Rename_WithBlankName_FailsAndLeavesCategoryUnchanged()
    {
        var category = Category.Create("Tiles", parentId: null, displayOrder: 3).Value;

        var result = category.Rename("   ", displayOrder: 9);

        Assert.True(result.IsFailure);
        Assert.Equal("Tiles", category.Name);
        Assert.Equal("tiles", category.Slug);
        Assert.Equal(3, category.DisplayOrder);
    }

    [Fact]
    public void ActivateDeactivate_TogglesIsActive()
    {
        var category = Category.Create("Handicrafts", parentId: null, displayOrder: 5).Value;

        category.Deactivate();
        Assert.False(category.IsActive);

        category.Activate();
        Assert.True(category.IsActive);
    }
}
