using System;
using System.Text.RegularExpressions;
using iSynaptic.Commons;

namespace Tardis
{
    public class Space : Node
    {
        private static readonly Regex NamePattern = new Regex(@"^[a-z][a-z0-9]*(\.[a-z][a-z0-9]*)*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public Space(Universe universe, string name) : base(universe)
        {
            Guard.NotNull(universe, "universe");

            if (!NamePattern.IsMatch(name))
                throw new ArgumentException("The name provided is not in the correct format.", "name");


        }
    }
}