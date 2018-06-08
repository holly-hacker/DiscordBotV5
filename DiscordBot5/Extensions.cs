using System.Collections.Generic;
using System.Text;

namespace HoLLy.DiscordBot
{
    internal static class Extensions
    {
        /// <summary>
        /// A safer version of <code>IEnumerable&lt;String&gt;.Aggregate</code>, that's faster and doesn't crash as easily.
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static string SafeAggregate(this IEnumerable<string> src)
        {
            if (src == null) return null;

            var sb = new StringBuilder();
            foreach (string s in src)
                sb.Append(s);
            return sb.ToString();
        }
    }
}
