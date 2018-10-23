using System;
using System.Collections.Generic;

namespace HoLLy.DiscordBot.Commands.DependencyInjection
{
    internal class DependencyContainer
    {
        private readonly Dictionary<Type, object> _cache = new Dictionary<Type, object>();

        public void Cache<T>(T obj) => _cache.Add(typeof(T), obj);

        public T Get<T>() => (T)GetOrThrow(typeof(T));
        public object Get(Type t) => GetOrThrow(t);

        private object GetOrThrow(Type t)
        {
            if (!_cache.TryGetValue(t, out object idk))
                throw new Exception($"Type {t} was not cached");
            return idk;
        }
    }
}
