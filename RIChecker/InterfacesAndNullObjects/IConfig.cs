
namespace RIChecker.Interfaces
{
    public interface IConfig
    {
        string ExtractsFolder { get; set; }
        string KeysFolder { get; set; }
        string ResultsFolder { get; set; }
        int SampleSize { get; set; }
        string Title { get; set; }
    }
}