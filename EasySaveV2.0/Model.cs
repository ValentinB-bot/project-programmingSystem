using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Resources;
using Newtonsoft.Json;
using System.Xml.Linq;
using System.Threading;

namespace EasySaveV2._0
{
    public class Model
    {
        // Methode too write instide the Log File
        private ViewModel _viewModel;
        private ResourceManager _resourceManager;
        
        public Model(ViewModel viewModel)
        {
            _viewModel = viewModel;
            _resourceManager = new ResourceManager("EasySave.Languages.Strings", typeof(ViewModel).Assembly);
        }

        public int nbFileCopy = 0;

        private void WriteFileForLog(string pathFile, string filePathSource, string filePathDestination, Stopwatch timeCopyPast, Stopwatch timeCryptPast, long sizeFile, string item)
        {
            // Check if the file exists
            if (File.ReadAllText(pathFile).Length == 0)
            {
                // If the file doesn't exist, create a new list with the current element
                List<LogInfo> logInfo = new List<LogInfo>
                {
                    new LogInfo
                    {
                        Name = "Save" + item,
                        SourceFilePath = filePathSource,
                        TargetFilePath = filePathDestination,
                        FileSize = sizeFile,
                        FileTransfertTime = (timeCopyPast.ElapsedMilliseconds).ToString() + " ms",
                        FileCryptageTime = (timeCryptPast.ElapsedMilliseconds).ToString() + " ms",
                        Date = DateTime.Now.ToString("F"),
                    }
                };

                // Convert list to JSON format
                string json = JsonConvert.SerializeObject(logInfo, Formatting.Indented);

                // Save JSON to a file
                File.WriteAllText(pathFile, json);
            }
            else
            {
                // If file exists, load existing content
                string existingJson = File.ReadAllText(pathFile);

                // Unserialize existing JSON into a list
                List<LogInfo> logInfo = JsonConvert.DeserializeObject<List<LogInfo>>(existingJson);

                // Add the new element to the list
                logInfo.Add(new LogInfo
                {
                    Name = "Save" + item,
                    SourceFilePath = filePathSource,
                    TargetFilePath = filePathDestination,
                    FileSize = sizeFile,
                    FileTransfertTime = (timeCopyPast.ElapsedMilliseconds).ToString() + " ms",
                    FileCryptageTime = (timeCryptPast.ElapsedMilliseconds).ToString() + " ms",
                    Date = DateTime.Now.ToString("F"),
                });

                // Convert updated list to JSON format
                string updatedJson = JsonConvert.SerializeObject(logInfo, Formatting.Indented);

                // Save updated JSON to file
                File.WriteAllText(pathFile, updatedJson);
            }
        }

        private void WriteFileForLogXML(string filePath, string filePathSource, string filePathDestination, Stopwatch timeCopyPast, Stopwatch timeCryptPast, long sizeFile, string item)
        {

            // Charger le fichier XML existant en tant qu'objet XDocument
            XDocument documentXml = XDocument.Load(filePath);

            // Créer un nouvel élément et l'ajouter à l'élément racine du document
            XElement nouvelElement = new XElement("Save-"+item,
                new XElement("SourceFilePath", filePathSource),
                new XElement("TargetFilePath", filePathDestination),
                new XElement("FileSize", sizeFile),
                new XElement("FileTransfertTime", (timeCopyPast.ElapsedMilliseconds).ToString() + " ms"),
                new XElement("FileCryptageTime", (timeCryptPast.ElapsedMilliseconds).ToString() + " ms"),
                new XElement("Date", DateTime.Now.ToString("F"))
            );

            documentXml.Root.Add(nouvelElement);

            // Enregistrer les modifications dans le fichier XML existant
            documentXml.Save(filePath);
        }

        // Methode to write instide the RealTimeState File
        private void WriteFileForRealTimeState(string filePath,string item, string filePathSource, string filePathDestination, string active, int totalFilesToCopy, long totalFilesSize, int nbFilesLeftToDo, long fileSizeLeft)
        {
            // Lire le contenu du fichier JSON
            string loadedJson = File.ReadAllText(filePath);

            // Désérialiser le JSON en une liste (ou autre structure de données)
            List<RealTimeStateInfo> loadedSaveInfos = JsonConvert.DeserializeObject<List<RealTimeStateInfo>>(loadedJson);

            // Vérifier si le paragraphe existe
            Console.WriteLine(loadedSaveInfos.Exists(objet => objet.Name == "Save-" + item));
            bool paragrapheExiste = loadedSaveInfos.Exists(objet => objet.Name == "Save-" + item);
            if (paragrapheExiste)
            {
                int i = 0;
                int indexParagraphe = 0;
                foreach (var paragraphe in loadedSaveInfos)
                {
                    if (paragraphe.Name.Equals("Save-" + item))
                    {
                        indexParagraphe = i;
                    }
                    i++;
                }
                // Modify the currently saved part
                loadedSaveInfos[indexParagraphe].SourceFilePath = filePathSource;
                loadedSaveInfos[indexParagraphe].TargetFilePath = filePathDestination;
                loadedSaveInfos[indexParagraphe].State = active;
                loadedSaveInfos[indexParagraphe].TotalFilesToCopy = totalFilesToCopy;
                loadedSaveInfos[indexParagraphe].NbFilesLeftToDo = nbFilesLeftToDo;
                loadedSaveInfos[indexParagraphe].TotalFilesSize = totalFilesSize;
                loadedSaveInfos[indexParagraphe].FileSizeLeft = fileSizeLeft;
                // Convert modified list to JSON format
                string modifiedJson = JsonConvert.SerializeObject(loadedSaveInfos, Formatting.Indented);
                // Save the modified JSON in the same file
                File.WriteAllText(filePath, modifiedJson);
            }
            else
            {
                RealTimeStateInfo newSave = new RealTimeStateInfo
                {
                    Name = "Save-" + item,
                    SourceFilePath = filePathSource,
                    TargetFilePath = filePathDestination,
                    State = active,
                    TotalFilesToCopy = totalFilesToCopy,
                    TotalFilesSize = totalFilesSize,
                    NbFilesLeftToDo = nbFilesLeftToDo,
                    FileSizeLeft = fileSizeLeft
                };

                // Ajouter le nouvel objet à la liste
                loadedSaveInfos.Add(newSave);

                // Sérialiser la liste mise à jour en JSON
                string newContentJson = JsonConvert.SerializeObject(loadedSaveInfos, Formatting.Indented);

                // Écrire le JSON dans le fichier
                File.WriteAllText(filePath, newContentJson);
            }
        }

        // Methode to create all files and folder required
        private void CreateFile(string pathFolder, string typeLog)
        {
            // Checks if the RealTimeState.json file does not yet exist
            if (!File.Exists(pathFolder + "\\RealTimeState.json"))
            {
                string pathFile = Path.Combine(pathFolder, "RealTimeState.json");
                // Create the RealTimeState.json file if it doesn't exist
                using (StreamWriter sw = File.CreateText(pathFile))
                {
                    //Console.WriteLine(_resourceManager.GetString("FileCreatedSuccessfully", CultureInfo.GetCultureInfo(_viewModel.language)));
                    sw.Close();
                }
            }
            // Checks that there is no "Log" folder in the
            if (Directory.GetDirectories(pathFolder, "Log").Length == 0)
            {
                // Create the "Log" folder if it doesn't exist
                Directory.CreateDirectory(pathFolder + "\\Log");
            }
            // Checks if the log file for the current date does not exist and if there is a "Log" folder
            if (typeLog.Equals("Xml"))
            {
                if (!File.Exists(pathFolder + "\\Log\\" + DateTime.Now.ToString("dd-MM-yyyy") + "_Log.xml") && Directory.GetDirectories(pathFolder, "Log").Length != 0)
                {
                    string pathFile = Path.Combine(pathFolder, "Log", DateTime.Now.ToString("dd-MM-yyyy") + "_Log.xml");
                    // Create log file for current date if it doesn't exist
                    // Créer un document XML avec un élément racine
                    XDocument documentXml = new XDocument(
                        new XElement("SaveInfo")
                    );

                    // Enregistrer le document XML dans un fichier
                    documentXml.Save(pathFile);
                }
            }
            else
            {
                if (!File.Exists(pathFolder + "\\Log\\" + DateTime.Now.ToString("dd-MM-yyyy") + "_Log.json") && Directory.GetDirectories(pathFolder, "Log").Length != 0)
                {
                    string pathFile = Path.Combine(pathFolder, "Log", DateTime.Now.ToString("dd-MM-yyyy") + "_Log.json");
                    // Create log file for current date if it doesn't exist
                    using (StreamWriter sw = File.CreateText(pathFile))
                    {
                        //Console.WriteLine(_resourceManager.GetString("FileCreatedSuccessfully", CultureInfo.GetCultureInfo(_viewModel.language)));
                        sw.Close();
                    }
                }
            }
        }

        //// Method for initializing the RealTimeState file
        private void initialiseFileForRealTimeState(string pathFile) 
        {
            // Create a list of RealTimeStateInfo objects with default values
            List<RealTimeStateInfo> realTimeStateInfo = new List<RealTimeStateInfo> { };

            // Convert list to JSON format
            string json = JsonConvert.SerializeObject(realTimeStateInfo, Formatting.Indented);

            // Save JSON to a file
            File.WriteAllText(pathFile, json);
        }

        //Method for converting a string to a char list
        public HashSet<string> ConvertToList(string input,int typelist)
        {
            List<string> result = new List<string>();

            // Split the input string by ";" to get individual elements
            string[] elements = input.Split(';');
            if (typelist == 0)
            {
                foreach (string element in elements)
                {
                    // Check if the input string contains only numbers, ";" and "-"
                    if (!Regex.IsMatch(input, @"^[0-9;-]+$"))
                    {
                        //Console.WriteLine(_resourceManager.GetString("InvalidCharacter", CultureInfo.GetCultureInfo(_viewModel.language)));
                        return new HashSet<string>(result); ; // Return empty list if invalid characters are detected
                    }

                    // Check if the element contains "-"
                    if (element.Contains("-"))
                    {
                        // If it does, split by "-" to get start and end range
                        string[] range = element.Split('-');
                        int start = int.Parse(range[0]);
                        int end = int.Parse(range[1]);

                        // Add all numbers in the range to the result list
                        for (int i = start; i <= end; i++)
                        {
                            result.Add(i.ToString());
                        }
                    }
                    else
                    {
                        // If no range specified, directly add the element to the result list
                        result.Add(element);
                    }

                }
                HashSet<string> hashSet = new HashSet<string>(result);
                return hashSet;
            }
            else if(typelist == 1)
            {
                HashSet<string> hashSet = new HashSet<string>(elements);
                return hashSet;
            }
            return null;
            

            
        }

        // Methode to retrieve the paths to the two required files
        public string[] findFolder()
        {
            string nameSourceFolder = "EasySaveSource";
            string nameSearchTargetFolder = "EasySaveDestination";
            string[] results = new string[2];
            DriveInfo[] drives = DriveInfo.GetDrives();

            foreach (DriveInfo drive in drives)
            {
                if (drive.IsReady)
                {
                    // PC-wide search for folders matching the given name
                    string[] resultsSource = Directory.GetDirectories(drive.RootDirectory.FullName, nameSourceFolder);
                    string[] resultsDestination = Directory.GetDirectories(drive.RootDirectory.FullName, nameSearchTargetFolder);
                    if (resultsSource.Length > 0)
                    {
                        // Displays the path of the first matching folder found
                        results[0] = resultsSource[0];
                    }
                    if (resultsDestination.Length > 0)
                    {
                        // Displays the path of the first matching folder found
                        results[1] = resultsDestination[0];
                    }
                }
            }

            return results;
        }

        //Method to retrieve the total number of files in a folder 
        private int CountFiles(string folder)
        {
            int totalyFilesNumber = 0;

            // Count files in current folder
            string[] filesInFolder = Directory.GetFiles(folder);
            totalyFilesNumber += filesInFolder.Length;

            // Retrieve the list of subfolders
            string[] subFolders = Directory.GetDirectories(folder);

            // Recursively browse each subfolder
            foreach (string subFolder in subFolders)
            {
                totalyFilesNumber += CountFiles(subFolder);
            }

            return totalyFilesNumber;
        }

        //Method for retrieving folder size in bytes
        public long GetDirectorySize(string folder)
        {
            // Create a DirectoryInfo object for the
            DirectoryInfo folderInfo = new DirectoryInfo(folder);

            // Check if the file exists
            if (!folderInfo.Exists)
            {
                //Console.WriteLine(_resourceManager.GetString("FolderDoesNotExist", CultureInfo.GetCultureInfo(_viewModel.language)));
                return 0;
            }

            // Recover all files in the folder and its subfolders
            FileInfo[] files = folderInfo.GetFiles("*.*", SearchOption.AllDirectories);

            // Calculate the total size by adding the size of each file
            long totalySize = 0;
            foreach (FileInfo file in files)
            {
                totalySize += file.Length;
            }

            return totalySize;
        }

        public int GetCountAllFile(HashSet<string> allSave)
        {
            int nbFile = 0;
            string[] PathSourceDestination = findFolder();
            string sourceBasePath = PathSourceDestination[0];
            foreach (string save in allSave)
            {
                nbFile += CountFiles(sourceBasePath+"\\"+save);
            }
            return nbFile;
        }

        public void CopyFolderContent(string input,string typeLog,string extensionCrypt,string processWrok)
        {
            string[] PathSourceDestination = findFolder();
            string sourceBasePath = PathSourceDestination[0];
            string destinationBasePath = PathSourceDestination[1];

            CreateFile(destinationBasePath,typeLog);
            initialiseFileForRealTimeState(destinationBasePath + "\\RealTimeState.json");
            if (input.Length > 0)
            {
                // Convert the input string to a HashSet of strings (presumably containing folder names)
                HashSet<string> output = ConvertToList(input,0);
                HashSet<string> extensionToCrypt;
                if (extensionCrypt != null)
                {
                    extensionToCrypt = ConvertToList(extensionCrypt, 1);
                }
                else
                {
                    extensionToCrypt = new HashSet<string>();
                }
                

                // Iterate over each folder name in the HashSet
                foreach (var item in output)
                {
                    // Construct the full source and destination paths for the current folder
                    string sourcePath = Path.Combine(sourceBasePath, item);
                    string destinationPath = Path.Combine(destinationBasePath, item);

                    try
                    {
                        int totalFiles = CountFiles(sourcePath);
                        long totalFileSize = GetDirectorySize(sourcePath);
                        // Call the recursive method to copy the contents of the source folder to the destination folder

                        bool success = CopyFolderRecursive(sourcePath, destinationPath,item, destinationBasePath, totalFiles, totalFileSize,typeLog, extensionToCrypt, processWrok);

                        if (success)
                        {
                            // Print a success message to the console
                            //Console.WriteLine(_resourceManager.GetString("FolderCopiedSuccessfully", CultureInfo.GetCultureInfo(_viewModel.language)), sourcePath);
                        }


                        if (Directory.Exists(sourcePath))
                        {
                            if (!Directory.Exists(destinationPath))
                            {
                                Directory.CreateDirectory(destinationPath);
                            }

                            string[] files = Directory.GetFiles(sourcePath);
                            bool CheckTargetProcesses; //True if checked (process isn't running), false at default to check
                            foreach (string file in files)
                            {
                                //Check current processes
                                CheckTargetProcesses = false;
                                while (CheckTargetProcesses == false)
                                {
                                    CheckTargetProcesses = CheckCurrentProcesses(processWrok);
                                }
                                string fileName = Path.GetFileName(file);
                                string destinationFile = Path.Combine(destinationPath, fileName);
                                File.Copy(file, destinationFile, true);
                                if (extensionToCrypt.Contains(Path.GetExtension(file)))
                                {
                                    EncryptFile(destinationFile, destinationFile);
                                }
                                nbFileCopy++;
                            }

                            //Console.WriteLine(_resourceManager.GetString("FolderContentCopiedSuccessfully", CultureInfo.GetCultureInfo(_viewModel.language)));
                        }
                        else
                        {
                            //Console.WriteLine(_resourceManager.GetString("FolderDoesNotExist", CultureInfo.GetCultureInfo(_viewModel.language)));
                        }
                    }
                    catch (Exception)
                    {
                        // Print an error message to the console if an exception occurs during the copying process
                        //Console.WriteLine(_resourceManager.GetString("ErrorOccurred", CultureInfo.GetCultureInfo(_viewModel.language)));
                    }
                }
            }
            else
            {
                // Print a message to the console if the input string has a length of 0
                //Console.WriteLine(_resourceManager.GetString("InvalidLengthOfString", CultureInfo.GetCultureInfo(_viewModel.language)));
            }
        }

        // Recursive method to copy the contents of a source folder to a destination folder

        private bool CopyFolderRecursive(string sourcePath, string destinationPath, string item, string destinationBasePath, int totalFiles,long totalFileSize, string typeLog, HashSet<string> extensionToCrypt,string processWrok, int totalFilesLeft = -1, long totalFileSizeLeft = -1)
        {
            if(totalFilesLeft == -1)
            {
                totalFilesLeft = totalFiles - 1;
            }
            if (totalFileSizeLeft == -1)
            {
                totalFileSizeLeft = totalFileSize;
            }
            // Check if the source directory exists
            if (!Directory.Exists(sourcePath))
            {
                // Print an error message to the console if the source directory does not exist
                //Console.WriteLine(_resourceManager.GetString("SourceDirectoryDoesNotExist", CultureInfo.GetCultureInfo(_viewModel.language)), sourcePath);
                return false; // Return false indicating failure
            }

            // Check if the destination directory exists; if not, create it
            if (!Directory.Exists(destinationPath))
            {
                Directory.CreateDirectory(destinationPath);
            }

            bool filesCopied = false;

            // Get the files in the source directory and copy them to the destination directory
            
            string[] files = Directory.GetFiles(sourcePath);
            bool CheckTargetProcesses; //True if checked (process isn't running), false at default to check
            foreach (string file in files)
            {
                //Check current processes
                CheckTargetProcesses = false;
                while (CheckTargetProcesses == false)
                {
                    Thread.Sleep(500);
                    CheckTargetProcesses = CheckCurrentProcesses(processWrok);
                }
                string fileName = Path.GetFileName(file);
                string destinationFile = Path.Combine(destinationPath, fileName);
                Stopwatch chronoFile = new Stopwatch();
                Stopwatch chronoCrypt = new Stopwatch();
                chronoFile.Start();
                File.Copy(file, destinationFile, true);
                filesCopied = true; // At least one file copied successfully
                chronoFile.Stop();
                FileInfo infoFichier = new FileInfo(destinationFile);
                // Récupère la taille du fichier en octets
                long sizeFile = infoFichier.Length;
                totalFileSizeLeft = totalFileSizeLeft - sizeFile;
                if (extensionToCrypt.Contains(Path.GetExtension(file)))
                {
                    chronoCrypt.Start();
                    EncryptFile(destinationFile, destinationFile);
                    chronoCrypt.Stop();
                }
                if (typeLog.Equals("Xml"))
                {
                    WriteFileForLogXML(Path.Combine(destinationBasePath + "\\Log", DateTime.Now.ToString("dd-MM-yyyy") + "_Log.xml"), file, destinationFile, chronoFile, chronoCrypt, sizeFile, item);
                }
                else
                {
                    WriteFileForLog(Path.Combine(destinationBasePath + "\\Log", DateTime.Now.ToString("dd-MM-yyyy") + "_Log.json"), file, destinationFile, chronoFile, chronoCrypt, sizeFile, item);
                }
                WriteFileForRealTimeState(Path.Combine(destinationBasePath, "RealTimeState.json"), item, file, destinationFile, ((totalFilesLeft == 0) ? "END" : "ACTIVE"), totalFiles, totalFileSize, totalFilesLeft, totalFileSizeLeft);
                totalFilesLeft--;
                nbFileCopy++;
            }

            // Get the subdirectories in the source directory and recursively copy their contents
            string[] subdirectories = Directory.GetDirectories(sourcePath);

            foreach (string subdir in subdirectories)
            {
                string subdirName = Path.GetFileName(subdir);
                string destinationSubdir = Path.Combine(destinationPath, subdirName);
                filesCopied |= CopyFolderRecursive(subdir, destinationSubdir, item, destinationBasePath, totalFiles, totalFileSize, typeLog, extensionToCrypt, processWrok, totalFilesLeft, totalFileSizeLeft) ; // Use bitwise OR to propagate success
            }

            return filesCopied; // Return whether any files were copied
        }
        
        private void EncryptFile(string fileName, string destinationFile)
        {
            //Encryption settings
            string configFile = @"..\..\..\config.txt";
            string key;

            try
            {
                key = "1234"; //File.ReadAllText(configFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine($@"Erreur lors de la lecture du fichier de configuration : {ex.Message}");
                return;
            }
            string executablePath = @"..\..\..\Encryption.exe";

            string sFilePath = Path.GetFullPath(executablePath);
            //Calling CryptoSoft to encrypt the file
            Console.WriteLine(sFilePath);
            destinationFile += ".encrypted"; //Define the name destination file

            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = sFilePath,
                Arguments = $"\"{fileName}\" \"{destinationFile}\" {key}",
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            //Calling executable to encrypt
            using (var process = Process.Start(processStartInfo))
            {
                //Wait while file is encrypted
                process.WaitForExit();
            }
            //Delete the temp. file
            File.Delete(fileName);
        }
        
        private bool CheckCurrentProcesses(string processWrok)
        {
            //Declare an array to stock current processes
            Process[] CurrentProcesses;

            //Declare a boolean to return if it's check or not (if application is running)
            bool RunningBusinessProcessing = false;
            //Process recovery
            CurrentProcesses = Process.GetProcesses();
            //Journey of each process to find if the name of the target process match with the name of the process target
            foreach (Process element in CurrentProcesses)
            {
                if(element.ProcessName.Equals(processWrok))
                {
                    //If the name of the target process is found, the verification boolean is false and the loop is exited
                    RunningBusinessProcessing = false;
                    break;
                }
                else
                {RunningBusinessProcessing = true;}
            }
            return RunningBusinessProcessing;
        }
    }

    //class that contains all the information needed for the RealTimeState
    class RealTimeStateInfo
    {
        public string Name { get; set; }
        public string SourceFilePath { get; set; }
        public string TargetFilePath { get; set; }
        public string State { get; set; }
        public int TotalFilesToCopy { get; set; }
        public int NbFilesLeftToDo { get; set; }
        public long TotalFilesSize { get; set; }
        public long FileSizeLeft { get; set; }
        public int Progression { get; set; }
    }

    //class that contains all the information needed for the log
    class LogInfo
    {
        public string Name { get; set; }
        public string SourceFilePath { get; set; }
        public string TargetFilePath { get; set; }
        public long FileSize { get; set; }
        public string FileTransfertTime { get; set; }
        public string FileCryptageTime { get; set; }
        public string Date { get; set; }
    }

}
