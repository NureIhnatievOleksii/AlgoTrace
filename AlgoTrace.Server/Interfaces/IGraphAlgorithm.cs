using AlgoTrace.Server.Algorithms.Graph;
using AlgoTrace.Server.Models.DTO;

namespace AlgoTrace.Server.Interfaces
{
    public interface IGraphAlgorithm
    {
        string Key { get; }
        string Name { get; }
        List<DetailedMatch> Execute(
            string sourceCode,
            string targetCode,
            Dictionary<string, object> parameters,
            out double similarityScore,
            out CodeGraph graphA,
            out CodeGraph graphB
        );
    }
}
