using System.Collections.Generic;
using System.Linq;

namespace Nett.Collections
{
    public sealed class WithDepth<T>
    {
        public WithDepth(int depth, T node)
        {
            this.Depth = depth;
            this.Node = node;
        }

        public int Depth { get; }

        public T Node { get; }

        public override string ToString()
            => $"{this.Depth}{this.Node}";
    }

    internal static class TreeOperations
    {
        public static IEnumerable<T> TraverseBreadthFirst<T>(this T root)
            where T : IGetChildren<T>
        {
            var q = new Queue<T>();
            q.Enqueue(root);
            while (q.Count > 0)
            {
                var current = q.Dequeue();
                yield return current;

                foreach (var child in current.GetChildren())
                {
                    q.Enqueue(child);
                }
            }
        }

        public static IEnumerable<T> TraversePreOrder<T>(this T root)
            where T : IGetChildren<T>
        {
            var s = new Stack<T>();
            s.Push(root);

            while (s.Count > 0)
            {
                var current = s.Pop();
                yield return current;

                foreach (var child in current.GetChildren().Reverse())
                {
                    s.Push(child);
                }
            }
        }

        public static IEnumerable<WithDepth<T>> TraversePreOrderWithDepth<T>(this T root)
            where T : IGetChildren<T>
        {
            var result = new List<WithDepth<T>>();
            TraversePreOrderWithDepth(root, result, 0);
            return result;
        }

        private static void TraversePreOrderWithDepth<T>(T current, List<WithDepth<T>> result, int curDepth)
                   where T : IGetChildren<T>
        {
            result.Add(new WithDepth<T>(curDepth, current));

            if (current == null) { return; }

            foreach (var child in current.GetChildren())
            {
                TraversePreOrderWithDepth(child, result, curDepth + 1);
            }
        }
    }
}
