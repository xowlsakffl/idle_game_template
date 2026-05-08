using System;

[Serializable]
public sealed class EnemyDefinition
{
    public EnemyDefinition(string id, string displayName, int baseHp, int baseGold)
    {
        Id = id;
        DisplayName = displayName;
        BaseHp = baseHp;
        BaseGold = baseGold;
    }

    public string Id { get; }
    public string DisplayName { get; }
    public int BaseHp { get; }
    public int BaseGold { get; }
}
