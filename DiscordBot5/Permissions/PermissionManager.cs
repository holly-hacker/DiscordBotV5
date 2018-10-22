using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Discord.WebSocket;
using HoLLy.DiscordBot.Parsers;
using HoLLy.DiscordBot.Parsers.Tokens;
using HoLLy.DiscordBot.Permissions.Conditions.Servers;
using HoLLy.DiscordBot.Permissions.Conditions.Users;

namespace HoLLy.DiscordBot.Permissions
{
    public class PermissionManager
    {
        private Dictionary<IServerCondition, Dictionary<IUserCondition, int>> _conditions;

        internal void Read(string file)
        {
            _conditions = new Dictionary<IServerCondition, Dictionary<IUserCondition, int>>();

            var tokenizer = new BasicTokenizer();
            BasicToken[] array = tokenizer.Tokenize(File.ReadAllText(file)).ToArray();
            for (int i = 0; i < array.Length; i++) {
                var sToken = array[i];

                // Assuming the first token is always a server condition
                IServerCondition sc;
                switch (sToken.TextValue.ToUpperInvariant()) {
                    case "DEFAULTSERVER":
                    case "DEFAULT": // Let's be lenient here
                        sc = new DefaultServerCondition();
                        break;
                    case "SERVERID": {
                        // Expected parameter is '[012345798]'
                        var b1 = array[++i];
                        var id = array[++i];
                        var b2 = array[++i];

                        if (b1 is SymbolToken s1 && b2 is SymbolToken s2 && s1.Value == '[' && s2.Value == ']' && id is NumberToken nt) {
                            sc = new ServerIdCondition(nt.Value);
                        } else throw new Exception("Error while parsing SERVERID parameter.");
                        break;
                    }
                    default:
                        throw new Exception("Unexpected server condition: " + sToken.TextValue);
                }

                // We have the server condition, create a list of user conditions for it
                _conditions[sc] = new Dictionary<IUserCondition, int>();

                if (array[++i].TextValue != "{")
                    throw new Exception("Expected '{' instead of " + array[i]);

                // Now read the user permission lines
                BasicToken cToken;
                while (!((cToken = array[++i]) is SymbolToken)) {
                    // This token is not a symbol, so it should be a user condition

                    IUserCondition uc;
                    switch (cToken.TextValue.ToUpperInvariant()) {
                        case "ROLE": {
                            // Expected parameter is '[BlaBlaBla]'
                            var b1 = array[++i];
                            var role = array[++i];
                            var b2 = array[++i];

                            if (b1 is SymbolToken s1 && b2 is SymbolToken s2 && s1.Value == '[' && s2.Value == ']') {
                                uc = new UserRoleCondition(role.TextValue);
                            } else throw new Exception("Error while parsing ROLE parameter.");
                            break;
                        }
                        case "USERID": {
                            // Expected parameter is '[012345798]'
                            var b1 = array[++i];
                            var id = array[++i];
                            var b2 = array[++i];

                            if (b1 is SymbolToken s1 && b2 is SymbolToken s2 && s1.Value == '[' && s2.Value == ']' && id is NumberToken nt) {
                                uc = new UserIdCondition(nt.Value);
                            } else throw new Exception("Error while parsing USERID parameter.");
                            break;
                        }
                        case "SERVEROWNER":
                            uc = new UserServerOwnerCondition();
                            break;
                        case "DEFAULT":
                            uc = new DefaultUserCondition();
                            break;
                        default:
                            throw new Exception("Unexpected user condition: " + sToken.TextValue);
                    }

                    // Read the permission value
                    if (array[++i].TextValue != ":")
                        throw new Exception("Expected ':' instead of " + array[i]);

                    // Read an int, but it can be prefixed by a minus sign!
                    int perm = 0;
                    var x = array[++i];
                    if (x is NumberToken number) {
                        if (number.Value > int.MaxValue)
                            throw new Exception($"permission number {number.Value} out of bounds for type {typeof(int)}");
                        perm = (int)number.Value;
                    } else if (x is SymbolToken symbol) {
                        if (symbol.Value != '-')
                            throw new Exception($"Unexpected symbol '{symbol.Value}', expected '-' or a number");

                        var nextToken = array[++i];
                        if (nextToken is NumberToken numberNeg) {
                            if (numberNeg.Value > int.MaxValue)
                                throw new Exception($"permission number {numberNeg.Value} out of bounds for type {typeof(int)}");
                            perm = -(int)numberNeg.Value;
                        }
                    } else
                        throw new Exception($"Unexpected token type {x.GetType()} (value={x.TextValue}), expected number or '-'");

                    // We have the user condition, add it to the list
                    _conditions[sc][uc] = perm;
                }

                // All permissions are read now!
            }
        }

        public int GetPermissionLevel(SocketMessage message)
        {
            // Get all matching servers conditions
            var serverNonDefault = _conditions
                .Where(s => s.Key.Match(message.Channel))
                .Where(s => !(s.Key is DefaultServerCondition))
                .SelectMany(s => s.Value)
                .ToArray();

            if (serverNonDefault.Any()) {
                // Get all matching conditions
                var user = serverNonDefault
                    .Where(u => u.Key.Match(message.Author, message.Channel))
                    .ToArray();
                var userNonDefault = user
                    .Where(u => !(u.Key is DefaultUserCondition))
                    .ToArray();

                // Try non-default matches
                if (userNonDefault.Any())
                    return userNonDefault.Max(x => x.Value);

                // Fall back to default match
                if (user.Any())
                    return user.Max(x => x.Value);
            }

            if (_conditions.Any(s => s.Key is DefaultServerCondition)) {
                var defServer = _conditions.Single(s => s.Key is DefaultServerCondition).Value;

                // Get all matching conditions
                var user = defServer
                    .Where(u => u.Key.Match(message.Author, message.Channel))
                    .ToArray();
                var userNonDefault = user
                    .Where(u => !(u.Key is DefaultUserCondition))
                    .ToArray();

                // Try non-default matches
                if (userNonDefault.Any())
                    return userNonDefault.Max(x => x.Value);

                // Fall back to default match
                if (user.Any())
                    return user.Max(x => x.Value);
            }

            // Could return int32.min here
            throw new Exception("No permission for this user!");
        }
    }
}
