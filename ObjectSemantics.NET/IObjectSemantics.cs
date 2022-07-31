using System.Collections.Generic;

namespace ObjectSemantics.NET
{
    public interface IObjectSemantics
    {
        /// <summary>
        /// Generates a Data Template from Object Properties
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="record">Single Record of T</param>
        /// <param name="templateName">The File Template Name</param>
        /// <param name="additionalKeyValues">Additional Key Value parameters that you may need mapped to file</param>
        /// <returns></returns>
        string GenerateTemplate<T>(T record, string templateName, List<ObjectSemanticsKeyValue> additionalKeyValues = null) where T : new();
        /// <summary>
        /// Generates a Data Template from Collection of Object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="records">Collection of T</param>
        /// <param name="templateName">The File Template Name</param>
        /// <param name="additionalKeyValues">Additional Key Value parameters that you may need mapped to file</param>
        /// <returns></returns>
        string GenerateTemplate<T>(List<T> records, string templateName, List<ObjectSemanticsKeyValue> additionalKeyValues = null) where T : new();
    }
}
