using System.Collections.Generic;

namespace Smartitecture.Application.Models
{
    /// <summary>
    /// Represents a tool that an agent can use to perform actions.
    /// </summary>
    public class AgentTool
    {
        /// <summary>
        /// Gets or sets the unique identifier for the tool.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the tool.
        /// </summary>
        public string Name { get; set; }


        /// <summary>
        /// Gets or sets the description of the tool.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the list of parameters that the tool accepts.
        /// </summary>
        public List<ToolParameter> Parameters { get; set; } = new List<ToolParameter>();

        /// <summary>
        /// Gets or sets a value indicating whether the tool is enabled.
        /// </summary>
        public bool IsEnabled { get; set; } = true;
    }

    /// <summary>
    /// Represents a parameter for an agent tool.
    /// </summary>
    public class ToolParameter
    {
        /// <summary>
        /// Gets or sets the name of the parameter.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the data type of the parameter.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the description of the parameter.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the parameter is required.
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Gets or sets the default value of the parameter.
        /// </summary>
        public object DefaultValue { get; set; }
    }
}
