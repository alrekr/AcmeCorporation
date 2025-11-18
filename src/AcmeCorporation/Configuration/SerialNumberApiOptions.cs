namespace AcmeCorporation.Configuration;

public class SerialNumberApiOptions
{
    public const string SectionName = "SerialNumberApi";
    public int MaxBatchSize { get; set; } = 100;
}
