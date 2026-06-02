using System.Collections.Generic;

namespace IdleGame.Data
{
    public static partial class TalentData
    {
        private static TalentDefinition[] BuildTalents()
        {
            var result = new List<TalentDefinition>();
            TalentDefinition[] previousDepth = new TalentDefinition[0];

            for (int depth = 0; depth < talentSpecs.Length; depth++)
            {
                TalentSpec[] specs = talentSpecs[depth];
                var currentDepth = new TalentDefinition[specs.Length];
                for (int nodeIndex = 0; nodeIndex < specs.Length; nodeIndex++)
                {
                    TalentSpec spec = specs[nodeIndex];
                    currentDepth[nodeIndex] = new TalentDefinition(
                        spec.Id,
                        spec.DisplayName,
                        spec.Icon,
                        GetDepthLabel(depth),
                        nodeIndex,
                        depth,
                        DefaultMaxLevel,
                        DefaultCost,
                        spec.ValuePerLevel,
                        spec.EffectKind,
                        GetPrerequisiteIds(previousDepth, nodeIndex, specs.Length));
                    result.Add(currentDepth[nodeIndex]);
                }

                previousDepth = currentDepth;
            }

            return result.ToArray();
        }

        private static string[] GetPrerequisiteIds(TalentDefinition[] previousDepth, int currentIndex, int currentCount)
        {
            if (previousDepth == null || previousDepth.Length == 0)
            {
                return new string[0];
            }

            var ids = new List<string>();
            int previousCount = previousDepth.Length;

            if (previousCount == 1)
            {
                ids.Add(previousDepth[0].Id);
            }
            else if (currentCount == 1)
            {
                for (int i = 0; i < previousDepth.Length; i++)
                {
                    ids.Add(previousDepth[i].Id);
                }
            }
            else if (previousCount == currentCount)
            {
                ids.Add(previousDepth[currentIndex].Id);
            }
            else if (previousCount == 2 && currentCount == 3)
            {
                if (currentIndex <= 1)
                {
                    ids.Add(previousDepth[0].Id);
                }

                if (currentIndex >= 1)
                {
                    ids.Add(previousDepth[1].Id);
                }
            }
            else if (previousCount == 3 && currentCount == 2)
            {
                if (currentIndex == 0)
                {
                    ids.Add(previousDepth[0].Id);
                    ids.Add(previousDepth[1].Id);
                }
                else
                {
                    ids.Add(previousDepth[1].Id);
                    ids.Add(previousDepth[2].Id);
                }
            }
            else
            {
                ids.Add(previousDepth[0].Id);
            }

            return ids.ToArray();
        }

        private static Dictionary<string, TalentDefinition> BuildTalentMap()
        {
            var map = new Dictionary<string, TalentDefinition>();
            for (int i = 0; i < talents.Length; i++)
            {
                map[talents[i].Id] = talents[i];
            }

            return map;
        }

        private static Dictionary<int, List<TalentDefinition>> BuildTalentDepthMap()
        {
            var map = new Dictionary<int, List<TalentDefinition>>();
            for (int i = 0; i < talents.Length; i++)
            {
                TalentDefinition talent = talents[i];
                if (!map.TryGetValue(talent.Tier, out List<TalentDefinition> depthTalents))
                {
                    depthTalents = new List<TalentDefinition>();
                    map[talent.Tier] = depthTalents;
                }

                depthTalents.Add(talent);
            }

            return map;
        }

        private static string GetDepthLabel(int depth)
        {
            return (depth + 1) + "뎁스";
        }

        private static TalentSpec Spec(
            string id,
            string displayName,
            string icon,
            TalentEffectKind effectKind,
            double valuePerLevel)
        {
            return new TalentSpec(id, displayName, icon, effectKind, valuePerLevel);
        }

        private sealed class TalentSpec
        {
            public TalentSpec(
                string id,
                string displayName,
                string icon,
                TalentEffectKind effectKind,
                double valuePerLevel)
            {
                Id = id;
                DisplayName = displayName;
                Icon = icon;
                EffectKind = effectKind;
                ValuePerLevel = valuePerLevel;
            }

            public string Id { get; }
            public string DisplayName { get; }
            public string Icon { get; }
            public TalentEffectKind EffectKind { get; }
            public double ValuePerLevel { get; }
        }
    }
}
