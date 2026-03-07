using AlgoTrace.Server.Models.DTO;

namespace AlgoTrace.Server.Algorithms.Graph
{
    public class SubgraphIsomorphismAlgorithm : IGraphAlgorithm
    {
        public string Key => "subgraph_isomorphism";
        public string Name => "Subgraph Isomorphism Search";

        public List<DetailedMatch> Execute(string sourceCode, string targetCode, Dictionary<string, object> parameters, out double similarityScore)
        {
            var graphA = GraphUtils.BuildGraph(sourceCode);
            var graphB = GraphUtils.BuildGraph(targetCode);

            var matches = new List<DetailedMatch>();
            
            // Heuristic: Check if the sequence of node types in A exists in B
            // This simulates finding if Graph A is embedded in Graph B
            
            var typesA = graphA.Nodes.Select(n => n.Type).ToList();
            var typesB = graphB.Nodes.Select(n => n.Type).ToList();

            int matchedNodes = 0;
            
            for (int i = 0; i <= typesB.Count - typesA.Count; i++)
            {
                bool isSubGraph = true;
                for (int j = 0; j < typesA.Count; j++)
                {
                    if (typesB[i + j] != typesA[j])
                    {
                        isSubGraph = false;
                        break;
                    }
                }

                if (isSubGraph)
                {
                    matchedNodes = typesA.Count;
                    similarityScore = 100.0;
                    return matches; // Found exact subgraph structure
                }
            }
            similarityScore = 0.0;
            return matches;
        }
    }
}