using System.Collections.Generic;

namespace SqlParser.Infrastructure.Tokenizing {
    public class TokenMatcher {
        public readonly string TokenToMatch;

        private List<int> _matchedIndices = new List<int>();
        private char[] _previousCharsBuffer;    // Buffer for analyzing token match outcomes
        private int _lastMatchIndex = -1;

        public TokenMatcher(string tokenToMatch) {
            TokenToMatch = tokenToMatch;

            // If token is 2 chars long, we only need to store 1 char of previous characters to be able
            // to evaluate a match given an incoming character; 
            // ergo, _previousCharsBuffer.Length should be: tokenLength - 1
            _previousCharsBuffer = new char[tokenToMatch.Length - 1];
        }

        public bool TryMatchChar(char newChar, int charIndex) {
            var willMatch = willMatchWithNewChar(newChar, charIndex);
            if (willMatch) {
                _matchedIndices.Add(charIndex - (TokenToMatch.Length - 1));
                _lastMatchIndex = charIndex;
            }

            // Pop out head of _previousCharsBuffer by shifting chars to the left
            for (int j = 0; j < _previousCharsBuffer.Length; j++) {

                if (j == _previousCharsBuffer.Length - 1) break;

                _previousCharsBuffer[j] = _previousCharsBuffer[j + 1];
            }

            // Push current char to the last/tail position.
            _previousCharsBuffer[_previousCharsBuffer.Length - 1] = newChar;
            
            return willMatch;
        }


        public IEnumerable<int> GetMatchedIndices() {
            return _matchedIndices;
        }

        #region non-public
        private bool willMatchWithNewChar(char newChar, int newCharIndex) {
            if (_lastMatchIndex > -1) {
                // Ensure non-greedy match.
                // Ex. Given input '---' with token '--', match count should be 1, not 2.
                var isNewCharPartOfLastMatch = newCharIndex - _lastMatchIndex < TokenToMatch.Length;
                if (isNewCharPartOfLastMatch) {
                    return false;
                }
            }

            var previousChars = new string(_previousCharsBuffer);
            return TokenToMatch == previousChars + newChar.ToString();
        }
        #endregion non-public
    }
}
