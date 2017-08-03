using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using DropBoxLinker.Properties;

namespace DropBoxLinker
{
	public partial class frmGetLinks : Window
	{
		public frmGetLinks()
		{
			InitializeComponent();

			// create filter
			Filter = new BackgroundWorker();
			Filter.WorkerSupportsCancellation = true;
			Filter.DoWork += FilterStart;
			Filter.RunWorkerCompleted += FilterComplete;

			// load and sort contents
			SyncRoot = new SyncFolder(Settings.Default.SyncDirectory);
			SortingChanged(null, null);

			// set contexts
			treeBrowser.DataContext = SyncRoot;
			grdSelectionInfo.DataContext = SyncRoot;

			// total file count
			sldDateRange.Maximum = SyncRoot.Dates.Count - 1;

			// show 10 files (or all of them)
			sldDateRange.Value = (sldDateRange.Maximum > 9) ? sldDateRange.Maximum - 9 : 0;
			FilterChange(null, null);
		}

		// data
		public static SyncFolder SyncRoot;

		// filtering
		private BackgroundWorker Filter;
		private Int32 LastFilteredValue = -1;
		private void FilterChange(object sender, EventArgs e)
		{
			// check-in
			if (SyncRoot == null || Filter == null) return;

			// get new value
			var value = (int)sldDateRange.Value;

			// do not perform filtering twice
			if (value == LastFilteredValue) return;

			// restart or start
			if (Filter.IsBusy) Filter.CancelAsync();
			else Filter.RunWorkerAsync(value);
			
			
		}
		private void FilterStart(object sender, DoWorkEventArgs e)
		{
			// get value
			var value = (int)e.Argument;

			// get date to filter
			var date = SyncRoot.Dates[value];

			// apply filtering
			SyncRoot.ApplyDateFilter(date, Filter);

			// save results
			if (!Filter.CancellationPending)
				LastFilteredValue = value;
		}
		private void FilterComplete(object sender, RunWorkerCompletedEventArgs e)
		{
			// check new value
			var value = (int)sldDateRange.Value;

			// re-start, if it has changed
			if (value != LastFilteredValue)
				Filter.RunWorkerAsync(value);
		}

		// sorting
		private void SortingChanged(object sender, SelectionChangedEventArgs e)
		{
			if (lstSortKind == null || lstSortOrder == null || SyncRoot == null) return;

			var kind = (lstSortKind.SelectedItem as ComboBoxItem).Content as String;
			var order = (lstSortOrder.SelectedItem as ComboBoxItem).Content as String;

			Func<SyncFile, dynamic> sort_key;
			switch (kind) {
				case "date": sort_key = sf => sf.Date; break;
				case "name": sort_key = sf => sf.Name; break;
				case "type": sort_key = sf => sf.Ext;  break;
				case "size": sort_key = sf => sf.Size; break;
				default: sort_key = null; break; }

			SyncRoot.Sort(sort_key, order == "ascending");
		}

		// context menu
		private void BrowserRefreshList(object sender, RoutedEventArgs e)
		{
			// save currently selected filter
			var value = sldDateRange.Value;

			// reload and sort contents
			SyncRoot = new SyncFolder(Settings.Default.SyncDirectory);
			SortingChanged(null, null);

			// reset contexts
			treeBrowser.DataContext = SyncRoot;
			grdSelectionInfo.DataContext = SyncRoot;

			// total file count
			sldDateRange.Maximum = SyncRoot.Dates.Count - 1;

			// try to reload previous value
			sldDateRange.Value = (sldDateRange.Maximum > value) ? value : 0;

			// force refiltering
			LastFilteredValue = -1;
			FilterChange(null, null);
		}
		private void BrowserSelectAll(object sender, RoutedEventArgs e)
		{
			ChangeSelection(SyncRoot, b => true);
		}
		private void BrowserDeselectAll(object sender, RoutedEventArgs e)
		{
			ChangeSelection(SyncRoot, b => false);
		}
		private void BrowserInvertSelection(object sender, RoutedEventArgs e)
		{
			ChangeSelection(SyncRoot, b => !b);
		}
		private void ChangeSelection(SyncFolder folder, Func<bool, bool> func)
		{
			foreach (var c in folder.VisibleChildren)
			{
				if (c is SyncFile)
				{
					(c as SyncFile).IsSelected =
						func((bool)(c as SyncFile).IsSelected);
				}
				else if (c is SyncFolder)
				{
					ChangeSelection(c as SyncFolder, func);
				}
			}
		}

		// commands
		private void GetLinks(object sender, RoutedEventArgs e)
		{
			// retrieve selected files
			var selected_files = SyncRoot.SelectedFiles;
			if (selected_files.Count == 0) {
				MessageBox.Show("Nothing selected!", "Error", MessageBoxButton.OK,
				MessageBoxImage.Information); return; }

			Hide();

			// create URL list
			var links = "";
			for (var i = 0; i < selected_files.Count - 1; i++)
				links += App.GetPublicURL(selected_files[i]) + "\r\n";
			links += App.GetPublicURL(selected_files.Last());

			// update clipboard
			if (Clipboard.ContainsText())
			{
				// get existing text
				var text = Clipboard.GetText();

				// are there any dropbox links?
				if (text.Contains(Settings.Default.HttpPath) && !Settings.Default.CleanClipboard)
				{
					// append links to the end
					Clipboard.SetText(text + "\r\n" + links);

					// notify
					foreach (var f in selected_files)
						frmNotify.Launch(f, NotifyType.Append);
				}
				else
				{
					// overwrite with links
					Clipboard.SetText(links);

					// notify
					frmNotify.Launch(selected_files.First(), NotifyType.Set);
					for (var f = 1; f < selected_files.Count; f++)
						frmNotify.Launch(selected_files[f], NotifyType.Append);
				}
			}
			
			// something special
			else
			{
				// overwrite with links
				Clipboard.SetText(links);

				// notify
				frmNotify.Launch(selected_files.First(), NotifyType.Set);
				for (var f = 1; f < selected_files.Count; f++)
					frmNotify.Launch(selected_files[f], NotifyType.Append);
			}

			Close();
		}
		private void Cancel(object sender, RoutedEventArgs e)
		{
			Close();
		}

	}

	// classes
	public abstract class SyncItem : INotifyPropertyChanged
	{
		// common properties
		public SyncFolder Parent { get; protected set; }
		public string Name { get; protected set; }

		// other abstract properties
		public abstract bool IsVisible { get; set; }
		public abstract bool? IsSelected { get; set; }
		public abstract ulong Size { get; }
		public abstract ulong SelectedSize { get; }
		public abstract uint SelectedCount { get; }

		// INotifyPropertyChanged
		public event PropertyChangedEventHandler PropertyChanged;
		public void NotifyPropertyChanged(String PropertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
		}
	}
	public class SyncFolder : SyncItem
	{
		// sorting
		private static Boolean _SortAscending = false;
		private static Func<SyncFile, dynamic> _SortKind = si => si.Date;
		public void Sort(Func<SyncFile, dynamic> kind, Boolean ascending)
		{
			_SortKind = kind;
			_SortAscending = ascending;
			//_Sort = value;
			SortFolder();
			NotifyPropertyChanged("VisibleChildren");
			
		}
		private void SortFolder()
		{
			// get and sort folders
			//var folders = Children.Where(c => c is SyncFolder)
			//    .Select(si => si as SyncFolder)
			//    .OrderBy(si => si.Name);

			var child = _Children.OrderByDescending(si => si is SyncFolder);

			child = (_SortAscending) ?
				child.ThenBy(si => (si is SyncFile) ? _SortKind(si as SyncFile) : (si as SyncFolder).Name) :
				child.ThenByDescending(si => (si is SyncFile) ? _SortKind(si as SyncFile) : null)
					 .ThenBy(si => (si is SyncFolder) ? (si as SyncFolder).Name : null);

			_Children = child.ToList();

			// get and sort files
			//var files = _Sort(Children.Where(c => c is SyncFile).Select(si => si as SyncFile));

			// concatenate
			//var child = folders.Select(f => f as SyncItem).ToList();
			//child.AddRange(files.Select(f => f as SyncItem).ToList());

			// done
			//_Children = child;


			// sync subfolders
			foreach (var si in _Children)
				if (si is SyncFolder)
					(si as SyncFolder).SortFolder();
		}

		// children
		private List<SyncItem> _Children;
		public List<SyncItem> Children 
		{
			get { return _Children; } 
		}
		public List<SyncItem> VisibleChildren
		{
			get { return Children.Where(c => c.IsVisible == true).ToList(); }
		}
		
		// visibility
		public override bool IsVisible
		{
			get  { return VisibleChildren.Count > 0; }
			set
			{
				foreach (var c in Children)
					c.IsVisible = value;

				NotifyPropertyChanged("IsVisible");
				NotifyPropertyChanged("VisibleChildren");
				NotifyPropertyChanged("SelectedCount");
				NotifyPropertyChanged("SelectedSize");
			}
		}

		// filtering
		public void ApplyDateFilter(DateTime date, BackgroundWorker worker)
		{
			foreach (var c in Children)
			{
				if (worker.CancellationPending)
					return;

				if (c is SyncFile)
				{
					c.IsVisible = (c as SyncFile).Date >= date;
				}
				else if (c is SyncFolder)
				{
					(c as SyncFolder).ApplyDateFilter(date, worker);
				}
			}
		}

		// dates list
		private List<DateTime> _Dates;
		public List<DateTime> Dates
		{
			get
			{
				// create
				if (_Dates == null)
				{
					var dates = new List<DateTime>();

					// append subfolders' content
					var dirs = Children.Where(c => c is SyncFolder)
								.OfType<SyncFolder>().ToList();
					foreach (var d in dirs) dates.AddRange(d.Dates);

					// append files
					var files = Children.Where(c => c is SyncFile)
								.OfType<SyncFile>().ToList();
					foreach (var f in files) dates.Add(f.Date);

					// sort and return
					_Dates = dates.OrderBy(d => d).ToList();
				}

				return _Dates;
			}
		}

		// selected paths list
		public List<String> SelectedFiles
		{
			get
			{
				var selected_files = VisibleChildren
					.Where(si => (si is SyncFile) && si.IsSelected == true)
					.Select(si => (si as SyncFile).Path).ToList();

				var selected_folders = VisibleChildren.Where(si => si is SyncFolder &&
					si.IsSelected != false).Select(si => si as SyncFolder).ToList();
				foreach (var f in selected_folders)
					selected_files.AddRange(f.SelectedFiles);

				return selected_files;
			}
		}

		// selection
		public override bool? IsSelected
		{
			get
			{
				// count selected and non-selected content
				var selected_count = Children.Count(c => c.IsVisible && c.IsSelected == true);
				var unselected_count = Children.Count(c => c.IsVisible && c.IsSelected == false);
				var undefined_count = Children.Count(c => c.IsVisible && c.IsSelected == null);

				// switch cases
				if (selected_count > 0 && unselected_count > 0) return null; // some selected, some not
				else if (selected_count > 0 && unselected_count == 0) return true; // everything selected
				else if (undefined_count > 0) return null;
				else return false; // nothing selected
			}
			set
			{
				if (value != null)
				{
					foreach (var c in VisibleChildren)
					{
						c.IsSelected = value;
					}
						

					NotifyPropertyChanged("IsSelected");
					NotifyPropertyChanged("SelectedSize");
					NotifyPropertyChanged("SelectedCount");

					if (Parent != null)
					{
						Parent.NotifyPropertyChanged("SelectedSize");
						Parent.NotifyPropertyChanged("SelectedCount");
					}
				}
			}
		}

		// counters
		public override ulong Size
		{
			get { return (ulong)Children.Sum(si => (long)si.Size); }
		}
		public override ulong SelectedSize
		{
			get 
			{
				var size = 0UL;

				foreach (var c in VisibleChildren)
				{
					if (c.IsSelected != false)
						size += c.SelectedSize;
				}

				return size;

				//return (ulong)VisibleChildren
				//    .Where(si => si.IsSelected != false)
				//    .Sum(si => (long)si.SelectedSize); 
			
			}
		}
		public override uint SelectedCount
		{
			get { return (uint)VisibleChildren
					.Where(si => si.IsSelected != false)
					.Sum(si => (int)si.SelectedCount); }
		}

		public void OnChildrenSelectionChange()
		{
			NotifyPropertyChanged("IsSelected");

			if (Parent != null)
			{
				Parent.OnChildrenSelectionChange();
			}
			else
			{
				NotifyPropertyChanged("SelectedSize");
				NotifyPropertyChanged("SelectedCount");
			}

		}
		public void OnChildrenVisibilityChange()
		{
			NotifyPropertyChanged("IsVisible");
			NotifyPropertyChanged("VisibleChildren");

			if (Parent != null)
			{
				Parent.OnChildrenVisibilityChange();
			}
			else
			{
				NotifyPropertyChanged("SelectedSize");
				NotifyPropertyChanged("SelectedCount");
			}

		}

		// constructors
		public SyncFolder(DirectoryInfo info, SyncFolder parent)
		{
			// create self
			Name = info.Name;
			Parent = parent;
			_Children = new List<SyncItem>();

			// append folders
			var folders = info.GetDirectories()
				.OrderBy(d => d.Name)
				.Select(di => new SyncFolder(di, this));
			_Children.AddRange(folders);

			// append files
			var files = info.GetFiles()
				.Select(fi => new SyncFile(fi, this));
			_Children.AddRange(files);
		}
		public SyncFolder(String path) :
			this(new DirectoryInfo(path), null) { }
		
	}
	public class SyncFile : SyncItem
	{
		public string Path { get; private set; }

		// selection
		private bool? _IsSelected;
		public override bool? IsSelected
		{
			get { return _IsSelected; }
			set
			{
				if (_IsSelected == value) return;
				_IsSelected = value;
				Parent.OnChildrenSelectionChange();
				NotifyPropertyChanged("IsSelected");
			}
		}

		// counters
		public override ulong SelectedSize
		{
			get { return (IsVisible && IsSelected == true) ? _Size : 0; }
		}
		public override uint SelectedCount
		{
			get { return (IsVisible && IsSelected == true) ? 1u : 0u; }
		}
		
		// extension
		public string Ext { get; set; }
		
		// size
		private ulong _Size;
		public override ulong Size
		{
			get { return _Size; }
		}
		
		// date
		private DateTime _Date;
		public DateTime Date
		{
			get { return _Date; }
		}

		// visibility
		private bool _IsVisible;
		public override bool IsVisible
		{
			get { return _IsVisible; }
			set
			{
				if (_IsVisible == value) return;
				_IsVisible = value;

				Parent.OnChildrenVisibilityChange();

				NotifyPropertyChanged("IsVisible");
				NotifyPropertyChanged("SelectedCount");
				NotifyPropertyChanged("SelectedSize");
			}
		}

		// constructor
		public SyncFile(FileInfo info, SyncFolder parent)
		{
			Parent = parent;
			Path = info.FullName;
			Name = info.Name.Replace(info.Extension, "");
			Ext = info.Extension.ToLower();
			_Date = info.LastWriteTime;
			_Size = (ulong)info.Length;
			IsSelected = false;
		}
	}

	// value-converters
	public class DateConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is DateTime)) return null;
			var date = (DateTime)value;

			if (date.Date == DateTime.Today) return "Today " + date.ToString("hh:mmtt");
			else if (date.Date == DateTime.Today.AddDays(-1)) return "Yesterday " + date.ToString("hh:mmtt");
			else if (DateTime.Today.Subtract(date).TotalDays <= 7) return date.ToString("d MMMM, hh:mmtt");
			else if (date.Year == DateTime.Today.Year) return date.ToString("d MMMM");
			else return date.ToString("d MMMM yyyy");
		}
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
	public class DayToDateConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is double)) return null;

			if (frmGetLinks.SyncRoot == null) return null;

			var date = frmGetLinks.SyncRoot.Dates[(int)(double)value];

			var c = new DateConverter().Convert(date, null, null, null);

			return c;
		}
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
	public class SaySizeConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is ulong)) return null;
			var count = (ulong)value;

			//if (count < 1024) return (count).ToString("N0") + " б";
			//else
			if (count < 921600) return ((float)count / 1024f).ToString("F1") + " KB";
			else if (count < 943718400) return ((double)count / 1048576d).ToString("F1") + " MB";
			else return ((double)count / 1073741824d).ToString("F1") + " GB";
		}
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	// treeview template-selector
	public class SyncItemTemplateSelector : DataTemplateSelector
	{
		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			if (item is SyncFolder)

				return (container as FrameworkElement)
					.FindResource("FolderTemplate") as DataTemplate;

			else if (item is SyncFile)

				return (container as FrameworkElement)
					.FindResource("FileTemplate") as DataTemplate;
			
			else 
				return null;
		}
	}

}
