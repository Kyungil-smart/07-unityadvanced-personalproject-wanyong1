using System.Collections.Generic;
using static CellType;

public class RuleSet
{
    // ¿¹: Baba -> {You}, Wall -> {Stop}
    private readonly Dictionary<ObjectType, HashSet<TextType>> _map = new();

    // Object IS Object : Baba -> Wall
    private readonly Dictionary<ObjectType, ObjectType> _transforms = new();

    public void Add(ObjectType subject, TextType property)
    {
        if (!_map.TryGetValue(subject, out var set))
        {
            set = new HashSet<TextType>();
            _map.Add(subject, set);
        }
        set.Add(property);
    }

    public void AddTransform(ObjectType from, ObjectType to)
    {
        if (from == ObjectType.None || to == ObjectType.None) return;
        _transforms[from] = to;
    }

    public bool Has(ObjectType subject, TextType property)
        => _map.TryGetValue(subject, out var set) && set.Contains(property);

    public bool TryGetTransform(ObjectType from, out ObjectType to)
        => _transforms.TryGetValue(from, out to);

    public int RuleCount
    {
        get
        {
            int count = 0;
            foreach (var kv in _map) count += kv.Value.Count;
            count += _transforms.Count;
            return count;
        }
    }

    public IEnumerable<KeyValuePair<ObjectType, HashSet<TextType>>> Pairs => _map;
    public IEnumerable<KeyValuePair<ObjectType, ObjectType>> TransformPairs => _transforms;
}