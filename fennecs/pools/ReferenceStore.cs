﻿namespace fennecs.pools;

public class ReferenceStore(int capacity = 4096)
{
    private readonly Dictionary<Entity, StoredReference<object>> _storage = new(capacity);
    
    public Entity Request<T>(T item) where T : class
    {
        ArgumentNullException.ThrowIfNull(nameof(item));
        
        var identity = Entity.Of(item);

        lock (_storage)
        {
            // Already tracking this item.
            if (_storage.TryGetValue(identity, out var reference))
            {
                if (reference.Item != item)
                {
                    throw new InvalidOperationException($"GetHashCode() collision in {typeof(T)}, causing Identity collision between {item} and {reference.Item} in {reference}.");
                }

                reference.Count++;
                _storage[identity] = reference;
                return identity;
            }

            // First time tracking this item.
            reference = new StoredReference<object>
            {
                Item = item,
                Count = 1,
            };

            _storage[identity] = reference;
            return identity;
        }
    }
    
    public T Get<T>(Entity entity) where T : class
    {
        lock (_storage)
        {
            if (!_storage.TryGetValue(entity, out var reference))
            {
                throw new KeyNotFoundException($"Identity is not tracking an instance of {typeof(T)}.");
            }
            
            return (T) reference.Item;
        }
    }
    
    
    public void Release(Entity entity)
    {
        lock (_storage)
        {
            if (_storage.TryGetValue(entity, out var reference))
            {
                reference.Count--;
                if (reference.Count == 0)
                {
                    _storage.Remove(entity);
                }
                else
                {
                    _storage[entity] = reference;
                }
            }
            else
            {
                throw new KeyNotFoundException($"Identity {entity} is not tracked.");
            }
        }
    }

    internal struct StoredReference<T>
    {
        public required T Item;
        public required int Count;
        public override string ToString() => $"{Item} x{Count}";
    }
}