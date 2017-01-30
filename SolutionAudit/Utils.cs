// Copyright 2017 Cvent, Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;

namespace SolutionAudit
{
    static class Utils
    {
        public static IEnumerable<T> Closure<T>(T root, Func<T, IEnumerable<T>> children)
        {
            var seen = new HashSet<T>();
            var stack = new Stack<T>();
            stack.Push(root);

            while (stack.Count != 0)
            {
                var item = stack.Pop();
                if (!seen.Add(item)) continue;
                yield return item;
                foreach (var child in children(item))
                {
                    stack.Push(child);
                }
            }
        }

        public static IEnumerable<TSource> Duplicates<TSource>(this IEnumerable<TSource> enumerable)
        {
            return enumerable.DuplicatesBy(s => s);
        }

        public static IEnumerable<TSource> DuplicatesBy<TSource, TKey>(this IEnumerable<TSource> enumerable,
            Func<TSource, TKey> keySelector)
        {
            return enumerable.GroupBy(keySelector).SelectMany(g => g.Skip(1));
        }

        public static bool AllEqual<T>(this IEnumerable<T> enumerable)
        {
            return enumerable.Distinct().Count() == 1;
        }
    }

    public abstract class Wrapper<T>
    {
        protected Wrapper(T wrapped)
        {
            Wrapped = wrapped;
        }

        protected T Wrapped { get; set; }
    }

    public interface IVisualStudioCommand
    {
        string VsCommand();
    }
}
