namespace OcapReplaySaver2
{
    /// <summary>
    /// A class representing a command.
    /// </summary>
    public class Command
    {
        /// <summary>
        /// Name of the command
        /// </summary>
        public string CommandName { get; set; }

        /// <summary>
        /// Arguments for the command
        /// </summary>
        public string[] Arguments { get; set; }
    }
}
