using System;
using System.Xml.XPath;

namespace Twentythree
{
    public static class Helpers
    {
        public static string GetNodeChildValue(XPathNavigator ANode, string AChildName)
        {
            XPathNodeIterator ContentNode = ANode.SelectChildren(AChildName, "");
            if (ContentNode.MoveNext()) return ContentNode.Current.Value;
            else return null;
        }

        public static int ConvertStringToInteger(string AValue)
        {
            int Result = -1;

            try
            {
                Result = Convert.ToInt32(AValue);
            }
            catch
            {
                return -1;
            }

            return Result;
        }

        public static double ConvertStringToDouble(string AValue)
        {
            double Result = -1;

            try
            {
                Result = Convert.ToDouble(AValue);
            }
            catch
            {
                return -1;
            }

            return Result;
        }
    }
}
