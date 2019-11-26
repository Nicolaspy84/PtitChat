using System;
using System.Collections.Generic;
using System.IO;

namespace PtitChat
{

    /// <summary>
    /// Class used to reconstruct files transmitted by network
    /// </summary>
    public class File
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="_filename">the filename (with its extension)</param>
        /// <param name="_origin">the origin of the file (the user who is sending it to us)</param>
        /// <param name="_nbChunks">total number of chunks making this file</param>
        public File(string _filename, string _origin, long _nbChunks)
        {
            filename = _filename;
            origin = _origin;
            nbChunks = _nbChunks;
            chunks = new Dictionary<long, Chunk>();
        }


        /// <summary>
        /// Filename with extension
        /// </summary>
        public readonly string filename;


        /// <summary>
        /// Origin of the file (who sent it)
        /// </summary>
        public readonly string origin;


        /// <summary>
        /// Total number of chunks making this file
        /// </summary>
        public readonly long nbChunks;


        /// <summary>
        /// True if this file has already been reconstructed
        /// </summary>
        public bool reconstructed;


        /// <summary>
        /// Array of chunks
        /// </summary>
        private Dictionary<long, Chunk> chunks;


        /// <summary>
        /// Call this method whenever a new chunk arrives for this file.
        /// This method will reconstruct the file if we received
        /// </summary>
        /// <param name="chunkID">the chunk ID</param>
        /// <param name="chunkData">the byte array</param>
        /// <returns>true if the file was reconstructed properly and saved</returns>
        public bool NewChunk(long chunkID, byte[] chunkData)
        {
            // If we don't have this chunk yet
            if (chunks.ContainsKey(chunkID) == false)
            {
                // Add it to our existing chunks
                chunks.Add(chunkID, new Chunk(chunkID, chunkData));

                // Test to try and reconstruct the file
                return ReconstructFile();
            }

            return false;
        }


        /// <summary>
        /// Try to reconstruct the file
        /// </summary>
        /// <param name="destination">folder destination</param>
        /// <returns>true if the file was reconstructed properly</returns>
        public bool ReconstructFile(string destination = "downloads/")
        {
            // For every chunk needed
            for (long i = 0; i < nbChunks; i++)
            {
                // If we don't have this chunk yet, return false
                if (chunks.ContainsKey(i) == false)
                {
                    return false;
                }
            }

            // Try to write the file
            try
            {
                // Trying to write file
                Console.WriteLine("Writing file {0}", destination + filename);

                // Check if the destination folder exists
                Directory.CreateDirectory(destination);

                // Delete the file if it exists
                if (System.IO.File.Exists(destination + filename))
                {
                    System.IO.File.Delete(destination + filename);
                }

                // If we arrive here it means we can safely reconstruct the file, so we loop again
                FileStream fileStream = new FileStream(destination + filename, FileMode.Create);

                // Write the data to the file, byte by byte
                for (long i = 0; i < nbChunks; i++)
                {
                    for (int j = 0; j < chunks[i].chunkData.Length; j++)
                    {
                        fileStream.WriteByte(chunks[i].chunkData[j]);
                    }
                }

                // Close the file stream
                fileStream.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

            // Set reconstructed to true and clear our dictionary
            chunks.Clear();
            reconstructed = true;
            return true;
        }


        /// <summary>
        /// Nested class used to represent file chunks
        /// </summary>
        private class Chunk
        {
            /// <summary>
            /// Default constructor for chunks
            /// </summary>
            /// <param name="_chunkID">the unique chunk ID</param>
            /// <param name="_chunkData">the byte array containing the chunk data</param>
            public Chunk(long _chunkID, byte[] _chunkData)
            {
                chunkID = _chunkID;
                chunkData = _chunkData;
            }


            /// <summary>
            /// Chunk unique ID
            /// </summary>
            public readonly long chunkID;


            /// <summary>
            /// Byte array to contain chunk data
            /// </summary>
            public readonly byte[] chunkData;
        }
    }
}
