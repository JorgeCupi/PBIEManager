using Microsoft.PowerBI.Api.V1.Models;
using PBIEMobile;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Windows.Input;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace PBIEMobileSDK.ViewModels
{
    class DashboardViewModel:INotifyPropertyChanged
    {
        private static ApplicationDataContainer localSettings;
        
        /// <summary>
        /// Region containing string objects for the Key and Workspace Collection Name 
        /// </summary>
        #region Credentials
        private string _Key;
        public string Key
        {
            get { return _Key; }
            set
            {
                _Key = value;
                OnPropertyChanged("Key");
                localSettings.Values["PBIEKey"] = _Key;
            }
        }

        private string _WorkspaceCollectionName;
        public string WorkspaceCollectionName
        {
            get { return _WorkspaceCollectionName; }
            set
            {
                _WorkspaceCollectionName = value;
                OnPropertyChanged("WorkspaceCollectionName");
                localSettings.Values["WorkspaceCollectionName"] = _WorkspaceCollectionName;
            }
        }
        #endregion

        /// <summary>
        /// Region containing a series of objects such as strings, Reports, Imports, Workspaces and such
        /// that will be used through the App for binding info to the view.
        /// </summary>
        #region Objects: Imports, Reports, Workspaces, File

        private string _WorkspaceNameToCreate;
        public string WorkspaceNameToCreate
        {
            get { return _WorkspaceNameToCreate; }
            set
            {
                _WorkspaceNameToCreate = value;
                OnPropertyChanged("WorkspaceNameToCreate");
            }
        }

        private string _DataSetNameToCreate;
        public string DataSetNameToCreate
        {
            get { return _DataSetNameToCreate; }
            set
            {
                _DataSetNameToCreate = value;
                OnPropertyChanged("DataSetNameToCreate");
            }
        }

        private string _Html;
        public string Html
        {
            get { return _Html; }
            set
            {
                _Html = value;
                OnPropertyChanged("Html");
            }
        }

        private Stream _StreamFromSelectedFile;
        public Stream StreamFromSelectedFile
        {
            get { return _StreamFromSelectedFile; }
            set
            {
                _StreamFromSelectedFile = value;
                OnPropertyChanged("StreamFromSelectedFile");
            }
        }

        private string _SelectedFile;
        public string SelectedFile
        {
            get { return _SelectedFile; }
            set
            {
                _SelectedFile = value;
                OnPropertyChanged("SelectedFile");
            }
        }

        private Import _SelectedImport;
        public Import SelectedImport
        {
            get { return _SelectedImport; }
            set
            {
                _SelectedImport = value;
                OnPropertyChanged("SelectedImport");
                if(SelectedImport != null && SelectedImport.Reports !=null)
                {
                    Reports = new ObservableCollection<Report>(SelectedImport.Reports);
                    if (Reports.Count > 0)
                        IsReportResultVisible = false;
                    else ReportResult = "There are no reports in this Import";
                }
            }
        }

        private ObservableCollection<Import> _Imports;
        public ObservableCollection<Import> Imports
        {
            get { return _Imports; }
            set
            {
                _Imports = value;
                OnPropertyChanged("Imports");
            }
        }

        private Workspace _SelectedWorkspace;
        public Workspace SelectedWorkspace
        {
            get { return _SelectedWorkspace; }
            set
            {
                _SelectedWorkspace = value;
                OnPropertyChanged("SelectedWorkspace");
                if (_SelectedWorkspace !=null)
                {
                    if(Reports!=null)
                        Reports.Clear();
                    PBIEClient.SetWorkspaceId(_SelectedWorkspace.WorkspaceId);
                    LoadImports();
                }
            }
        }

        private ObservableCollection<Workspace> _Workspaces;
        public ObservableCollection<Workspace> Workspaces
        {
            get { return _Workspaces; }
            set
            {
                _Workspaces = value;
                OnPropertyChanged("Workspaces");
            }
        }

        private Report _SelectedReport;
        public Report SelectedReport
        {
            get { return _SelectedReport; }
            set
            {
                _SelectedReport = value;
                OnPropertyChanged("SelectedReport");
                if (SelectedReport != null)
                    LoadReport();
            }
        }

        private ObservableCollection<Report> _Reports;
        public ObservableCollection<Report> Reports
        {
            get { return _Reports; }
            set
            {
                _Reports = value;
                OnPropertyChanged("Reports");
            }
        }
        #endregion
        
        /// <summary>
        /// Region containing string and boold objects that function as helpers to modify the UI
        /// </summary>
        #region Results (bools to define UX visibility)
        private string _WorkspaceResult;
        public string WorkspaceResult
        {
            get { return _WorkspaceResult; }
            set
            {
                _WorkspaceResult = value;
                OnPropertyChanged("WorkspaceResult");
            }
        }

        private bool _IsWorkspaceResultVisible;
        public bool IsWorkspaceResultVisible
        {
            get { return _IsWorkspaceResultVisible; }
            set
            {
                _IsWorkspaceResultVisible = value;
                OnPropertyChanged("IsWorkspaceResultVisible");
            }
        }

        private string _ReportResult;
        public string ReportResult
        {
            get { return _ReportResult; }
            set
            {
                _ReportResult = value;
                OnPropertyChanged("ReportResult");
            }
        }

        private string _ImportResult;
        public string ImportResult
        {
            get { return _ImportResult; }
            set
            {
                _ImportResult = value;
                OnPropertyChanged("ImportResult");
            }
        }

        private bool _IsImportResultVisible;
        public bool IsImportResultVisible
        {
            get { return _IsImportResultVisible; }
            set
            {
                _IsImportResultVisible = value;
                OnPropertyChanged("IsImportResultVisible");
            }
        }

        private bool _IsReportResultVisible;
        public bool IsReportResultVisible
        {
            get { return _IsReportResultVisible; }
            set
            {
                _IsReportResultVisible = value;
                OnPropertyChanged("IsReportResultVisible");
            }
        }
        #endregion

        #region Main methods: Read, Create, Update Workspaces and Imports
        public ICommand SetCredentialsPopUpCommand { get; set; }
        public void SetCredentialsPopUp()
        {
            SetCredentialsPopUpIsOpen = true;
            MainScreenOpacity = 0.5;
        }

        public ICommand LoadCredentialsCommand { get; set; }
        public void LoadCredentials()
        {
            LoadingActivityStarted(true);
            try
            {
                localSettings = ApplicationData.Current.LocalSettings;
                Key = localSettings.Values["PBIEKey"].ToString();
                WorkspaceCollectionName = localSettings.Values["WorkspaceCollectionName"].ToString();
                PBIEClient.InitializePBIEClient(Key, WorkspaceCollectionName);

                ClearUX();
            }
            catch (Exception)
            {
                DisplayMessage("No credentials", "Please enter valid Access Key and Workspace Collection Name values");
            }
            LoadingActivityStarted(false);
        }

        public ICommand CreateWorkspacePopUpCommand { get; set; }
        public void CreateWorkspacePopUp()
        {
            CreateWorkspacePopUpIsOpen = true;
            MainScreenOpacity = 0.5;
        }

        public ICommand CreateWorkspaceCommand { get; set; }
        public async void CreateWorkspace()
        {
            CreateWorkspacePopUpIsOpen = false;
            ActivityStatus = String.Format("Your Workspace {0} is being created...", WorkspaceNameToCreate);
            LoadingActivityStarted(true);

            var createdWorkspace = await PBIEClient.CreateWorkspace(WorkspaceNameToCreate);
            if (createdWorkspace != null)
            {
                ActivityStatus = "Your workspace has been created";
                Workspaces.Add(createdWorkspace);
                IsWorkspaceResultVisible = false;
            }

            else DisplayMessage("Create new Workspace", "Something went wrong, please try again");
            LoadingActivityStarted(false);
        }

        public ICommand AddImportPopUpCommand { get; set; }
        public void AddImportPopUp()
        {
            AddImportPopUpIsOpen = true;
            MainScreenOpacity = 0.5;
        }

        public ICommand AddImportCommand { get; set; }
        public async void AddImport()
        {
            AddImportPopUpIsOpen = false;
            ActivityStatus = "Your report is being uploaded...";
            LoadingActivityStarted(true);

            var createdImport = await PBIEClient.UploadImport(DataSetNameToCreate, StreamFromSelectedFile);

            StreamFromSelectedFile.Dispose();
            SelectedFile = String.Empty;

            if (createdImport != null)
            {
                ActivityStatus = "Your report has been uploaded";
                Imports.Add(createdImport);
                IsImportResultVisible = false;
            }
            else DisplayMessage("Unknown error", "Something went wrong, please try again");

            LoadingActivityStarted(false);
        }

        public ICommand SelectFileCommand { get; set; }
        public async void SelectFile()
        {
            var openPicker = new Windows.Storage.Pickers.FileOpenPicker()
            {
                ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail,
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Downloads
            };
            openPicker.FileTypeFilter.Add(".pbix");

            var file = await openPicker.PickSingleFileAsync();

            if (file != null)
            {
                try
                {
                    StreamFromSelectedFile = await file.OpenStreamForReadAsync();
                    SelectedFile = file.Path;
                }
                catch (Exception)
                {
                    DisplayMessage("PBIX file in use", "Please close your PowerBI Desktop App before uploading the PBIX file.");
                    SelectedFile = "No file selected";
                }
            }
            else
                SelectedFile = "No file selected";
        }

        public ICommand LoadWorkspacesCommand { get; set; }
        public async void LoadWorkspaces()
        {
            LoadingActivityStarted(true);

            try
            {
                ClearUX();

                Workspaces = new ObservableCollection<Workspace>(
                    await PBIEClient.GetWorkspaces());
                if (Workspaces.Count > 0)
                    IsWorkspaceResultVisible = false;
                else
                {
                    IsWorkspaceResultVisible = true;
                    WorkspaceResult = String.Format("There are no workspaces in your '{0}' workspace collection", WorkspaceCollectionName);
                }
                    
            }
            catch (Exception ex)
            {
                IsWorkspaceResultVisible = true;
                WorkspaceResult = ex.ToString();
            }
            LoadingActivityStarted(false);
        }

        public ICommand LoadImportsCommand { get; set; }
        public async void LoadImports()
        {
            LoadingActivityStarted(true);
            try
            {
                Imports = new ObservableCollection<Import>(
                    await PBIEClient.GetImports());
                if(Imports.Count>0)
                    IsImportResultVisible = false;
                else
                {
                    IsImportResultVisible = true;
                    ImportResult = String.Format("There are no reports in {0} workspace", SelectedWorkspace.DisplayName);
                }
                    
            }
            catch (Exception ex)
            {
                IsImportResultVisible = true;
                ImportResult = ex.ToString();
            }
            LoadingActivityStarted(false);
        }
        #endregion
        
        /// <summary>
        /// Constructor, defines the Command <-> Event relationships, initializes the PBIE Client and the objects that will be used through the App
        /// </summary>
        public DashboardViewModel()
        {
            LoadingActivityStarted(false);

            SetCredentialsPopUpCommand = new Command(SetCredentialsPopUp);
            CloseCredentialsPopUpCommand = new Command(CloseCredentialsPopUp);
            CancelWorkspaceCommand = new Command(CancelWorkspace);
            CancelImportCommand = new Command(CancelImport);
            SelectFileCommand = new Command(SelectFile);
            CreateWorkspacePopUpCommand = new Command(CreateWorkspacePopUp);
            CreateWorkspaceCommand = new Command(CreateWorkspace);
            AddImportPopUpCommand = new Command(AddImportPopUp);
            AddImportCommand = new Command(AddImport);
            LoadCredentialsCommand = new Command(LoadCredentials);
            LoadWorkspacesCommand = new Command(LoadWorkspaces);
            LoadImportsCommand = new Command(LoadImports);

            SelectedWorkspace = new Workspace();
            SelectedImport = new Import();

            Workspaces = new ObservableCollection<Workspace>();
            Workspaces.CollectionChanged += Workspaces_CollectionChanged;
            Imports = new ObservableCollection<Import>();
            Imports.CollectionChanged += Imports_CollectionChanged;
            Reports = new ObservableCollection<Report>();
            Reports.CollectionChanged += Reports_CollectionChanged;

            LoadCredentials();
        }

        /// <summary>
        /// 'Cheat' code to Display a selected Report.
        /// It breaks the MVVM Model as it creates a PopUpContent that will be displayed in the View.
        /// This is done because the WebView's Source can not bind a NavigationString straight forward
        /// </summary>
        private async void LoadReport()
        {
            MainScreenOpacity = 0.5;
            IsReportPopUpOpen = true;
            Html = await PBIEClient.LoadReport(SelectedReport, SelectedImport.Datasets[0].Id);

            ReportStackPanel = new StackPanel()
            {
                Height = 600,
                Width = 700
            };

            WebView wv = new WebView()
            {
                Margin = new Thickness(10),
                Height = 480,
                Width = 640,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            wv.NavigateToString(Html);

            Button bt = new Button()
            {
                Content = "Close window",
                Height = 40
            };

            bt.Click += (sender, eventArgs) =>
            {
                IsReportPopUpOpen = false;
                MainScreenOpacity = 1;
            };

            bt.HorizontalAlignment = HorizontalAlignment.Center;
            bt.Margin = new Thickness(10);

            ReportStackPanel.Children.Add(wv);
            ReportStackPanel.Children.Add(bt);
        }

        #region Collections changed events
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Reports_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("Reports");
        }

        private void Workspaces_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("workspaces");
        }

        private void Imports_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("Imports");
        }
        #endregion

        /// <summary>
        /// Region that contains a set of UX helpers
        /// </summary>
        #region UX
        private void ClearUX()
        {
            Workspaces.Clear();
            Imports.Clear();
            Reports.Clear();

            WorkspaceResult = "Refresh your workspaces list";
            ImportResult = "Select a Workspace to load its imports";
            ReportResult = "Select an Import to load its reports";

            IsWorkspaceResultVisible = true;
            IsImportResultVisible = true;
            IsReportResultVisible = true;
        }

        private StackPanel _ReportStackPanel;
        public StackPanel ReportStackPanel
        {
            get { return _ReportStackPanel; }
            set
            {
                _ReportStackPanel = value;
                OnPropertyChanged("ReportStackPanel");
            }
        }

        private bool _SetCredentialsPopUpIsOpen;
        public bool SetCredentialsPopUpIsOpen
        {
            get { return _SetCredentialsPopUpIsOpen; }
            set
            {
                _SetCredentialsPopUpIsOpen = value;
                OnPropertyChanged("SetCredentialsPopUpIsOpen");
            }
        }


        private bool _IsReportPopUpOpen;
        public bool IsReportPopUpOpen
        {
            get { return _IsReportPopUpOpen; }
            set
            {
                _IsReportPopUpOpen = value;
                OnPropertyChanged("IsReportPopUpOpen");
            }
        }

        public ICommand CloseCredentialsPopUpCommand { get; set; }
        public void CloseCredentialsPopUp()
        {
            LoadCredentials();
            SetCredentialsPopUpIsOpen = false;
            MainScreenOpacity = 1;
        }

        public ICommand CancelReportCommand { get; set; }
        public void CancelReport()
        {
            IsReportPopUpOpen = false;
            MainScreenOpacity = 1;
        }

        public ICommand CancelImportCommand { get; set; }
        public void CancelImport()
        {
            AddImportPopUpIsOpen = false;
            MainScreenOpacity = 1;
        }

        public ICommand CancelWorkspaceCommand { get; set; }
        public void CancelWorkspace()
        {
            CreateWorkspacePopUpIsOpen = false;
            MainScreenOpacity = 1;
        }

        private bool _CreateWorkspacePopUpIsOpen;
        public bool CreateWorkspacePopUpIsOpen
        {
            get { return _CreateWorkspacePopUpIsOpen; }
            set
            {
                _CreateWorkspacePopUpIsOpen = value;
                OnPropertyChanged("CreateWorkspacePopUpIsOpen");
            }
        }

        private bool _AddImportPopUpIsOpen;
        public bool AddImportPopUpIsOpen
        {
            get { return _AddImportPopUpIsOpen; }
            set
            {
                _AddImportPopUpIsOpen = value;
                OnPropertyChanged("AddImportPopUpIsOpen");
            }
        }

        private async void DisplayMessage(string title, string message)
        {
            await new MessageDialog(message, title).ShowAsync();
        }

        private void LoadingActivityStarted(bool hasStarted)
        {
            if (hasStarted)
            {
                LoadingActivity = true;
                MainScreenOpacity = 0.5;
            }
            else
            {
                LoadingActivity = false;
                MainScreenOpacity = 1;
                ActivityStatus = String.Empty;
            }
        }

        private bool _LoadingActivity;
        public bool LoadingActivity
        {
            get { return _LoadingActivity; }
            set
            {
                _LoadingActivity = value;
                OnPropertyChanged("LoadingActivity");
            }
        }

        private string _ActivityStatus;
        public string ActivityStatus
        {
            get { return _ActivityStatus; }
            set
            {
                _ActivityStatus = value;
                OnPropertyChanged("ActivityStatus");
            }
        }

        private double _MainScreenOpacity;
        public double MainScreenOpacity
        {
            get { return _MainScreenOpacity; }
            set
            {
                _MainScreenOpacity = value;
                OnPropertyChanged("MainScreenOpacity");
            }
        }
        #endregion
    }
}