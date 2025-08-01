using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Test1.Entities;

namespace Test1
{
    internal class Program
    {
        public static string GetDoneFilePath(string path)
        {
            var fileExtension = Path.GetExtension(path);
            var fileName = Path.GetFileName(path).Replace(fileExtension, "");

            return path.Replace(fileName, String.Concat(fileName, "Done"));
        }

        public static XmlDocument LoadXmlDocument(string path)
        {
            var document = new XmlDocument();
            try
            {
                var xmlReader = XmlReader.Create(path);
                document.Load(xmlReader);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }

            return document;
        }


        public static List<string> GetTouristsTextInfoFromXmlDocument(string path)
        {
            var document = LoadXmlDocument(path);

            var touristsAsTextInfoList = new List<string>();
            var touristsAsNodeList = document.GetElementsByTagName("tourists");
            foreach (XmlNode node in touristsAsNodeList)
            {
                touristsAsTextInfoList.Add(node.InnerText);
            }

            return touristsAsTextInfoList;
        }

        public static TouristGroup ParseTouristGroupFromText(string text)
        {
            var touristsInfo = text.Split('|', StringSplitOptions.RemoveEmptyEntries);
            var tourists = new TouristGroup();

            foreach (var touristInfo in touristsInfo)
            {
                var touristInfoArray = touristInfo.Split('/', StringSplitOptions.RemoveEmptyEntries);

                Tourist? tourist = null;
                switch (touristInfoArray[0])
                {
                    case "10":
                        tourist = new Adult();
                        break;
                    case "8":
                        tourist = new Child { BirthDate = DateTime.Parse(touristInfoArray[1]) };
                        break;
                    case "7":
                        tourist = new Infant { BirthDate = DateTime.Parse(touristInfoArray[1]) };
                        break;
                }

                if (tourist is null) continue;

                tourist.Surname = touristInfoArray[2];
                tourist.Name = touristInfoArray[3];
                tourist.Prefix = touristInfoArray[4];

                tourists.Add(tourist);
            }

            return tourists;
        }

        public static void WriteTouristInfoToXml(string path, TouristInfo touristsInfo)
        {
            var writer = new StreamWriter(path);
            var serializer = new XmlSerializer(typeof(TouristInfo));
            serializer.Serialize(writer, touristsInfo);
        }

        public static bool TouristsFromTextToXml(string path)
        {
            Console.WriteLine("Getting tourists info from text to xml document");
            path = Path.Combine(Directory.GetCurrentDirectory(), path);
            List<string> touristsTextList = new();

            try
            {
                touristsTextList = GetTouristsTextInfoFromXmlDocument(path);
                var allTourists = new List<TouristGroup>();

                foreach (string touristsTextInfo in touristsTextList)
                {
                    var touristsGroup = ParseTouristGroupFromText(touristsTextInfo);
                    allTourists.Add(touristsGroup);
                }
                var touristsInfo = new TouristInfo { TouristGroups = allTourists };

                path = GetDoneFilePath(path);

                WriteTouristInfoToXml(path, touristsInfo);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Something went wrong: {ex.ToString()}");
                return false;
            }
            Console.WriteLine($"Done. Result saved to file: {path}");
            
            return true;
        }


        public static string Reverse(string s)
        {
            return string.Concat<char>(s.Reverse());
        }

        public static string GetDigitsFromString(string s)
        {
            return string.Concat(s.Where(c => char.IsDigit(c)));
        }

        public static string RemoveDigitsFromString(string s)
        {
            return string.Concat(s.Where(c => !char.IsDigit(c)));
        }

        public static int CountWhitespaces(string s)
        {
            return s.Count(c => char.IsWhiteSpace(c));
        }

        /// <summary>
        /// Process 4 strings:
        /// 1. Reverse string;
        /// 2. Get all digits from string;
        /// 3. Remove all digits from string;
        /// 4. Count whitespaces in the string.
        /// </summary>
        /// <param name="path">Path to strings source Xml file</param>
        /// <returns>true, if all strings proccessed, false - overwise</returns>
        public static bool ProcessStringsFromXml(string path)
        {
            Console.WriteLine("Process 4 strings to test Reverse, Get digits, Remove Digits and Count whitespaces");
            var document = LoadXmlDocument(path);

            var resultDocument = new XmlDocument();
            var root = resultDocument.CreateElement("root");

            var strAsNodeList = document.DocumentElement?.ChildNodes;
            if (strAsNodeList is null || strAsNodeList.Count == 0) return false;
            
            foreach (XmlNode strNode in strAsNodeList) 
            {
                string resultStr = "";
                switch (strNode.Name)
                {
                    case "str1":
                        resultStr = Reverse(strNode.InnerText);                        
                        break;
                    case "str2":
                        resultStr = GetDigitsFromString(strNode.InnerText);
                        break;
                    case "str3":
                        resultStr = RemoveDigitsFromString(strNode.InnerText);
                        break;
                    case "str4":
                        resultStr = $"{CountWhitespaces(strNode.InnerText)}";
                        break;
                }

                var newNode = resultDocument.CreateElement(strNode.Name);
                newNode.InnerText = string.Concat(strNode.InnerText, "✇", resultStr);
                root.AppendChild(newNode);
            }

            resultDocument.AppendChild(root);
            path = GetDoneFilePath(path);
            var writer = XmlWriter.Create(path);
            resultDocument.Save(writer);
            Console.WriteLine($"Done. Result save to file: {path}");

            return true;
        }


        public static void RemoveNodeAndMoveChildsUp(XmlNode node)
        {
            var parentNode = node.ParentNode;
            if (parentNode is null) return;

            var childNodes = node.ChildNodes;
            var sibling = node.NextSibling as XmlNode;
            if (sibling is null)
            {
                foreach (XmlNode childNode in childNodes) parentNode.AppendChild(childNode);
            }
            else
            {
                foreach (XmlNode childNode in childNodes) parentNode.InsertBefore(childNode, sibling);
            }
            parentNode.RemoveChild(node);
        }

        public static char[] exceptSymbols = new char[] { '©', '¶', '∑', 'Œ' };
        public static string GetStringWithoutSymbols(string s, char[] chars)
        {
            var builder = new StringBuilder();

            foreach (var symbol in s)
            {
                if (chars.Contains(symbol)) continue;
                builder.Append(symbol);
            }

            return builder.ToString();
        }

        public static string[] removeNodeNames = new string[] { "ContactInfo", "MultimediaDescription", "ImageFormat"};
        /// <summary>
        /// Recursive processing Xml Document:
        /// 1. Removes elements with names specified in removeNodeNames and elements contains 'rds' in names
        /// 2. Moves 'Caption' attribute into child element for his parent
        /// 3. Leaves only one 'Description' node with longest inner text
        /// 4. Removes symbols in 'Description' inner text specified in exceptSymbols
        /// 5. Moves 'DesriptiveText' element into parent attribute.
        /// </summary>
        /// <param name="node"></param>
        public static void ProcessXmlNodes(XmlNode node)
        {
            if (!node.HasChildNodes) return;

            var document = node.OwnerDocument;
            var attributes = node.Attributes;
            var childNodes = node.ChildNodes;            
            int descriptionMaxLength = -1;

            for (int i = 0; i < childNodes.Count; i++)
            {
                var child = childNodes[i];
                if (child is null) continue;

                var childName = child.Name;
                if (childName.Equals("Description"))
                {
                    var currentDescriptionNodeInnerLength = child.InnerText.Length;

                    if (currentDescriptionNodeInnerLength < descriptionMaxLength)
                    {
                        // Remove current Description node as it is not longest
                        node.RemoveChild(child);
                        continue;
                    }
                    else
                    {
                        descriptionMaxLength = currentDescriptionNodeInnerLength;
                        child.InnerText = GetStringWithoutSymbols(child.InnerText, exceptSymbols);

                        // Remove pervious Description node if it exists
                        if (i > 0)
                        {
                            var prevChild = childNodes[i - 1];
                            if (prevChild is null) continue;
                            node.RemoveChild(prevChild);
                        }
                    }
                } 
                else if (childName.Contains("rds") || removeNodeNames.Contains(childName))
                {
                    RemoveNodeAndMoveChildsUp(child);
                    i = i <= 0 ? 0 : i - 1;
                }
                else if (childName.Equals("DescriptiveText") && document is not null)
                {
                    // Move 'DescirptibeText' element into parent element attribute
                    var descriptiveTextAttr = document.CreateAttribute(childName);
                    descriptiveTextAttr.InnerText = child.InnerText;
                    if (attributes is not null)
                    {
                        attributes.Append(descriptiveTextAttr);
                        node.RemoveChild(child);
                        i = i <= 0 ? 0 : i - 1;
                    }
                }

                ProcessXmlNodes(child);
            }

            if (document is null) return;
            if (attributes is null) return;
            for (int i = 0; i < attributes.Count; i++)
            {
                var attribute = attributes[i];
                if (attribute.Name.Equals("Caption"))
                {
                    // Move 'Caption' attribute into parent node
                    var captionNode = document.CreateElement(attribute.Name);
                    captionNode.InnerText = attribute.InnerText;
                    node.AppendChild(captionNode);
                    attributes.Remove(attribute);
                }
            }

        }
        
        public static bool AlterXmlDocument(string path)
        {
            var document = LoadXmlDocument(path);
            var nodeList = document.ChildNodes;

            ProcessXmlNodes(document);

            path = GetDoneFilePath(path);
            var writer = XmlWriter.Create(path);
            document.Save(writer);

            return true;
        }


        static void Main(string[] args)
        {
            if (args.Length > 1)
            {
                string path = Path.Combine(Directory.GetCurrentDirectory(), args[1]);
                switch (args[0])
                {
                    case "-h":
                    case "--help":
                        Console.WriteLine("--tourists <path>\n\tGet tourists list from plain text to xml document");
                        break;
                    case "--tourists":
                        TouristsFromTextToXml(path);
                        break;
                    case "--strings":
                        ProcessStringsFromXml(path);
                        break;
                    case "--process_xml":
                        AlterXmlDocument(path);
                        break;
                }
            }
            else if (args.Length == 1 && new string[] { "-h", "--help" }.Any(el => el.Equals(args[0])))
            {
                Console.WriteLine("--tourists <path>\n\tGet tourists list from plain text to xml document");
                Console.WriteLine("--strings <path>\n\tProcess 4 strings: \n\t1. Reverse string; \n\t2. Get all digits from string; \n\t3. Remove all digits from string; \n\t4. Count whitespaces.");
                Console.WriteLine("--process_xml <path>\n\tRecursive processing Xml Document: \r\n\t1. Removes elements with names specified in removeNodeNames and elements contains 'rds' in names \r\n\t2. Moves 'Caption' attribute into child element for his parent \r\n\t3. Leaves only one 'Description' node with longest inner text \r\n\t4. Removes symbols in 'Description' inner text specified in exceptSymbols \r\n\t5. Moves 'DesriptiveText' element into parent attribute.");
            }
            else Console.WriteLine("Run program with flag '-h' to get some help");
        }
    }
}
