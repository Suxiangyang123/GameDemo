// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEditor;
// using System.IO;
// using Excel;
// using System.Data;
// using System;
// using System.Linq;
// public class ExcelToJson : Editor
// {
//     //[MenuItem("TGDTools/ExcelToJson")]
//     public static void ReadExcelToJson()
//     {
//         //目标文件夹
//         string sourceDir = string.Format("{0}/../../excelsconfig", Environment.CurrentDirectory);
//         if(!Directory.Exists(sourceDir))
//         {
//             Debug.LogError(string.Format("目标文件夹不存在！:{0}", sourceDir));
//             return;
//         }
 
//         //检查文件
//         string sourceFile = string.Format("{0}/测试表.xlsx", sourceDir);
//         if (!File.Exists(sourceFile))
//         {
//             Debug.LogError(string.Format("目标文件不存在！:{0}", sourceFile));
//             return;
//         }
//         //读取源文件
//         Dictionary<string, Dictionary<string, object>> holder = new ();
//         if (!ReadExcel(sourceFile, holder))
//         {
//             Debug.LogError(string.Format("读取目标文件失败 : {0}", sourceFile));
//             return;
//         }
//         //检查生成目录
//         string targetDir = string.Format("{0}/Assets/Resources/ExcelToJson", Environment.CurrentDirectory);
//         if (!Directory.Exists(targetDir))
//         {
//             Directory.CreateDirectory(targetDir);
//             Debug.LogFormat("目标文件夹不存在，创建新目录 : {0}", targetDir);
//         }
//         //转换格式并保存
//         string tablePath = string.Format("{0}/TextExcelToJson.json", targetDir);
//         if (File.Exists(tablePath))
//         {
//             File.Delete(tablePath);
//         }
//         FileInfo fileInfo = new FileInfo(tablePath);
//         using (StreamWriter streamWriter = fileInfo.CreateText())
//         {
//             streamWriter.Write(JsonUtility.ToJson(holder));
//             // streamWriter.Write(LitJson.JsonMapper.ToJson(holder));
//             streamWriter.Flush();
//             streamWriter.Close();
//         }
//         AssetDatabase.Refresh();
//         Debug.Log("=================== 生成成功!!!");
//     }
 
//     private static bool ReadExcel(string filePath, Dictionary<string, Dictionary<string, object>> dataHolder)
//     {
//         try
//         {
//             EditorUtility.DisplayProgressBar("ReadExcel", filePath, 0);
//             //读取文件
//             using (FileStream fileStream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
//             {
//                 //创建读取
//                 IExcelDataReader dataReader = ExcelReaderFactory.CreateOpenXmlReader(fileStream);
//                 //转换格式
//                 DataSet dataSet = dataReader.AsDataSet();
//                 int index = 0;
//                 //遍历页签
//                 foreach (DataTable dataTable in dataSet.Tables)
//                 {
//                     //页签名称
//                     string sheetName = dataTable.TableName;
//                     //Debug.LogFormat("------------->> sheetName:{0}", sheetName);
//                     EditorUtility.DisplayProgressBar("Read Excel Sheet", sheetName, index*1f/ dataSet.Tables.Count);
 
//                     index++;
//                     //一个页签
//                     if (!dataHolder.TryGetValue(sheetName, out Dictionary<string, object> sheetData))
//                     {
//                         sheetData = new Dictionary<string, object>();
//                         dataHolder.Add(sheetName, sheetData);
//                     }
//                     //读取行
//                     DataRowCollection dataRows = dataTable.Rows;
//                     //读取列
//                     DataColumnCollection dataColumns = dataTable.Columns;
//                     //Debug.LogFormat("------------->> dataRows.Count:{0}  dataColumns.Count:{1}", dataRows.Count, dataColumns.Count);
 
//                     //遍历
//                     for (int i = 1; i < dataColumns.Count; i++)
//                     {
//                         string name = dataRows[0][i].ToString().Trim();
//                         if (!string.IsNullOrEmpty(name))
//                         {
//                             //Debug.LogFormat("------------->> i:{0}  name:{1}", i, name);
//                             for (int j = 1; j < dataRows.Count; j++)
//                             {
//                                 string key = dataRows[j][0].ToString().Trim();
//                                 //Debug.LogFormat("------------->>  j:{0} key:{1}", j, key);
//                                 //一行数据
//                                 string value = dataRows[j][i].ToString().Trim();
//                                 if (!string.IsNullOrEmpty(value))
//                                 {
//                                     //Debug.LogFormat("------------->>  i:{0} j:{1} value:{2}", i, j, value);
//                                     //dataHolder.Add(name, value);
//                                     if (!sheetData.TryGetValue(key, out object data))
//                                     {
//                                         data = new Dictionary<string, string>();
//                                         sheetData.Add(key, data);
//                                     }
//                                         (data as Dictionary<string, string>).Add(name, value);
//                                 }
//                             }
//                         }
//                     }
//                 }
 
//             }
//             EditorUtility.ClearProgressBar();
//             return true;
//         }
//         catch (Exception e)
//         {
//             Debug.LogError(string.Format("读取数据异常 : {0}", e.Message));
//         }
//         return false;
//     }
 
//     [MenuItem("TGDTools/ExportExcel")]
//     public static void ExportExcel()
//     {
//         //目标文件夹
//         string sourceDir = string.Format("{0}/../../excelsconfig", Environment.CurrentDirectory);
//         if (!Directory.Exists(sourceDir))
//         {
//             Debug.LogError(string.Format("目标文件夹不存在！:{0}", sourceDir));
//             return;
//         }
//         List<string> fileList = RecursivePathGetFiles(sourceDir);
//         for(int i=0; i<fileList.Count; i++)
//         {
//             string fileName = fileList[i];
            
//             EditorUtility.DisplayProgressBar("ReadExcel", fileName, i*1f/fileList.Count);
//             //检查文件
//             if (!File.Exists(fileName))
//             {
//                 Debug.LogError(string.Format("目标文件不存在！:{0}", fileName));
//                 continue;
//             }
//             ReadExcel(fileName);
//         }
//         EditorUtility.ClearProgressBar();
//         AssetDatabase.Refresh();
//         Debug.Log("=================== 生成成功!!!");
//     }
 
//     private static void ReadExcel(string fileName)
//     {
//         try
//         {
//             Debug.LogFormat("------------->>ReadExcel  {0}", fileName);
//             //读取文件
//             using (FileStream fileStream = File.Open(fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
//             {
//                 //创建读取
//                 IExcelDataReader dataReader = ExcelReaderFactory.CreateOpenXmlReader(fileStream);
//                 //转换格式
//                 DataSet dataSet = dataReader.AsDataSet();
//                 //读取表（第一个页签）
//                 DataTable dataTable = dataSet.Tables[0];
//                 CreateClasssFile(dataTable);
//                 CreateJsonFile(dataTable);
//             }
//         }
//         catch (Exception e)
//         {
//             Debug.LogError(string.Format("读取数据异常 : {0}", e.Message));
//         }
//     }
 
//     private static void CreateClasssFile(DataTable dataTable)
//     {
//         //页签名称
//         string sheetName = dataTable.TableName;
//         //读取行
//         DataRowCollection dataRows = dataTable.Rows;
//         //读取列
//         DataColumnCollection dataColumns = dataTable.Columns;
 
 
//         string text = "using System;\nusing System.Collections.Generic;\nnamespace TGDFramework.Core\n{\n    public class " + sheetName + ("\n    {\n");
//         for (int i=0; i<dataColumns.Count; i++)
//         {
//             string valeName = dataRows[1][i].ToString().Trim();
//             string valeType = dataRows[2][i].ToString().Trim();
//             if (!string.IsNullOrEmpty(valeName) && !string.IsNullOrEmpty(valeType))
//             {
//                 string memberStr = string.Format("        public {0} {1};\n", valeType, valeName);
//                 Debug.LogFormat("----------------CreateClasssFile------------->>>  {0} {1}", valeType, valeName);
//                 text = String.Concat(text, memberStr);
//             }
//         }
//         string tail = "    }\n}\n";
//         text = String.Concat(text, tail);
 
//         string dir = string.Format("{0}/Assets/Scripts/TGDFramework/ExcelToJson/Table", Environment.CurrentDirectory);
//         if (!Directory.Exists(dir))
//         {
//             Directory.CreateDirectory(dir);
//         }
 
//         string filePath = string.Format("{0}/{1}.cs", dir, sheetName);
//         if (File.Exists(filePath))
//         {
//             File.Delete(filePath);
//         }
//         FileInfo fileInfo = new FileInfo(filePath);
//         using (StreamWriter streamWriter = fileInfo.CreateText())
//         {
//             streamWriter.Write(text);
//             streamWriter.Flush();
//             streamWriter.Close();
//         }
//         Debug.LogFormat("----------------------------->>>create Class {0} succ!", sheetName);
//     }
 
//     private static void CreateJsonFile(DataTable dataTable)
//     {
//         //页签名称
//         string sheetName = dataTable.TableName;
//         //读取行
//         DataRowCollection dataRows = dataTable.Rows;
//         //读取列
//         DataColumnCollection dataColumns = dataTable.Columns;
 
//         List<object> dataHolder = new List<object>();
//         //遍历
//         for (int i = 3; i < dataRows.Count; i++)
//         {
//             string keyStr = dataRows[i][0].ToString().Trim();
//             if (string.IsNullOrEmpty(keyStr))
//             {
//                 break;
//             }
//             int key = int.Parse(keyStr);            
//             //Debug.LogFormat("----------------------------->>> key {0}", key);
//             Dictionary<object, object> data = new Dictionary<object, object>();
 
//             for (int j = 0; j < dataColumns.Count; j++)
//             {                
//                 string valeName = dataRows[1][j].ToString().Trim();
//                 string valeType = dataRows[2][j].ToString().Trim();
 
//                 string value = dataRows[i][j].ToString().Trim();
//                 //Debug.LogFormat("----------------------------->>> valeType:{0} valeName:{1} value:{2}", valeType, valeName, value);
//                 if (!string.IsNullOrEmpty(value))
//                 {
//                     if (valeType.Equals("int"))
//                     {
//                         data.Add(valeName, int.Parse(value));
//                     }
//                     else if (valeType.Equals("string"))
//                     {
//                         data.Add(valeName, value);
//                     }
//                     else if (valeType.Equals("List<int>"))
//                     {
//                         List<int> list = new List<int>();
//                         string[] listStr = value.Split('+');
//                         foreach(string str in listStr)
//                         {
//                             list.Add(int.Parse(str));
//                         }
//                         data.Add(valeName, list);
//                     }
//                     else if (valeType.Equals("List<string>"))
//                     {
                        
//                         string[] listStr = value.Split('+');
//                         List<string> list = new List<string>(listStr);
//                         data.Add(valeName, list);
//                     }
//                     else if (valeType.Equals("Dictionary<string,string>"))
//                     {
//                         string[] listStr = value.Split('|');
//                         Dictionary<string, string> dic = listStr.ToDictionary(
//                                                                             sKey => { /*Debug.Log(sKey);*/ return sKey.Split('+')[0]; }, 
//                                                                             sElement => { /*Debug.Log(sElement);*/return sElement.Split('+')[1]; }
//                                                                             );
//                         data.Add(valeName, dic);
//                     }
//                 }                              
//             }
//             dataHolder.Add(data);
//         }
//         string dir = string.Format("{0}/Assets/Scripts/TGDFramework/ExcelToJson/Json", Environment.CurrentDirectory);
//         if (!Directory.Exists(dir))
//         {
//             Directory.CreateDirectory(dir);
//         }
//         string filePath = string.Format("{0}/{1}.json", dir, sheetName);
//         if (File.Exists(filePath))
//         {
//             File.Delete(filePath);
//         }
//         FileInfo fileInfo = new FileInfo(filePath);
//         using (StreamWriter streamWriter = fileInfo.CreateText())
//         {
//             streamWriter.Write(JsonUtility.ToJson(dataHolder));
//             // streamWriter.Write(LitJson.JsonMapper.ToJson(dataHolder));
//             streamWriter.Flush();
//             streamWriter.Close();
//         }
//         Debug.LogFormat("----------------------------->>>create Json {0} succ!", sheetName);
//     }
 
//     public static List<string> RecursivePathGetFiles(string path)
//     {
//         List<string> fileList = new List<string>();
//         fileList.Clear();
//         RecursivePath(path, fileList);
//         return fileList;
//     }
 
//     private static void RecursivePath(string path, List<string> fileList)
//     {
//         string[] files = Directory.GetFiles(path);
//         string[] dirs = Directory.GetDirectories(path);
 
//         int count = files.Length;
//         for (int i = 0; i < count; i++)
//         {
//             string ext = Path.GetExtension(files[i]);
//             if (ext.Equals(".mat"))
//             {
//                 continue;
//             }
//             fileList.Add(files[i].Replace("\\", "/"));
//         }
 
//         count = dirs.Length;
//         for (int i = 0; i < count; i++)
//         {
//             RecursivePath(dirs[i], fileList);
//         }
//     }
// }