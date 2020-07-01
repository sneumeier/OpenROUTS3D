
namespace Traci
{
    /// <summary>
    /// Representation of a single TraCI-Result
    /// </summary>
    public class TraciResult
    {
        public int Length { get; set; }
        public byte Identifier { get; set; }
        public byte[] Response { get; set; }
    }
}