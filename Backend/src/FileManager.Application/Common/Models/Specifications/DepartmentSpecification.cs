using FileManager.Domain.Models;

namespace FileManager.Application.Common.Models.Specifications;

public class DepartmentSpecification : Specification<Department>
{
    public DepartmentSpecification(string? nameSearch, int pageNumber, int pageSize)
    {
        if (!string.IsNullOrWhiteSpace(nameSearch))
        {
            AddCriteria(x => x.Name.Contains(nameSearch));
        }

        ApplyOrderBy(x => x.Name);
        ApplyPaging(pageNumber, pageSize);
    }
}
