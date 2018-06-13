using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HoLLy.DiscordBot.Parsers.Tokens;

namespace HoLLy.DiscordBot.Parsers
{
    internal class BasicTokenizer : ITokenizer<BasicToken>
    {
        protected virtual char[] WhiteSpace { get; } = { ' ', '\r', '\n', '\t' };
        protected virtual char[] StringBoundaries { get; } = { '"', '\'' };
        protected virtual char[] TokenBoundaries { get; } = { 
            ',', '.', ':', ';',
            '=', '+', '-', '*', '/', '%', '!', '~', '&', '|',  
            '{', '}', '[', ']', '(', ')', '<', '>', 
        };

        public virtual IEnumerable<BasicToken> Tokenize(string input)
        {
            // Loop through string
            for (int i = 0; i < input.Length;) {
                // Skip whitespace
                SkipWhitespace(input, ref i);
                
                // Read token
                if (i < input.Length)
                    yield return ReadToken(input, ref i);
            }
        }

        private BasicToken ReadToken(string input, ref int idx)
        {
            int startIdx = idx;
            char c = input[idx];

            // Is this a string?
            if (IsStringBoundary(c)) {
                idx++;
                return new StringToken(ReadUntilChar(input, ref idx, c), c);
            }

            // Is this a symbol?
            if (IsTokenBoundary(c)) {
                idx++;
                return new SymbolToken(c);
            }

            // Is this a number?
            if (IsNumber(c)) {
                // This will break if numbers are too long :(
                return new NumberToken(ulong.Parse(ReadWhileCondition(input, ref idx, IsNumber)));
            }

            // This is something else, perhaps a keyword
            for (; idx < input.Length; idx++) {
                c = input[idx];
                if (IsWhitespace(c) || IsTokenBoundary(c))
                    return new BasicToken(input.Substring(startIdx, idx-startIdx));
            }
            
            return new BasicToken(input.Substring(startIdx, idx-startIdx));
        }

        private static string ReadUntilChar(string input, ref int idx, char c)
        {
            int startIdx = idx++;
            for (; idx < input.Length; idx++)
                if (input[idx] == c)
                    return input.Substring(startIdx, idx++ - startIdx);

            throw new EndOfStreamException($"Couldn't find string boundary {c}");
        }

        private static string ReadWhileCondition(string input, ref int idx, Func<char, bool> c)
        {
            int startIdx = idx++;
            for (; idx < input.Length; idx++)
                if (!c(input[idx]))
                    return input.Substring(startIdx, idx - startIdx);

            throw new EndOfStreamException($"Condition {c.Method.Name} never met");
        }

        private void SkipWhitespace(string input, ref int idx)
        {
            for (; idx < input.Length; idx++)
                if (!IsWhitespace(input[idx]))
                    return;
        }
        
        protected bool IsNumber(char c) => c >= '0' && c <= '9';
        protected bool IsWhitespace(char c) => WhiteSpace.Contains(c);
        protected bool IsStringBoundary(char c) => StringBoundaries.Contains(c);
        protected bool IsTokenBoundary(char c) => TokenBoundaries.Contains(c) || StringBoundaries.Contains(c);
    }
}
