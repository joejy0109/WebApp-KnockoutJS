using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace ConsoleApplication5
{
    [XmlRoot("authroize")]
    public class Authorize
    {
        [XmlElement("controller")]
        public List<AuthTargetController> Controllers { get; set; }
    }

    
    public class AuthTargetController
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlElement("auth")]
        public List<Auth> AuthList { get; set; }
    }

    public class Auth
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("serviceName")]
        public string ServiceName { get; set; }

        [XmlAttribute("authKey")]
        public string AuthKey { get; set; }

        [XmlAttribute("allowAnonymous")]
        public bool AllowAnonymous { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            MakeXml();
            //ReadXml();
        }

        static void ReadXml()
        {
            XmlSerializer xsr = new XmlSerializer(typeof(Authorize));
            var ctrl = (Authorize)xsr.Deserialize(XmlReader.Create("authorize.xml"));

        }

        static void MakeXml()
        {
            string path = @"C:\Users\jj19503\Desktop\CiTi MFP\Citi.MyCitigoldFP\trunk\Citi.MyCitigoldFP.Web.Controllers";

            var doc = new XDocument();

            var root = new XElement("authroize");
            foreach (var file in Directory.GetFiles(path).Where(x=>Path.GetExtension(x) == ".cs"))
            {
                string controllerName = null;
                var ctrlMatch = Regex.Match(file, @"([a-zA-Z]+)(?:Controller)");

                if (!ctrlMatch.Success)
                    continue;                    
                controllerName = ctrlMatch.Result("$1");
                
                using (var reader = new StreamReader(file, Encoding.UTF8))
                {
                    var codes = reader.ReadToEnd();

                    var matches = Regex.Matches(codes, @"public\s+(?:async\s+Task\<)?ActionResult(?:\>)?\s+(\w+)\(.*\)", RegexOptions.Multiline | RegexOptions.IgnoreCase);


                    var ctlrItem = new XElement("controller", new XAttribute("name", controllerName));

                    foreach (Match match in matches)
                    {
                        var methodName = match.Groups[1];

                        ctlrItem.Add(new XElement("auth",
                            new XAttribute("name", methodName),
                            new XAttribute("serviceName", methodName),
                            new XAttribute("authKey", methodName + "_auth"),
                            new XAttribute("allowAnonymous", false)
                            )
                        );


                    }
                    root.Add(ctlrItem);
                }
            }
            doc.Add(root);
            using (var writer = XmlWriter.Create("authorize.xml", new XmlWriterSettings { Indent = true }))
            {
                doc.Save(writer);
            }
        }
    }
}
