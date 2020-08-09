using System.Collections.Generic;

namespace SqlParser.Infrastructure.Tokenizing2 {
    public class Tokenizer2 {

        private Token2 _currentToken;

        public IEnumerable<Token2> Tokenize(string inputText) {
            var results = new List<Token2>();
            for (int k = 0; k < inputText.Length; k++) {
                if (_currentToken == null) {
                    _currentToken = new Token2();
                }
                if (_currentToken.TryAppend(inputText[k], k, results) && 
                    !_currentToken.IsDone) {
                    continue;
                }
                else {
                    var consumedLastCharSent = _currentToken.IsDone;
                    results.Add(_currentToken.Finalize());
                    _currentToken = new Token2();
                    if (!consumedLastCharSent) {
                        _currentToken.TryAppend(inputText[k], k, results);
                    }
                }
            }

            if (_currentToken != null && !_currentToken.IsEmpty) {
                results.Add(_currentToken.Finalize());
            }

            return results;
        }
    }
}
