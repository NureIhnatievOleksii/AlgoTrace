using AlgoTrace.Server.Models.DTO;

namespace AlgoTrace.Server.Algorithms.Graph
{
    public class ProgramDependenceGraphAlgorithm : IGraphAlgorithm
    {
        public string Key => "pdg";
        public string Name => "Program Dependence Graph";

        public List<DetailedMatch> Execute(string sourceCode, string targetCode, Dictionary<string, object> parameters, out double similarityScore)
        {
            // Build graphs with Data Dependencies enabled
            var graphA = GraphUtils.BuildGraph(sourceCode, includeDataDeps: true);
            var graphB = GraphUtils.BuildGraph(targetCode, includeDataDeps: true);

            var matches = new List<DetailedMatch>();
            int edgeMatches = 0;

            // Compare Data Edges
            // A match occurs if two edges connect similar types of nodes in both graphs
            foreach (var edgeA in graphA.Edges.Where(e => e.Type == "data"))
            {
                var srcA = graphA.Nodes.FirstOrDefault(n => n.Id == edgeA.SourceId);
                var tgtA = graphA.Nodes.FirstOrDefault(n => n.Id == edgeA.TargetId);

                foreach (var edgeB in graphB.Edges.Where(e => e.Type == "data"))
                {
                    var srcB = graphB.Nodes.FirstOrDefault(n => n.Id == edgeB.SourceId);
                    var tgtB = graphB.Nodes.FirstOrDefault(n => n.Id == edgeB.TargetId);

                    if (srcA != null && tgtA != null && srcB != null && tgtB != null)
                    {
                        if (srcA.Type == srcB.Type && tgtA.Type == tgtB.Type)
                        {
                            edgeMatches++;
                            matches.Add(new DetailedMatch
                            {
                                Id = edgeMatches + 5000,
                                Type = "Data Dependency Match",
                                LeftLines = new List<int> { srcA.LineIndex + 1, tgtA.LineIndex + 1 },
                                RightLines = new List<int> { srcB.LineIndex + 1, tgtB.LineIndex + 1 },
                                Severity = "high"
                            });
                            break; // Count unique edge mapping roughly
                        }
                    }
                }
            }
            similarityScore = graphA.Edges.Count > 0 ? Math.Min(100, (double)edgeMatches / graphA.Edges.Count * 100) : 0;
            return matches;
        }
    }
}