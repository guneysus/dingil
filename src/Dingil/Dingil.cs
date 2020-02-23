using System;
using System.Collections.Generic;

namespace Dingil.Core
{
    /// <summary>
    /// Dynamic Generator IL
    /// </summary>
    public class Dingil
    {
        public static Type PlainTypeGenerator(string name, Dictionary<string, Type> props)
        {
            throw new NotImplementedException();
        }
        public static IEnumerable<Type> PlainTypeGenerator(DingilTypes types)
        {
            foreach (var item in types.Types)
            {
                yield return PlainTypeGenerator(item.Key, item.Value);
            }
        }

        protected Dingil() { }


    }


}
