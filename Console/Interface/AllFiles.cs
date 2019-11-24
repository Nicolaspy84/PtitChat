using System;
using System.Collections.Generic;

namespace Interface
{

    /// <summary>
    /// This class takes care of all the files received
    /// </summary>
    public static class AllFiles
    {
        /// <summary>
        /// Holds all the files : the key is the file name (must be a unique identifier)
        /// </summary>
        public static Dictionary<string, File> All = new Dictionary<string, File>();


        /// <summary>
        /// Call this method whenever we receive a new chunk
        /// </summary>
        /// <param name="origin">the origin of the file (the owner)</param>
        /// <param name="filename">the filename with its extension (test.txt)</param>
        /// <param name="nbChunks">number of chunks making this file</param>
        /// <param name="chunkID">the chunk ID of the chunk we received</param>
        /// <param name="chunkData">the chunk data (byte array) of the chunk we received</param>
        /// <returns>true if the file was properly reconstructed</returns>
        public static bool NewChunk(string origin, string filename, long nbChunks, long chunkID, byte[] chunkData)
        {
            lock (All)
            {
                // If we don't have a file for this chunk yet, create a new file object
                if (All.ContainsKey(filename) == false)
                {
                    All.Add(filename, new File(filename, origin, nbChunks));
                }

                // Now we can add the chunk if this file hasn't been reconstructed yet
                if (All[filename].reconstructed == false)
                {
                    return All[filename].NewChunk(chunkID, chunkData);
                }
                else
                {
                    Console.WriteLine("File {0} has already been reconstructed", filename);
                }
            }

            return false;
        }
    }
}

