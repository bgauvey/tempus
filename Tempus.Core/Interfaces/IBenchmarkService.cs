using Tempus.Core.Models;

namespace Tempus.Core.Interfaces;

public interface IBenchmarkService
{
    /// <summary>
    /// Generates a comprehensive benchmark report comparing user metrics against industry standards
    /// </summary>
    Task<BenchmarkReport> GenerateBenchmarkReportAsync(string userId, int periodDays = 30);

    /// <summary>
    /// Gets the default industry benchmark standards
    /// </summary>
    BenchmarkData GetIndustryBenchmarks();

    /// <summary>
    /// Compares a specific metric against its benchmark
    /// </summary>
    BenchmarkComparison CompareMetric(string category, string metricName, double actualValue, double benchmarkValue, bool higherIsBetter = true);

    /// <summary>
    /// Calculates an overall benchmark score (0-100)
    /// </summary>
    double CalculateOverallScore(List<BenchmarkComparison> comparisons);
}
