namespace FileManager.Application.Common.Interfaces;

public interface IDepartmentRepository
{
    Task<bool> DoesExistAsync(int departmentId, CancellationToken ct);
}