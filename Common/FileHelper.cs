using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace XpimRevitAddIn.Common
{
    /// <summary>
    /// 文件处理基础类
    /// </summary>
    public class FileHelper
    { 
        #region 获取文本文件字符串
        public static string GetTextFromFile(string filePath)
        {
            return GetTextFromFile(filePath, Encoding.UTF8);
        }
        public static string GetTextFromFile(string filePath, Encoding encoding)
        {
            TextReader tr = null;
            try
            {
                tr = new StreamReader(filePath, encoding);
                string fileText = tr.ReadToEnd();
                return fileText;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (tr != null)
                {
                    tr.Close();
                    tr.Dispose();
                    tr = null;
                }
            }
        }
        #endregion

        #region 保存文本文件字符串
        public static void SaveTextToFile(string text, string filePath)
        {
            Encoding encoding = new System.Text.UTF8Encoding(false);
            SaveTextToFile(text, filePath, encoding);
        }
        public static void SaveTextToFile(string text, string filePath, Encoding encoding)
        {
            TextWriter tw = null;
            try
            {
                tw = new StreamWriter(filePath, false, encoding);
                tw.Write(text);
                tw.Flush();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (tw != null)
                {
                    tw.Close();
                    tw.Dispose();
                    tw = null;
                }
            }
        }
        #endregion

        #region 压缩多个文件
        public static void ZipFiles(string[] filePaths, string zipedFilePath)
        {
            ZipOutputStream zipStream = null;
            FileStream zipedFs = null;
            FileStream fs = null;

            try
            {
                zipedFs = File.Create(zipedFilePath);
                zipStream = new ZipOutputStream(zipedFs);
                for (int i = 0; i < filePaths.Length; i++)
                {
                    string fileToZip = filePaths[i];

                    if (!File.Exists(fileToZip))
                    {
                        throw new Exception("不存在的文件. filePath = " + fileToZip);
                    }
                    else
                    {
                        fs = File.OpenRead(fileToZip);
                        byte[] buffer = new byte[fs.Length];
                        fs.Read(buffer, 0, buffer.Length);
                        fs.Close();
                        fs.Dispose();
                        fs = null;
                        ZipEntry ent = new ZipEntry(Path.GetFileName(fileToZip));
                        zipStream.PutNextEntry(ent);
                        zipStream.Write(buffer, 0, buffer.Length);
                    }
                }

                zipStream.Finish();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (zipedFs != null)
                {
                    zipedFs.Close();
                    zipedFs.Dispose();
                }
                if (zipStream != null)
                {
                    zipStream.Close();
                }
                if (fs != null)
                {
                    fs.Close();
                    fs.Dispose();
                }
            }
        }
        #endregion

        #region 压缩文件夹
        public static void ZipDirectory(string dirPath, string zipedFilePath, UseZip64 useZip64)
        {
            ZipOutputStream zipStream = null;

            FileStream zipedFs = null;
            FileStream fs = null;

            try
            {
                zipedFs = File.Create(zipedFilePath);
                zipStream = new ZipOutputStream(zipedFs);
                zipStream.UseZip64 = useZip64;
                DirectoryInfo dirInfo = new DirectoryInfo(dirPath);
                FileInfo[] fileInfos = dirInfo.GetFiles();

                for (int i = 0; i < fileInfos.Length; i++)
                {
                    string fileToZip = fileInfos[i].FullName;

                    fs = File.OpenRead(fileToZip);
                    byte[] buffer = new byte[fs.Length];
                    fs.Read(buffer, 0, buffer.Length);
                    fs.Close();
                    fs.Dispose();
                    fs = null;
                    string name = fileToZip.Substring(dirPath.Length);
                    ZipEntry ent = new ZipEntry(name);
                    zipStream.PutNextEntry(ent);
                    zipStream.Write(buffer, 0, buffer.Length);
                }

                DirectoryInfo[] subDirInfos = dirInfo.GetDirectories();
                for(int i = 0; i < subDirInfos.Length; i++)
                {
                    string subDirPath = subDirInfos[i].FullName;
                    ZipSubDirectory(zipStream, subDirPath, dirPath);
                }

                zipStream.Finish();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (zipedFs != null)
                {
                    zipedFs.Close();
                    zipedFs.Dispose();
                }
                if (zipStream != null)
                {
                    zipStream.Close();
                }
                if (fs != null)
                {
                    fs.Close();
                    fs.Dispose();
                }
            }

        }
        private static void ZipSubDirectory(ZipOutputStream zipStream, string dirPath, string rootDirPath)
        { 
            FileStream fs = null;
            try
            { 
                DirectoryInfo dirInfo = new DirectoryInfo(dirPath);
                FileInfo[] fileInfos = dirInfo.GetFiles();

                for (int i = 0; i < fileInfos.Length; i++)
                {
                    string fileToZip = fileInfos[i].FullName;

                    fs = File.OpenRead(fileToZip);
                    byte[] buffer = new byte[fs.Length];
                    fs.Read(buffer, 0, buffer.Length);
                    fs.Close();
                    fs.Dispose();
                    fs = null;
                    string name = fileToZip.Substring(rootDirPath.Length);
                    ZipEntry ent = new ZipEntry(name);
                    zipStream.PutNextEntry(ent);
                    zipStream.Write(buffer, 0, buffer.Length);
                }

                DirectoryInfo[] subDirInfos = dirInfo.GetDirectories();
                for (int i = 0; i < subDirInfos.Length; i++)
                {
                    string subDirPath = subDirInfos[i].FullName;
                    ZipSubDirectory(zipStream, subDirPath, rootDirPath);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {  
                if (fs != null)
                {
                    fs.Close();
                    fs.Dispose();
                }
            }

        }
        #endregion
    }
}
