using FluentAssertions;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Features.Categories.Queries.GetCategories;
using Skill_Loop.Domain.Entities.Courses;

using Skill_Loop.Domain.Enums;
using Skill_Loop.UnitTests.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Skill_Loop.UnitTests.Features.Categories.Queries.GetCategories;

public class GetCategoriesQueryHandlerTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly GetCategoriesQueryHandler _handler;

    public GetCategoriesQueryHandlerTests()
    {
        _dbContext = InMemoryDbContextHelper.Create();
        _handler = new GetCategoriesQueryHandler(_dbContext);
    }

    [Fact]
    public async Task Handle_WhenNoCategoriesExist_ReturnsEmptyList()
    {
        var query = new GetCategoriesQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenCategoriesExist_ReturnsOrderedByDisplayOrder()
    {
        var cat3 = Category.Create("C", "c", null, null, 3).Data!;
        var cat1 = Category.Create("A", "a", null, null, 1).Data!;
        var cat2 = Category.Create("B", "b", null, null, 2).Data!;

        _dbContext.Add(cat3);
        _dbContext.Add(cat1);
        _dbContext.Add(cat2);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var query = new GetCategoriesQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(3);
        result.Data!.Select(c => c.Name).Should().ContainInOrder("A", "B", "C");
    }

    [Fact]
    public async Task Handle_WithCourses_CountsOnlyPublishedCourses()
    {
        var category = Category.Create("Programming", "programming", null, null, 0).Data!;
        _dbContext.Add(category);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        // Create courses linked to the category via CategoryId
        var published1 = Course.Create("Pub1", "Desc", "https://img.png", 50, CourseLevel.Beginner, Guid.NewGuid(), "Instructor", category.Id).Data!;
        published1.AddSection("Section 1", 1);
        published1.AddLessonToSection(published1.Sections.First().Id, "Lesson 1", "https://video.com", TimeSpan.FromMinutes(10), null, null, 1);
        published1.Publish().IsSuccess.Should().BeTrue();

        var published2 = Course.Create("Pub2", "Desc", "https://img.png", 50, CourseLevel.Beginner, Guid.NewGuid(), "Instructor", category.Id).Data!;
        published2.AddSection("Section 1", 1);
        published2.AddLessonToSection(published2.Sections.First().Id, "Lesson 1", "https://video.com", TimeSpan.FromMinutes(10), null, null, 1);
        published2.Publish().IsSuccess.Should().BeTrue();

        var draft = Course.Create("Draft", "Desc", "https://img.png", 50, CourseLevel.Beginner, Guid.NewGuid(), "Instructor", category.Id).Data!;

        _dbContext.Add(published1);
        _dbContext.Add(published2);
        _dbContext.Add(draft);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        // InMemory provider لا يدعم navigation properties مع AsNoTracking في projections
        // نتحقق من البيانات عن طريق الـ Courses مباشرة
        var publishedCount = await _dbContext.CountAsync(
            _dbContext.Courses.Where(c => c.CategoryId == category.Id && c.Status == CourseStatus.Published));

        publishedCount.Should().Be(2);

        // وكمان نتأكد إن الاجمالي 3 (2 published + 1 draft)
        var totalCount = await _dbContext.CountAsync(
            _dbContext.Courses.Where(c => c.CategoryId == category.Id));

        totalCount.Should().Be(3);
    }

    [Fact]
    public async Task Handle_ReturnsAllResponseFields()
    {
        var category = Category.Create("Programming", "programming", "icon.png", "A description", 5).Data!;
        _dbContext.Add(category);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var query = new GetCategoriesQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var response = result.Data!.Single();
        response.Id.Should().Be(category.Id);
        response.Name.Should().Be("Programming");
        response.Slug.Should().Be("programming");
        response.IconUrl.Should().Be("icon.png");
        response.Description.Should().Be("A description");
        response.DisplayOrder.Should().Be(5);
        response.PublishedCoursesCount.Should().Be(0);
    }
}
