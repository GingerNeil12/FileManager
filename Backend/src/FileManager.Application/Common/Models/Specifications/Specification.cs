using System.Linq.Expressions;

namespace FileManager.Application.Common.Models.Specifications;

public abstract class Specification<T>
{
    private readonly List<Expression<Func<T, object>>> _includes = [];

    public Expression<Func<T, bool>>? Criteria { get; private set; }
    public IReadOnlyList<Expression<Func<T, object>>> Includes => _includes;
    public Expression<Func<T, object>>? OrderBy { get; private set; }
    public Expression<Func<T, object>>? OrderByDescending { get; private set; }
    public int PageNumber { get; private set; } = 1;
    public int PageSize { get; private set; } = 20;

    protected void AddCriteria(Expression<Func<T, bool>> criteria) => Criteria = criteria;

    protected void AddInclude(Expression<Func<T, object>> include) => _includes.Add(include);

    protected void ApplyOrderBy(Expression<Func<T, object>> expr) => OrderBy = expr;

    protected void ApplyOrderByDescending(Expression<Func<T, object>> expr) => OrderByDescending = expr;

    protected void ApplyPaging(int pageNumber, int pageSize)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}
