using System;

namespace IdleGame.Data
{
    public sealed class HeroTranscendOptionState
    {
        public HeroTranscendOptionState(string optionId)
        {
            OptionId = optionId ?? string.Empty;
        }

        public string OptionId { get; set; }
    }

}
