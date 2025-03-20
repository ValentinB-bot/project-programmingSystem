using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EasySaveV2._0
{

    public class ViewModel : INotifyPropertyChanged
    {

        public ViewModel()
        {
            UpdateButtonContent();
        }

        private void UpdateButtonContent()
        {
            if (LanguageBoxCommand.Equals("Français"))
            {
                ContentCopyButton = "Copier";
                LanguageLabel = "Langue";
                ExtensionLogLabel = "Extension Log";
                WhiteListeLabel = "ListeBlanche Cryptage";
                ProcessusMetierLabel = "Processus Métier";
                PlayPauseStop = "En Attente";

            }
            else
            {
                ContentCopyButton = "Copy";
                LanguageLabel = "Language";
                ExtensionLogLabel = "Extension Log";
                WhiteListeLabel = "WhiteList Cryptage";
                ProcessusMetierLabel = "Work Process";
                PlayPauseStop = "Waiting";
            }
        }

        #region MainWindow;


        private string contentCopyButton;
        public string ContentCopyButton
        {
            get { return contentCopyButton; }
            set
            {
                if (contentCopyButton != value)
                {
                    contentCopyButton = value;
                    OnPropertyChanged();
                }
            }
        }

        private string saveToCopy;
        public string SaveToCopy
        {
            get { return saveToCopy; }
            set
            {
                if (saveToCopy != value)
                {
                    saveToCopy = value;
                    OnPropertyChanged();
                }
            }
        }

        private int progressValue;
        public int ProgressValue
        {
            get { return progressValue; }
            set
            {
                if (progressValue != value)
                {
                    progressValue = value;
                    OnPropertyChanged();
                }
            }
        }

        private string playPauseStop;
        public string PlayPauseStop
        {
            get { return playPauseStop; }
            set
            {
                if (playPauseStop != value)
                {
                    playPauseStop = value;
                    OnPropertyChanged();
                }
            }
        }



        
        private ICommand openParameterCommand;
        public ICommand OpenParameterCommand
        {
            get
            {
                if (openParameterCommand == null)
                {
                    openParameterCommand = new RelayCommand(OpenParameter);
                }
                return openParameterCommand;
            }
        }

        private ICommand copierCommand;
        public ICommand CopierCommand
        {
            get
            {
                if (copierCommand == null)
                {
                    copierCommand = new RelayCommand(StartProgress);
                }
                return copierCommand;
            }
        }



        private void OpenParameter()
        {
            ParameterWindow parameterWindow = new ParameterWindow();
            parameterWindow.Show();
        }
        private async void StartProgress()
        {
            Model model = new Model(this);
            if (languageBoxCommand.Equals("Français"))
            {
                PlayPauseStop = "En cours";
            }
            else
            {
                PlayPauseStop = "Playing";
            }
            OnPropertyChanged();
            /*await Task.Run(() =>
            {

                HashSet<string> allSave = model.ConvertToList(saveToCopy,0);
                int nbMaxFile = 0;
                nbMaxFile += model.GetCountAllFile(allSave);
                int progressStep = model.nbFileCopy / nbMaxFile;
                // Update the ProgressBar here
                // Use Dispatcher.Invoke to update the UI from a different thread
                for (int i = 0; i <= 100; i++)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ProgressValue = progressStep;
                    });

                    Thread.Sleep(500); // Ne bloque pas le thread principal ici, sinon cela annule l'effet de l'utilisation de Task.Run
                }
            });*/

            await Task.Run(() =>
            {
                if (saveToCopy != null && saveToCopy != "")
                {
                

                    //call the Run method of the view
                    string typeFileLog;

                    //check that the necessary files have been found
                    try
                    {
                        string[] pathFolder = model.findFolder();

                    }
                    catch (Exception ex)
                    {

                    }
                    model.CopyFolderContent(saveToCopy, extensionBoxCommand,whiteListCryptage, processusWork);
                    if (languageBoxCommand.Equals("Français"))
                    {
                        MessageBox.Show("Sauvegarde " + saveToCopy + " copier avec succès");
                    }
                    else
                    {
                        MessageBox.Show("Save " + saveToCopy + " copy successful");
                    }
                
                }
                else
                {
                    if (languageBoxCommand.Equals("Français"))
                    {
                        MessageBox.Show("Veuillez choisir les sauvegardes à copier");
                    }
                    else
                    {
                        MessageBox.Show("Please select the save you wish to copy");
                    }
                
                }
            });

            if (languageBoxCommand.Equals("Français"))
            {
                PlayPauseStop = "Fini";
            }
            else
            {
                PlayPauseStop = "Finish";
            }
            OnPropertyChanged();
        }

        #endregion

        #region ParameterWindow;

        private string languageLabel;
        public string LanguageLabel
        {
            get { return languageLabel; }
            set
            {
                if (languageLabel != value)
                {
                    languageLabel = value;
                    OnPropertyChanged();
                }
            }
        }

        private string extensionLogLabel;
        public string ExtensionLogLabel
        {
            get { return extensionLogLabel; }
            set
            {
                if (extensionLogLabel != value)
                {
                    extensionLogLabel = value;
                    OnPropertyChanged();
                }
            }
        }

        private string whiteListeLabel;
        public string WhiteListeLabel
        {
            get { return whiteListeLabel; }
            set
            {
                if (whiteListeLabel != value)
                {
                    whiteListeLabel = value;
                    OnPropertyChanged();
                }
            }
        }

        private string processusMetierLabel;
        public string ProcessusMetierLabel
        {
            get { return processusMetierLabel; }
            set
            {
                if (processusMetierLabel != value)
                {
                    processusMetierLabel = value;
                    OnPropertyChanged();
                }
            }
        }

        private string languageBoxCommand = "English";
        public string LanguageBoxCommand
        {
            get { return languageBoxCommand; }
            set
            {
                languageBoxCommand = value.Substring(39).ToString();
                UpdateButtonContent();
                OnPropertyChanged();
            }
        }

        private string extensionBoxCommand = "Json";
        public string ExtensionBoxCommand
        {
            get { return extensionBoxCommand; }
            set
            {
                extensionBoxCommand = value.Substring(39).ToString();
                OnPropertyChanged(nameof(ExtensionBoxCommand));
            }
        }

        private string whiteListCryptage;
        public string WhiteListCryptage
        {
            get { return whiteListCryptage; }
            set
            {
                whiteListCryptage = value;
                OnPropertyChanged();
            }
        }

        private string processusWork;
        public string ProcessusWork
        {
            get { return processusWork; }
            set
            {
                processusWork = value;
                OnPropertyChanged();
            }
        }


        #endregion


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public class RelayCommand : ICommand
        {
            private readonly Action _execute;

            public RelayCommand(Action execute)
            {
                _execute = execute;
            }

            public event EventHandler CanExecuteChanged;

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                _execute?.Invoke();
            }
        }
    }
}
