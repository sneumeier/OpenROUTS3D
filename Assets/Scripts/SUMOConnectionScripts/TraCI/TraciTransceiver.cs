using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Traci
{
    /// <summary>
    /// This script contains the basis functionalities for transmitting/receiving messages
    /// </summary>
    public class TraciTransceiver
    {
        private TraciSumoConnector connector = TraciSumoConnector.Instance;
        private readonly byte[] _receiveBuffer = new byte[32768];

        /// <summary>
        /// Sends a message to SUMO and returns the received answer
        /// </summary>
        /// <param name="command"></param>
        public TraciResult[] SendMessage(TraciCommand command)
        {
            return SendMessage(new[] { command });
        }

        /// <summary>
        /// Sends a message to SUMO and returns the received answer
        /// </summary>
        /// <param name="commands"></param>
        public TraciResult[] SendMessage(IEnumerable<TraciCommand> command)
        {
            // 1. Prepare the message
            var msg = GetMessageBytes(command);

            try
            {
                // 2. Send the message
                connector.Stream.Write(msg, 0, msg.Length);

                // 3. Handle the response
                var bytesRead = connector.Stream.Read(_receiveBuffer, 0, 32768);
                if (bytesRead < 0)
                {
                    // Read returns 0 if the client closes the connection
                    throw new IOException();
                }

                byte[] response = _receiveBuffer.Take(bytesRead).ToArray();
                TraciResult[] result = HandleResponse(response);

                if (result != null)
                {
                    return result;
                }
                else
                {
                    UnityEngine.Debug.Log("Response was null");
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Returns the correct ordered Bytes from an int,
        /// can be used to setup a message to send
        /// </summary>
        /// <param name="i"></param>
        public byte[] GetTraCIBytesFromInt32(int i)
        {
            return BitConverter.GetBytes(i).Reverse().ToArray();
        }

        /// <summary>
        /// Returns the correct ordered Bytes from a string,
        /// can be used to setup a message to send
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public byte[] GetTraCIBytesFromASCIIString(string s)
        {
            var bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes(s.Length).Reverse());
            bytes.AddRange(Encoding.ASCII.GetBytes(s));
            return bytes.ToArray();
        }

        /// <summary>
        /// Returns a correctly interpreted data-type object
        /// </summary>
        /// <param name="type"> type-identifier, see: http://sumo.dlr.de/wiki/TraCI/Protocol#Data_types </param>
        /// <param name="array"> array of bytes to convert </param>
        /// <returns></returns>
        public object GetValueFromTypeAndArray(byte type, byte[] array)
        {
            switch (type)
            {
                case 0x01:
                    {
                        TraciPosition2D pos = new TraciPosition2D();
                        pos.X = BitConverter.ToDouble(array.Take(8).Reverse().ToArray(), 0);
                        pos.Y = BitConverter.ToDouble(array.Skip(8).Take(8).Reverse().ToArray(), 0);
                        return pos;
                    }
                case 0x03:
                    {
                        TraciPosition3D pos = new TraciPosition3D();
                        pos.X = BitConverter.ToDouble(array.Take(8).Reverse().ToArray(), 0);
                        pos.Y = BitConverter.ToDouble(array.Skip(8).Take(8).Reverse().ToArray(), 0);
                        pos.Z = BitConverter.ToDouble(array.Skip(16).Take(8).Reverse().ToArray(), 0);
                        return pos;
                    }
                case 0x07:
                    return array[0];
                case 0x08:
                    return BitConverter.ToChar(array, 0);
                case 0x09:
                    var t1 = array.Take(4).Reverse().ToArray();
                    return BitConverter.ToInt32(t1, 0);
                case 0x0A:
                    var t2 = array.Take(4).Reverse().ToArray();
                    return BitConverter.ToSingle(t2, 0);
                case 0x0B:
                    var t3 = array.Take(8).Reverse().ToArray();
                    return BitConverter.ToDouble(t3, 0);
                case 0x0C:
                    int sLenght = BitConverter.ToInt32(array.Take(4).Reverse().ToArray(),0);
                    return Encoding.ASCII.GetString(array.Skip(4).Take(sLenght).ToArray());
                case 0x0E:
                    {
                        var t5 = array.Take(4).Reverse().ToArray();
                        int stringCount = BitConverter.ToInt32(t5, 0);
                        List<string> result = new List<string>();

                        int skipIndex = 4;
                        for (int i = 1; i <= stringCount; i++)
                        {
                            int sLength = BitConverter.ToInt32(array.Skip(skipIndex).Take(4).Reverse().ToArray(), 0);
                            result.Add(Encoding.ASCII.GetString(array.Skip(skipIndex + 4).Take(sLength).ToArray()));
                            skipIndex = skipIndex + sLength + 4;
                        }
                        return result;
                    }
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Overloaded Method to get a byte array ready for sending
        /// </summary>
        /// <param name="command"> Single TraciCommand </param>
        /// <returns></returns>
        private byte[] GetMessageBytes(TraciCommand command)
        {
            return GetMessageBytes(new[] { command });
        }

        /// <summary>
        /// Overloaded Method to get a byte array ready for sending
        /// </summary>
        /// <param name="commands"> Array of TraciCommands </param>
        /// <returns></returns>
        private byte[] GetMessageBytes(IEnumerable<TraciCommand> commands)
        {
            // List of commands to send
            var commandList = new List<List<byte>>();

            // Inspect the single TraciCommands
            foreach (var c in commands)
            {
                var byteList = new List<byte>();

                // Add the Command-Length
                if (c.Contents == null)
                {
                    // no contents: only Length-Field and Identifier-Field => 2
                    byteList.Add(2); 
                }
                else if ((c.Contents.Length + 2) <= 255)
                {
                    byteList.Add((byte)(c.Contents.Length + 2));
                }
                else
                {
                    byteList.Add(0);
                    byteList.AddRange(BitConverter.GetBytes(c.Contents.Length + 6).Reverse());
                }

                // Add the Command-Identifier
                byteList.Add(c.Identifier);

                // Add the Command-Contents
                if (c.Contents != null)
                {
                    byteList.AddRange(c.Contents);
                }
                commandList.Add(byteList);
            }

            // Determine the total Message-Length, includes the header => 4
            var totalLength = commandList.Select(x => x.Count).Sum() + 4;

            // Setup the total message
            var totalMessage = new List<byte>();
            totalMessage.AddRange(BitConverter.GetBytes(totalLength).Reverse()); // Add totalLength
            commandList.ForEach(x => totalMessage.AddRange(x));                    // Add all TraciCommands

            return totalMessage.ToArray();
        }

        /// <summary>
        /// Returns the received message,
        /// is called by SendMessage()
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        private TraciResult[] HandleResponse(byte[] response)
        {
            try
            {
                // 1. Get the total Message-Length
                var receivedLength = response.Take(4).Reverse().ToArray();
                var totalLength = BitConverter.ToInt32(receivedLength, 0);

                var i = 4;
                var results = new List<TraciResult>();
                while (i < totalLength)
                {
                    var traciResult = new TraciResult();
                    var j = 0;
                    int len = response[i + j++];

                    // 2. Get the byte-length of the Result
                    traciResult.Length = len - 2; // bytes lenght will be: Command - Command-Length-Field(1) - Identifier(1)
                    if (len == 0)
                    {
                        if (response.Count() > (i + j + 4))
                        {
                            receivedLength = response.Skip(i + j).Take(4).Reverse().ToArray();
                            len = BitConverter.ToInt32(receivedLength, 0);
                            traciResult.Length = len - 6; // bytes lenght will be: Command - Command-Length-Field(1) - int32len(4) - identifier(1)
                            j += 4;
                        }
                        else
                        {
                            break;
                        }
                    }

                    // 3. Get the Command-Identifier
                    traciResult.Identifier = response[i + j++];

                    // 4. Get the Command-Contents
                    var cmd = new List<byte>();
                    while (j < len)
                    {
                        cmd.Add(response[i + j++]);
                    }
                    traciResult.Response = cmd.ToArray();
                    i += j;
                    results.Add(traciResult);
                }

                // 5. Return the results off all commands
                return results.ToArray();
            }
            catch (IndexOutOfRangeException)
            {
                return null;
            }
        }

        /// <summary>
        /// Common Method to send messages to SUMO.
        /// Returns the received message content
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="commandId">To be set as specified in TraciConstants</param>
        /// <param name="objectIds">Set to 'null' if there is no ID or it can be ignored</param>
        /// <param name="variableId">To be set as specified in TraciConstants</param>
        /// <param name="responseId">To be set as specified in TraciConstants</param>
        /// <returns></returns>
        public List<T> getUniversal<T>(byte commandId, List<string> objectIds, byte variableId, byte responseId)
        {
            // 1. Setup the Message
            List<TraciCommand> commandList = new List<TraciCommand>();
            if (objectIds == null) // the message will contain a single command
            {
                byte[] content;
                List<byte> b1 = new List<byte> { variableId };
                List<byte> b2 = new List<byte> { 0, 0, 0, 0 }; // empty String
                b1.AddRange(b2);
                content = b1.ToArray();
                TraciCommand command = new TraciCommand
                {
                    Identifier = commandId,
                    Contents = content
                };
                commandList.Add(command);
            }
            else  // the message will contain multiple commands
            {
                foreach (string s in objectIds)
                {
                    byte[] content;
                    List<byte> b1 = new List<byte> { variableId };
                    b1.AddRange(GetTraCIBytesFromASCIIString(s));
                    content = b1.ToArray();

                    TraciCommand command = new TraciCommand
                    {
                        Identifier = commandId,
                        Contents = content
                    };
                    commandList.Add(command);
                }
            }

            // if there are no commands, return an empty list
            if (commandList.Count == 0) 
            {
                return new List<T>();
            }

            // 2. Send the message to SUMO
            var response = SendMessage(commandList);
            if (response.Length > 0)
            {
                List<T> resultList = new List<T>();

                foreach (var r in response)
                {
                    if (r.Identifier == responseId &&
                        r.Response[0] == variableId)
                    {
                        var i = r.Response.Skip(1).Take(4).Reverse().ToArray();
                        var idLength = BitConverter.ToInt32(i, 0);
                        var type = r.Response[5 + idLength];

                        // 0x0E -> type: stringList
                        if (type == 0x0E)
                        {
                            resultList = (List<T>) GetValueFromTypeAndArray(type, r.Response.Skip(6 + idLength).ToArray());
                        }
                        else
                        {
                            T result = (T)GetValueFromTypeAndArray(type, r.Response.Skip(6 + idLength).ToArray());
                            resultList.Add(result);
                        }
                    }
                }
                return resultList;
            }
            else
            {
                UnityEngine.Debug.LogError("Wrong response in getUniversal");
                UnityEngine.Debug.LogError("Stack: " + new System.Diagnostics.StackTrace());
                return null;
            }
        }
    }
}
