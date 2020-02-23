using System;
using System.Collections.Generic;
using System.Linq;

namespace Dingil.Core
{
    public class DingilTypes
    {
        public DingilTypes(Dictionary<string, Dictionary<string, Type>> types)
        {
            Types = types;
        }

        public static DingilTypes New(Dictionary<string, Dictionary<string, Type>> types) => new DingilTypes(types);

        protected DingilTypes() { }

        public Dictionary<string, Dictionary<string, Type>> Types { get; set; }

        public IEnumerable<Tuple<string, IEnumerable<Tuple<string, Type>>>> GetTypes()
        {
            return Types.Select(type => new Tuple<string, IEnumerable<Tuple<string, Type>>>(type.Key, type.Value.Select(prop => new Tuple<string, Type>(prop.Key, prop.Value))));
        }
    }
}

