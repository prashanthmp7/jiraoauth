using System.Collections.Generic;

namespace JiraOAuthConnectLib
{
    /// <summary>
    /// Class used for Sorting the QueryParameter class based on parameter name
    /// </summary>
    public class LexicographicComparer : IComparer<QueryParameter>
    {
        public int Compare(QueryParameter x, QueryParameter y)
        {
            if (x.Name == y.Name)
                return string.Compare(x.Value, y.Value);
            else
                return string.Compare(x.Name, y.Name);
        }
    }
}
