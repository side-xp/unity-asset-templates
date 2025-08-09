using System.Collections.Generic;
using System.Text.RegularExpressions;

using UnityEngine;

namespace SideXP.AssetTemplates.EditorOnly
{

    /// <summary>
    /// Utility for creating Regex patterns to detext prefix or suffix in a name.
    /// </summary>
    public class PrefixSuffixPattern
    {

        /// <summary>
        /// Options for matching name parts.
        /// </summary>
        public struct PartOptions
        {

            /// <summary>
            /// Default options used for matching name parts.
            /// </summary>
            public static readonly PartOptions Default = new PartOptions
            {
                FirstLetterCaseInsensitive = true,
                MatchIfNextLetterUppercase = true,
                RemovePart = true
            };

            /// <summary>
            /// If enabled, this prefix or suffix matches whether it's used with a first lower or uppercase letter (other letters must
            /// match your specification though).
            /// </summary>
            public bool FirstLetterCaseInsensitive;

            /// <summary>
            /// For prefixes only. If enabled, this prefix will match event if it's not followed by a space but by an uppercase letter.
            /// </summary>
            public bool MatchIfNextLetterUppercase;

            /// <summary>
            /// If enabled, this prefix or suffix will be removed from the input name if matched.
            /// </summary>
            public bool RemovePart;

        }

        private Dictionary<string, PartOptions> _prefixes = new Dictionary<string, PartOptions>();
        private Dictionary<string, PartOptions> _suffixes = new Dictionary<string, PartOptions>();

        /// <inheritdoc cref="PrefixSuffixPattern"/>
        public PrefixSuffixPattern() { }

        /// <inheritdoc cref="PrefixSuffixPattern"/>
        /// <param name="prefixes"><inheritdoc cref="_prefixes" path="/summary"/></param>
        /// <param name="suffixes"><inheritdoc cref="_suffixes" path="/summary"/></param>
        public PrefixSuffixPattern(string[] prefixes, string[] suffixes)
        {
            foreach (string prefix in prefixes)
                AddPrefix(prefix);
            
            foreach (string suffix in suffixes)
                AddSuffix(suffix);
        }

        /// <inheritdoc cref="PrefixSuffixPattern"/>
        /// <param name="parts">The name parts to match as both prefixes and suffixes.</param>
        public PrefixSuffixPattern(params string[] parts)
            : this (parts, parts) { }

        /// <inheritdoc cref="AddPrefix(string, PartOptions)"/>
        public void AddPrefix(string prefix)
        {
            AddPrefix(prefix, PartOptions.Default);
        }

        /// <summary>
        /// Registers a prefix to match;
        /// </summary>
        /// <param name="prefix">The prefix to match.</param>
        /// <param name="options">The options for making the prefix match.</param>
        public void AddPrefix(string prefix, PartOptions options)
        {
            // Cancel if the prefix has already been registered
            if (_prefixes.ContainsKey(prefix))
                return;

            _prefixes.Add(prefix, options);
        }

        /// <inheritdoc cref="AddSuffix(string, PartOptions)"/>
        public void AddSuffix(string suffix)
        {
            AddSuffix(suffix, PartOptions.Default);
        }

        /// <summary>
        /// Registers a suffix to match;
        /// </summary>
        /// <param name="suffix">The suffix to match.</param>
        /// <param name="options">The options for making the suffix match.</param>
        public void AddSuffix(string suffix, PartOptions options)
        {
            // Cancel if the suffix has already been registered
            if (_suffixes.ContainsKey(suffix))
                return;

            _suffixes.Add(suffix, options);
        }

        /// <inheritdoc cref="Match(string, out string, out string, out bool)"/>
        public bool Match(string name)
        {
            return Match(name, out _, out _, out _);
        }

        /// <summary>
        /// Checks if one of the defiend parts match.
        /// </summary>
        /// <param name="name">The name to match.</param>
        /// <param name="outputName">Outputs the "final" name, after removing the matching part if needed.</param>
        /// <param name="matchingPart">Outputs the part that validated the match.</param>
        /// <param name="isPrefix">Outputs true if the matching part is a prefix, or false if the macthing part is a suffix.</param>
        /// <returns>Returns true if one of the defined parts has matched.</returns>
        public bool Match(string name, out string outputName, out string matchingPart, out bool isPrefix)
        {
            name = name.Trim();

            // Check for prefixes
            foreach ((string part, PartOptions options) in _prefixes)
            {
                // Open group "prefix"
                string pattern = "^(?<prefix>";
                // Add a match for the exact part
                pattern += $"(?:{part})";
                // Add a match for the part with first letter case insensitive if applicable
                if (options.FirstLetterCaseInsensitive)
                {
                    string partWithoutCasing = InverseFirstLetterCasing(part);
                    pattern += $"|(?:{partWithoutCasing})";
                }
                // Close group "prefix"
                pattern += ")";

                // If allowed, add lookahead rule so the prefix matches only if followed by a space character or an uppercase letter
                if (options.MatchIfNextLetterUppercase)
                    pattern += @"(?=\s|[A-Z])";
                // Else, add lookahead rule so the prefix matches only if followed by a space character
                else
                    pattern += @"(?=\s)";

                // Use the built regex
                Regex regex = new Regex(pattern);
                Match match = regex.Match(name);
                // Skip if the prefix doesn't match
                if (!match.Success)
                    continue;

                matchingPart = match.Groups["prefix"].Value.Trim();
                outputName = options.RemovePart
                    ? name.Substring(match.Groups["prefix"].Value.Length)
                    : name;
                outputName.Trim();
                isPrefix = true;
                return true;
            }

            // Check for suffixes
            foreach ((string part, PartOptions options) in _suffixes)
            {
                // Open group "suffix", and non-capturing group to assert the whole pattern at the end
                string pattern = "(?<suffix>(?:";
                // Add a match for the exact part: if the part starts with a lowercase letter, it MUST be preceeded by a space character.
                // If the part starts with an uppercase letter, it CAN be preceeded by a space character
                pattern += $@"(?:\s{(char.IsLower(part[0]) ? @"+" : "*")}{part})";
                // Add a match for the part with first letter case insensitive if applicable
                if (options.FirstLetterCaseInsensitive)
                {
                    string partWithoutCasing = InverseFirstLetterCasing(part);
                    pattern += $@"|(?:\s{(char.IsLower(partWithoutCasing[0]) ? @"+" : "*")}{partWithoutCasing})";
                }
                // Close the non-capturing group and group "suffix"
                // Also asset the pattern at the end position
                pattern += "))$";

                // Use the built regex
                Regex regex = new Regex(pattern);
                Match match = regex.Match(name);
                // Skip if the prefix doesn't match
                if (!match.Success)
                    continue;

                matchingPart = match.Groups["suffix"].Value.Trim();
                outputName = options.RemovePart
                    ? name.Substring(0, match.Index)
                    : name;
                outputName.Trim();
                isPrefix = false;
                return true;
            }

            outputName = name;
            matchingPart = null;
            isPrefix = false;
            return false;
        }

        /// <summary>
        /// Inverse the casing of the first letter of a given string: uppercase letter is changed to lower, and reverse.
        /// </summary>
        /// <param name="str">The string to process.</param>
        /// <returns>Returns the processed string, with its first letter's casing inverted.</returns>
        private string InverseFirstLetterCasing(string str)
        {
            char[] chars = str.ToCharArray();
            if (char.IsUpper(chars[0]))
                chars[0] = char.ToLower(chars[0]);
            else
                chars[0] = char.ToUpper(chars[0]);
            return new string(chars);
        }

    }

}
