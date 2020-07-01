
namespace Traci
{
    /// <summary>
    /// Representation of a single TraCI-Command
    /// </summary>
    public class TraciCommand
    {
        public byte Identifier { get; set; }
        public byte[] Contents { get; set; }
    }
}