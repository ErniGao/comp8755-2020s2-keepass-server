using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

//======================================================================
//
//        filename : ApiFile.cs
//        description : This is a file utility class. 
//                      It is used to read and write data into file system
//        created by Erni Gao at  Nov 2020
//   
//======================================================================

namespace KeePassServer.Network_Util
{
    class ApiFile
    {
        /// <summary>
        /// read all files in a folder
        /// </summary>
        /// <param name="path">path of a folder</param>
        /// <returns>all data in the folder in byte array</returns>
        public static byte[] readAllFiles(string path)
        {
            // read folder name
            byte[] buffer = Encoding.UTF8.GetBytes(Path.GetFileName(path));  
            buffer = ApiArray.addHeader((byte)buffer.Length, buffer); 

            var files = Directory.GetFiles(path);
            int count = 0;
            foreach (var file in files)
            {
                //get the name of the file
                string fileName = Path.GetFileName(file);
                byte[] fileNameByte = System.Text.Encoding.UTF8.GetBytes(fileName);  
                byte[] nameBuffer = ApiArray.addHeader((byte)fileNameByte.Length, fileNameByte);  

                //get content of the file
                byte[] content = readFile(Path.Combine(path, fileName));  
                byte[] fileLen = BitConverter.GetBytes(content.Length); 
                byte[] lengthBuffer = ApiArray.addHeader((byte)fileLen.Length, fileLen); 

                //combine all data in a file
                byte[] oneFile = ApiArray.concatArray(nameBuffer, lengthBuffer);
                oneFile = ApiArray.concatArray(oneFile, content);
                buffer = ApiArray.concatArray(buffer, oneFile);
                count++;

            }

            buffer = ApiArray.addHeader((byte)count, buffer);  

            return buffer;
        }

        /// <summary>
        /// read a file
        /// </summary>
        /// <param name="path">the path of this file</param>
        /// <returns>all data in the file in byte array</returns>
        private static byte[] readFile(string path)
        {
            byte[] buffer;
            using (FileStream fsRead = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                buffer = new byte[fsRead.Length];
                int r = fsRead.Read(buffer, 0, buffer.Length);
            }
            return buffer;
        }

        /// <summary>
        /// write all files in a folder
        /// </summary>
        /// <param name="target">directory that all files will be written to</param>
        /// <param name="buffer">byte array containing info of all files</param>
        public static void writeAllFiles(string target, byte[] buffer)
        {
            //get number of files in this folder
            int fileNum = buffer[0]; 

            //get the lenght of folder name 
            int folderNameLen = buffer[1]; 

            //copy all data except file number and folder name length into another buffer
            byte[] buffer1 = new byte[buffer.Length - 2];
            Array.Copy(buffer, 2, buffer1, 0, buffer1.Length);

            //get folder name and all file data
            byte[] folderNameTemp;  
            byte[] fileData;    
            ApiArray.splitArray(buffer1, folderNameLen, out folderNameTemp, out fileData);

            //get folder name in string
            string folderName = Encoding.UTF8.GetString(folderNameTemp); 
             
            //directory used to save all files
            string dir = getDir(target, folderName); 
            
            for (int i = 1; i <= fileNum; i++)
            {
                // get file name
                int nameLen = fileData[0];
                byte[] fileName = new byte[nameLen];
                Array.Copy(fileData, 1, fileName, 0, nameLen);

                //get the rest of data
                fileData = ApiArray.sliceArray(fileData, nameLen + 1);
               
                //get the length of current file
                int lenIndicator = fileData[0];
                byte[] fileLen = new byte[lenIndicator];
                Array.Copy(fileData, 1, fileLen, 0, lenIndicator);
                int fileLength = BitConverter.ToInt32(fileLen, 0);

                //get the rest of data
                fileData = ApiArray.sliceArray(fileData, lenIndicator + 1);
               
                //get content of this file
                byte[] fileContent;
                byte[] contentLeft;
                ApiArray.splitArray(fileData, fileLength, out fileContent, out contentLeft);
                fileData = contentLeft;
  
                // write one file
                writeFile(dir, System.Text.Encoding.UTF8.GetString(fileName), fileContent);
            }
        }

        /// <summary>
        /// write a file in a directory
        /// </summary>
        /// <param name="dir">file directory</param>
        /// <param name="title">name of the file</param>
        /// <param name="buffer">file content in byte array</param>
        public static void writeFile(string dir, string title, byte[] buffer)
        {
            string path = Path.Combine(dir, title);
            using (FileStream fsWrite = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
            {
                fsWrite.Write(buffer, 0, buffer.Length);
            }
        }

        /// <summary>
        /// get the path of directory based on folder name
        /// </summary>
        /// <param name="baseFolder">name of the folder we need to locate in </param>
        /// <param name="folderName">name of the folder we save files</param>
        /// <returns>folder path</returns>
        public static string getDir(string baseFolder, string folderName)
        {
            string path = Path.Combine(System.Environment.CurrentDirectory, baseFolder, folderName);
            if (Directory.Exists(baseFolder))
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            else
            {
                Directory.CreateDirectory(baseFolder);
                Directory.CreateDirectory(path);
            }
            return path;
        }

        /// <summary>
        /// list all files or folders name under a folder
        /// </summary>
        /// <param name="path">path of a folder</param>
        /// <returns>a list of file/folder names</returns>
        public static DirectoryInfo[] listContents(string path)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(path);
            DirectoryInfo[] contents = dirInfo.GetDirectories();
            return contents;
        }
    }
}
