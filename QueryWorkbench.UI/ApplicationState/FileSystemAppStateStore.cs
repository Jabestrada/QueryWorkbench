using QueryWorkbenchUI.Configuration;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace QueryWorkbenchUI.ApplicationState {
    public class FileSystemAppStateStore : IAppStateStore {
        public AppState LoadAppState(string filename) {
            if (!File.Exists(filename)) {
                return null;
            }
            using (StreamReader sw = new StreamReader(filename)) {
                var reader = new XmlTextReader(sw);
                var deserializer = new DataContractSerializer(typeof(AppState));
                var result = deserializer.ReadObject(reader);
                return (AppState)result;
            }
        }

        public void SaveAppState(string filename, AppState appState) {
            var serializer = new DataContractSerializer(typeof(AppState));
            string xmlString;
            using (var sw = new StringWriter())
            using (var writer = new XmlTextWriter(sw)) {
                writer.Formatting = Formatting.Indented;
                serializer.WriteObject(writer, appState);
                writer.Flush();
                xmlString = sw.ToString();
            }

            using (StreamWriter sw = new StreamWriter(filename)) {
                sw.Write(xmlString);
                sw.Flush();
                sw.Close();
            }
        }
    }
}
