using System.Collections.Generic;
using static CellType;

public class RuleSet
{
    // ¿¹: Baba -> {You}, Wall -> {Stop}
    private readonly Dictionary<ObjectType, HashSet<TextType>> _map = new();

    public void Add(ObjectType subject, TextType property)
    {
        if (!_map.TryGetValue(subject, out var set))
        {
            set = new HashSet<TextType>();
            _map.Add(subject, set);
        }
        set.Add(property);
    }

    public bool Has(ObjectType subject, TextType property)
        => _map.TryGetValue(subject, out var set) && set.Contains(property);

    public int RuleCount
    {
        get
        {
            int count = 0;
            foreach (var kv in _map) count += kv.Value.Count;
            return count;
        }
    }

    public IEnumerable<KeyValuePair<ObjectType, HashSet<TextType>>> Pairs => _map;
}