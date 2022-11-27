using System.Collections.Generic;

namespace ObjectSemantics.NET
{
    public interface IObjectSemantics
    {
        /// <summary>
        /// Generate a Data Template From Object Properties
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="record">Single Record of T that may include a Collection inside it</param>
        /// <param name="template">Template File containing Template Name and Template Contents</param>
        /// <param name="additionalKeyValues">Additional Key Value parameters that you may need mapped to file</param>
        /// <returns></returns>
        string GenerateTemplate<T>(T record, ObjectSemanticsTemplate template, List<ObjectSemanticsKeyValue> additionalKeyValues = null) where T : new();

        /// <summary>
        /// Generates a Data Template from Object Properties
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="record">Single Record of T that may include a Collection inside it</param>
        /// <param name="templateFileName">The File Template FileName as saved in Directory specified in Options.</param>
        /// <param name="additionalKeyValues">Additional Key Value parameters that you may need mapped to file</param>
        /// <returns></returns>
        string GenerateTemplate<T>(T record, string templateFileName, List<ObjectSemanticsKeyValue> additionalKeyValues = null) where T : new();
    }
}
