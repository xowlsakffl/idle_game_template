using System;
using UnityEngine;

namespace IdleGame.Data
{
    public sealed class HeroTranscendOptionDefinition
    {
        public HeroTranscendOptionDefinition(
            string id,
            string heroId,
            HeroTranscendOptionScope scope,
            HeroTranscendGrade grade,
            string description,
            float probabilityWeight)
        {
            Id = id;
            HeroId = heroId ?? string.Empty;
            Scope = scope;
            Grade = grade;
            Description = description ?? string.Empty;
            ProbabilityWeight = Mathf.Max(0.0001f, probabilityWeight);
        }

        public string Id { get; }
        public string HeroId { get; }
        public HeroTranscendOptionScope Scope { get; }
        public HeroTranscendGrade Grade { get; }
        public string Description { get; }
        public float ProbabilityWeight { get; }
        public bool IsExclusive => Scope == HeroTranscendOptionScope.Exclusive;
        public string ScopeLabel => IsExclusive ? "전용" : "공용";
    }

}
