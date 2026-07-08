using System;
using System.Collections.Generic;
using AlgoTrace.Server.Interfaces;
using AlgoTrace.Server.Models.DTO.Analysis;
using AlgoTrace.Server.Utils;

namespace AlgoTrace.Server.Algorithms.Textual
{
    public class RabinKarpAlgorithm : ITextAlgorithm
    {
        public string Key => "rabin_karp";
        public string Name => "Rabin-Karp Block Search";

        private const long BaseP = 31;
        private const long Modulus = 1_000_000_009;

        public List<DetailedMatch> Execute(string source, string target, out double similarityScore)
        {
            var sLines = SourceNormalizer.GetLines(source);
            var tLines = SourceNormalizer.GetLines(target);
            var matches = new List<DetailedMatch>();
            int blockSize = 5;
            int matchCounter = 0;

            if (sLines.Length < blockSize || tLines.Length < blockSize)
            {
                similarityScore = 0.0;
                return matches;
            }

            var sNorms = new string[sLines.Length];
            bool[] isValidSourceLine = new bool[sLines.Length];
            int validSourceLines = 0;

            for (int i = 0; i < sLines.Length; i++)
            {
                sNorms[i] = SourceNormalizer.NormalizeLine(sLines[i]);
                if (sNorms[i].Length >= 5)
                {
                    isValidSourceLine[i] = true;
                    validSourceLines++;
                }
            }

            var tNorms = new string[tLines.Length];
            for (int j = 0; j < tLines.Length; j++)
            {
                tNorms[j] = SourceNormalizer.NormalizeLine(tLines[j]);
            }

            var targetHashes = BuildTargetHashes(tNorms, blockSize);

            long highestPower = 1;
            for (int i = 0; i < blockSize - 1; i++)
            {
                highestPower = (highestPower * BaseP) % Modulus;
            }

            bool[] isMatched = new bool[sLines.Length];
            int lastReportedSourceEnd = -1;

            long currentSourceHash = 0;
            int currentBlockCharLength = 0;

            for (int i = 0; i <= sLines.Length - blockSize; i++)
            {
                if (i == 0)
                {
                    for (int k = 0; k < blockSize; k++)
                    {
                        currentSourceHash = (currentSourceHash * BaseP + GetStringHash(sNorms[k])) % Modulus;
                        currentBlockCharLength += sNorms[k].Length;
                    }
                }
                else
                {
                    long oldLineHash = GetStringHash(sNorms[i - 1]);
                    currentSourceHash = (currentSourceHash - (oldLineHash * highestPower) % Modulus + Modulus) % Modulus;

                    long newLineHash = GetStringHash(sNorms[i + blockSize - 1]);
                    currentSourceHash = (currentSourceHash * BaseP + newLineHash) % Modulus;

                    currentBlockCharLength += sNorms[i + blockSize - 1].Length - sNorms[i - 1].Length;
                }

                if (currentBlockCharLength < 25)
                    continue;

                if (targetHashes.TryGetValue(currentSourceHash, out var candidateIndices))
                {
                    int bestJ = -1;

                    foreach (int j in candidateIndices)
                    {
                        if (IsExactMatch(sNorms, i, tNorms, j, blockSize))
                        {
                            bestJ = j;
                            break;
                        }
                    }

                    if (bestJ != -1)
                    {
                        for (int k = 0; k < blockSize; k++)
                        {
                            if (isValidSourceLine[i + k])
                            {
                                isMatched[i + k] = true;
                            }
                        }

                        if (i > lastReportedSourceEnd)
                        {
                            matches.Add(new DetailedMatch
                            {
                                Id = 4000 + matchCounter++,
                                Type = "Exact Block Match",
                                LeftLines = new List<int> { i + 1, i + blockSize },
                                RightLines = new List<int> { bestJ + 1, bestJ + blockSize },
                                Severity = "high"
                            });
                            lastReportedSourceEnd = i + blockSize - 1;
                        }
                    }
                }
            }

            if (validSourceLines == 0)
            {
                similarityScore = 0.0;
            }
            else
            {
                int matchedValidLinesCount = 0;
                for (int i = 0; i < sLines.Length; i++)
                {
                    if (isMatched[i]) matchedValidLinesCount++;
                }

                similarityScore = Math.Min(100.0, (double)matchedValidLinesCount / validSourceLines * 100);
            }

            return matches;
        }

        private Dictionary<long, List<int>> BuildTargetHashes(string[] tNorms, int blockSize)
        {
            var map = new Dictionary<long, List<int>>();
            long hash = 0;
            long highestPower = 1;

            for (int i = 0; i < blockSize - 1; i++)
                highestPower = (highestPower * BaseP) % Modulus;

            for (int j = 0; j <= tNorms.Length - blockSize; j++)
            {
                if (j == 0)
                {
                    for (int k = 0; k < blockSize; k++)
                        hash = (hash * BaseP + GetStringHash(tNorms[k])) % Modulus;
                }
                else
                {
                    long oldHash = GetStringHash(tNorms[j - 1]);
                    hash = (hash - (oldHash * highestPower) % Modulus + Modulus) % Modulus;
                    long newHash = GetStringHash(tNorms[j + blockSize - 1]);
                    hash = (hash * BaseP + newHash) % Modulus;
                }

                if (!map.ContainsKey(hash))
                    map[hash] = new List<int>();

                map[hash].Add(j);
            }

            return map;
        }

        private long GetStringHash(string str)
        {
            long hash = 0;
            for (int i = 0; i < str.Length; i++)
            {
                hash = (hash * BaseP + str[i]) % Modulus;
            }
            return hash;
        }

        private bool IsExactMatch(string[] sNorms, int sIndex, string[] tNorms, int tIndex, int blockSize)
        {
            for (int k = 0; k < blockSize; k++)
            {
                if (sNorms[sIndex + k] != tNorms[tIndex + k])
                    return false;
            }
            return true;
        }
    }
}