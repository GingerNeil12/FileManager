using FileManager.Application.Common.Models.Specifications;

using Microsoft.EntityFrameworkCore;

namespace FileManager.Persistence.Specifications;

public static class SpecificationEvaluator<T> where T : class
{
    public static IQueryable<T> GetQuery(IQueryable<T> inputQuery, Specification<T> specification)
    {
        IQueryable<T> query = specification.Includes
            .Aggregate(inputQuery, (current, include) => current.Include(include));

        if (specification.Criteria is not null)
        {
            query = query.Where(specification.Criteria);
        }

        if (specification.OrderBy is not null)
        {
            query = query.OrderBy(specification.OrderBy);
        }
        else if (specification.OrderByDescending is not null)
        {
            query = query.OrderByDescending(specification.OrderByDescending);
        }

        return query;
    }
}
