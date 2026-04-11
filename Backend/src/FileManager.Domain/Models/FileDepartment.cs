namespace FileManager.Domain.Models;

public class FileDepartment
{
    private FileMetadata? _fileMetadata;
    private Department? _department;

    private FileDepartment(
        Guid fileMetadataId,
        int departmentId,
        DateTime assignedOn,
        DateTime? downloadedOn,
        DateTime? deletedOn,
        Guid? downloadedBy,
        Guid? deletedBy)
    {
        FileMetadataId = fileMetadataId;
        DepartmentId = departmentId;
        AssignedOn = assignedOn;
        DownloadedOn = downloadedOn;
        DeletedOn = deletedOn;
        DownloadedBy = downloadedBy;
        DeletedBy = deletedBy;
    }

    public Guid FileMetadataId { get; }
    public int DepartmentId { get; }
    public DateTime AssignedOn { get; }
    public DateTime? DownloadedOn { get; private set; }
    public DateTime? DeletedOn { get; private set; }
    public Guid? DownloadedBy { get; private set; }
    public Guid? DeletedBy { get; private set; }

    public virtual FileMetadata FileMetadata
    {
        set => _fileMetadata = value;
        get => _fileMetadata ?? throw new InvalidOperationException("FileMetadata not intialized.");
    }

    public virtual Department Department
    {
        set => _department = value;
        get => _department ?? throw new InvalidOperationException("Department not intialized.");
    }

    public static FileDepartment Create(Guid fileMetadataId, int departmentId)
        => new(fileMetadataId, departmentId, DateTime.UtcNow, null, null, null, null);

    public void Download(Guid downloadedBy)
    {
        DownloadedOn = DateTime.UtcNow;
        DownloadedBy = downloadedBy;
    }

    public void Delete(Guid deletedBy)
    {
        DeletedOn = DateTime.UtcNow;
        DeletedBy = deletedBy;
    }
}